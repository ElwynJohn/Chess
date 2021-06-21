using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.ComponentModel;
using Chess.Models;

namespace Chess.ViewModels
{
    public class BoardViewModel
    {
        public ChessBoard board;
        public ChessTile[] tiles = new ChessTile[64];
        public BoardViewModel() : this(String.Empty, true, true) { }
        public BoardViewModel(string gameRecordPath, bool isInteractable, bool displayOverlay)
        {
            try { client.Connect(100); }
            catch (TimeoutException) { }

            board = new ChessBoard(this, "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            Rows = new ObservableCollection<ChessRow>();
            Moves = new ObservableCollection<ChessMove>(LoadGame(gameRecordPath));
            Turns = new ObservableCollection<TurnData>();
            dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chess", "GameHistorys");
            filePath = Path.Combine(dirPath, $@"{Guid.NewGuid()}.json");

            for (int y = 0; y < 8; y++)
            {
                ObservableCollection<ChessTile> rowTiles = new ObservableCollection<ChessTile>();
                for (int x = 0; x < 8; x++)
                {
                    int currentTile = y * 8 + x;
                    tiles[currentTile] = new ChessTile(board, currentTile, displayOverlay);
                    rowTiles.Add(tiles[currentTile]);
                }
                Rows.Add(new ChessRow { RowTiles = rowTiles });
            }

            IsInteractable = true;
            for (int i = 0; i < Moves.Count; i++)
                NextMove();
            IsInteractable = isInteractable;
        }

        public ObservableCollection<ChessRow> Rows { get; private set; }
        public ObservableCollection<ChessMove> Moves { get; private set; }
        public ObservableCollection<TurnData> Turns { get; private set; }
        public bool IsInteractable { get; set; }
        public bool isWhitesMove { get; private set; } = true;

        public NamedPipeClientStream client = new NamedPipeClientStream("ChessIPC");
        private string dirPath;
        private string filePath;
        private bool viewingCurrentMove = true;
        private int currentMove = -1; //currentMove points to the move that has just been made.
                                      //Therefore, if NextMove() is called, currentMove + 1 is the move that should be
                                      //executed (if it exists).
        private bool gameOver = false;

        public void MakeMove(ChessMove move) => MakeMove(move, true, false, false, true);
        public void PreviousMove() => MakeMove(new ChessMove(), false, true, false, false);
        public void NextMove() => MakeMove(new ChessMove(), false, false, true, false);
        private void MakeMove(ChessMove move, bool newMove, bool previousMove, bool nextMove, bool saveGame)
        {
            if (!IsInteractable)
                return;
            viewingCurrentMove = currentMove == Moves.Count - 1;

            if (newMove)
            {
                if (!viewingCurrentMove)
                    return;
                Moves.Add(move);
                AddMoveToTurns(move);
                currentMove++;
                isWhitesMove = !isWhitesMove;
            }
            else if (previousMove && currentMove >= 0)
            {
                move = new ChessMove(Moves[currentMove].data);
                currentMove--;
            }
            else if (nextMove && currentMove < Moves.Count - 1)
                move = Moves[++currentMove];
            else
                return;

            board[move.To] = board[move.From];
            board[move.From] = ChessPiece.None;

            ChessMove testmove = new ChessMove(move.From, move.To);

            if (newMove && IsInCheckMate(SimplifyBoard()))
            {
                gameOver = true;
                Console.WriteLine("Game Over\n{0} won", isWhitesMove ? "black" : "white");
            }
            if (saveGame)
                SaveGame();

            if (client.IsConnected)
            {
                if (newMove)
                {
                    byte[] buf = new byte[2];
                    client.Write(move.data, 0, 2);
                    client.Read(buf, 0, 2);
                    ChessMove server_move = new ChessMove(buf);
                    Console.WriteLine("Got move: {0}", server_move);
                    board[server_move.To] = board[server_move.From];
                    board[server_move.From] = ChessPiece.None;
                    Moves.Add(server_move);
                    AddMoveToTurns(server_move);
                    currentMove++;
                }
            }
            Console.WriteLine($"From: {testmove.From}\t To: {testmove.To}");
            Console.WriteLine(board);
        }

        public ChessMove PiecePositions(ChessTile origin, ChessTile target)
        {
            byte[] positions = new byte[4];
            byte rank = 0;
            byte file = 0;
            foreach (ChessRow row in Rows)
            {
                if (row.RowTiles == null)
                    continue;
                foreach (ChessTile tile in row.RowTiles)
                {
                    if (tile == origin)
                    {
                        positions[0] = file;
                        positions[1] = rank;
                    }
                    if (tile == target)
                    {
                        positions[2] = file;
                        positions[3] = rank;
                    }
                    file++;
                }
                file = 0;
                rank++;
            }
            ChessMove move = new ChessMove()
            {
                From = ChessBoard.Pos64(positions[0], positions[1]),
                To = ChessBoard.Pos64(positions[2], positions[3]),
                /* OriginFile = positions[0], */
                /* OriginRank = positions[1], */
                /* TargetFile = positions[2], */
                /* TargetRank = positions[3] */
            };
            return move;
        }


        // IsLegalMove takes considerChecks as an argument because a pinned
        // piece can still give check: i.e. a piece that is pinned against the king
        // can still move to kill the enemy king even if doing so leaves its own
        // king in check.
        public bool IsLegalMove(ChessMove move) => IsLegalMove(SimplifyBoard(), (byte)move.From, (byte)move.To);
        private bool IsLegalMove(ChessPiece[] board, byte originPos, byte targetPos)
        {
            byte originFile = (byte)(originPos % 8);
            byte originRank = (byte)(originPos / 8);
            byte targetFile = (byte)(targetPos % 8);
            byte targetRank = (byte)(targetPos / 8);

            return IsLegalMove(board, originFile, originRank, targetFile, targetRank, true, this.isWhitesMove);
        }
        private bool IsLegalMove(ChessPiece[] board, byte originFile, byte originRank, byte targetFile, byte targetRank, bool considerChecks, bool _isWhitesMove)
        {
            int originPos = originRank * 8 + originFile;
            int targetPos = targetRank * 8 + targetFile;
            ChessPiece originPieceColor = board[originPos] & ChessPiece.IsWhite;
            ChessPiece targetPieceColor = board[targetPos] & ChessPiece.IsWhite;

            if (board[originPos] == ChessPiece.None)
                return false;
            //Prevent white's pieces being moved on black's move & vice versa
            if ((!_isWhitesMove && ((board[originPos] & ChessPiece.IsWhite) != 0)) ||
                (_isWhitesMove && ((board[originPos] & ChessPiece.IsWhite) == 0)))
                return false;
            // Prevent self capture
            if (ChessPiece.None != board[targetPos] && (originPieceColor ^ targetPieceColor) == 0)
                return false;

            // Only Knights can jump over pieces
            if ((board[originPos] & ChessPiece.Knight) == 0)
            {
                var dirVec = new int[]
                {
                    targetFile - originFile,
                    targetRank - originRank,
                };

                for (int i = 0; i < 2; i++)
                    if (dirVec[i] != 0)
                        dirVec[i] /= Math.Abs(dirVec[i]);

                int checkPos = originPos;
                int checkFile = originFile;
                int checkRank = originRank;
                while (true)
                {
                    checkFile += dirVec[0];
                    checkRank += dirVec[1];
                    checkPos = checkFile + 8 * checkRank;
                    if (checkPos < 0 || checkPos >= board.GetLength(0))
                        return false;
                    if (checkPos == targetPos)
                        break;
                    if (board[checkPos] != ChessPiece.None)
                        return false;
                }
            }
            if (considerChecks)
            {
                ChessPiece[] boardAfterMove = new ChessPiece[64];
                for (int i = 0; i < 64; i++)
                    boardAfterMove[i] = board[i];
                boardAfterMove[originPos] = ChessPiece.None;
                boardAfterMove[targetPos] = board[originPos];
                if (PositionOfChecker(boardAfterMove) != Byte.MaxValue)
                    return false;
            }

            if (0 != (board[originPos] & ChessPiece.Knight))
            {
                if (Math.Abs(originRank - targetRank) == 2 && Math.Abs(originFile - targetFile) == 1)
                    return true;
                else if (Math.Abs(originRank - targetRank) == 1 && Math.Abs(originFile - targetFile) == 2)
                    return true;
                else
                    return false;
            }

            if ((board[originPos] & (ChessPiece.Bishop | ChessPiece.Queen)) != 0)
            {
                if (Math.Abs(targetRank - originRank) == Math.Abs(targetFile - originFile))
                    return true;
            }

            if ((board[originPos] & (ChessPiece.Castle | ChessPiece.Queen)) != 0)
            {
                if (targetRank == originRank)
                    return true;
                if (targetFile == originFile)
                    return true;
            }

            if ((board[originPos] & ChessPiece.Pawn) != 0)
            {
                // Pawns can only move forwards
                if ((board[originPos] & ChessPiece.IsWhite) != 0)
                {
                    if (targetRank > originRank)
                        return false;
                }
                else
                {
                    if (targetRank < originRank)
                        return false;
                }

                // Allow moving 2 squares at the start
                if (originRank == 1 || originRank == 6)
                {
                    if (Math.Abs(targetRank - originRank) > 2)
                        return false;
                }
                else
                {
                    if (Math.Abs(targetRank - originRank) > 1)
                        return false;
                }

                if (targetFile == originFile && board[targetPos] == ChessPiece.None)
                    return true;

                // Can't move horizontally
                if (targetRank == originRank)
                    return false;

                // Can take pieces diagonally 1 square in front
                if (board[targetPos] == ChessPiece.None)
                    return false;
                if ((Math.Abs(targetFile - originFile) == 1 && Math.Abs(targetRank - originRank) == 1))
                    return true;
            }

            if ((board[originPos] & ChessPiece.King) != 0)
            {
                if (Math.Abs(targetRank - originRank) <= 1 && Math.Abs(targetFile - originFile) <= 1)
                    return true;
            }

            return false;
        }

        public static ChessMove[] LoadGame(string gameRecordPath)
        {
            if (gameRecordPath == String.Empty)
                return new ChessMove[0];

            ChessMove[]? moves;
            using (StreamReader reader = new StreamReader(gameRecordPath))
            {
                string json = reader.ReadToEnd();
                moves = JsonSerializer.Deserialize<ChessMove[]>(json);
            }
            return moves != null ? moves : new ChessMove[0];
        }
        private void SaveGame()
        {
            Directory.CreateDirectory(dirPath);
            using (StreamWriter writer = new StreamWriter(filePath))
                writer.Write(JsonSerializer.Serialize<ChessMove[]>(Moves.ToArray<ChessMove>()));
        }

        private bool IsInCheckMate(ChessPiece[] board)
        {
            LinkedList<byte> testKingMoves = new LinkedList<byte>();
            LinkedList<byte> testLegalBlockingMoves = new LinkedList<byte>();
            LinkedList<byte> testBlockingMoves = new LinkedList<byte>();
            bool testIsInMate = true;
            // The above variables can be removed. They are here to help
            // with debugging if a bug is found.
            //
            // testIsInMate can be removed and replaced by returning
            // early (which would save CPU time).

            // Check if the king is in check
            byte attackingPiecePos = PositionOfChecker(board);
            if (attackingPiecePos == Byte.MaxValue)
                return false;

            // Check if the king can move
            byte king = FindKing(board, isWhitesMove);
            sbyte[] kingMoves = new sbyte[8] { -9, -8, -7, -1, 1, -7, 8, 9 };
            for (int i = 0; i < 8; i++)
            {
                sbyte starget = (sbyte)(king + kingMoves[i]);
                if (starget < 0 || starget > 63)
                    continue;
                byte target = (byte)starget;

                if (IsLegalMove(board, king, target))
                {
                    testKingMoves.AddLast(target);
                    testIsInMate = false;
                }
            }

            LinkedList<byte> friendlyPiecePositions = new LinkedList<byte>();
            for (byte i = 0; i < 64; i++)
            {
                if (isWhitesMove && ((board[i] & ChessPiece.IsWhite) != 0))
                    friendlyPiecePositions.AddLast(i);
                else if (!isWhitesMove && ((board[i] & ChessPiece.IsWhite) == 0))
                    friendlyPiecePositions.AddLast(i);
            }

            // Check if the checker can be captured. (See Note A)
            for (byte i = 0; i < friendlyPiecePositions.Count; i++)
            {
                if (IsLegalMove(board, friendlyPiecePositions.ElementAt(i), attackingPiecePos))
                {
                    testLegalBlockingMoves.Remove(attackingPiecePos);
                    testLegalBlockingMoves.AddLast(attackingPiecePos);
                    testIsInMate = false;
                }
            }

            // King can't move and knight cant be killed so it's checkmate
            if ((board[attackingPiecePos] & ChessPiece.Knight) != 0)
                return true;

            // Check if the checker can be blocked:
            // First, store all positions that would block a checker.
            // Second, check to see if any friendly piece can move to one of these
            // positions. (See Note A)
            byte originFile = (byte)(king % 8);
            byte originRank = (byte)(king / 8);
            byte targetFile = (byte)(attackingPiecePos % 8);
            byte targetRank = (byte)(attackingPiecePos / 8);
            var dirVec = new int[] { targetFile - originFile, targetRank - originRank, };
            for (int i = 0; i < 2; i++)
                if (dirVec[i] != 0)
                    dirVec[i] /= Math.Abs(dirVec[i]);
            byte fileCheckPos = originFile;
            byte rankCheckPos = originRank;
            LinkedList<byte> blockingPositions = new LinkedList<byte>();
            while (true)
            {
                fileCheckPos = (byte)(fileCheckPos + dirVec[0]);
                rankCheckPos = (byte)(rankCheckPos + dirVec[1]);
                if (fileCheckPos == targetFile)
                {
                    break;
                }
                testBlockingMoves.Remove((byte)(rankCheckPos * 8 + fileCheckPos));
                testBlockingMoves.AddLast((byte)(rankCheckPos * 8 + fileCheckPos));
                blockingPositions.AddLast((byte)(rankCheckPos * 8 + fileCheckPos));
            }

            for (int i = 0; i < friendlyPiecePositions.Count; i++)
            {
                for (int j = 0; j < blockingPositions.Count; j++)
                {
                    if (IsLegalMove(board, friendlyPiecePositions.ElementAt(i), blockingPositions.ElementAt(j)))
                    {
                        testLegalBlockingMoves.Remove(blockingPositions.ElementAt(j));
                        testLegalBlockingMoves.AddLast(blockingPositions.ElementAt(j));
                        testIsInMate = false;
                    }
                }
            }

            // Note A: As a by-product of using IsLegalMove, we also check if
            // this move actually takes the king out of check. This deals
            // with situations where there is more than one checker.

            if (testIsInMate)
                return true;
            else
            {
                /*
                Console.WriteLine("\nChecker at: {0},{1}", attackingPiecePos % 8 + 1, 8 - (attackingPiecePos / 8));
                Console.WriteLine("Legal King Moves:");
                foreach (byte pos in testKingMoves)
                {
                    Console.WriteLine("{0},{1}", pos % 8 + 1, 8 - (pos / 8));
                }
                Console.WriteLine("Positions that would break a checker's line of sight (this position might not break the check (e.g. if there are two checkers)):");
                foreach (byte pos in testBlockingMoves)
                {
                    Console.WriteLine("{0},{1}", pos % 8 + 1, 8 - (pos / 8));
                }
                Console.WriteLine("Positions that can be reached and would break check:");
                foreach (byte pos in testLegalBlockingMoves)
                {
                    Console.WriteLine("{0},{1}", pos % 8 + 1, 8 - (pos / 8));
                }
                Console.WriteLine();
                */
                return false;
            }

            return true;
        }
        // Returns Byte.MaxValue when no piece is checking the king
        private byte PositionOfChecker(ChessPiece[] board)
        {
            byte king = FindKing(board, isWhitesMove);
            for (byte i = 0; i < 64; i++)
            {
                if (board[i] == ChessPiece.None)
                    continue;
                // pieces cannot check their own king
                if (isWhitesMove && (board[i] & ChessPiece.IsWhite) != 0)
                    continue;
                if (!isWhitesMove && (board[i] & ChessPiece.IsWhite) == 0)
                    continue;

                byte originFile = (byte)(i % 8);
                byte originRank = (byte)(i / 8);
                byte kingFile = (byte)(king % 8);
                byte kingRank = (byte)(king / 8);
                if (IsLegalMove(board, originFile, originRank, kingFile, kingRank, false, !isWhitesMove))
                    return i;
            }
            return Byte.MaxValue;
        }
        private byte FindKing(ChessPiece[] board, bool isWhite)
        {
            for (byte i = 0; i < 64; i++)
            {
                if ((board[i] & ChessPiece.King) == 0)
                    continue;
                if (isWhite && (board[i] & ChessPiece.IsWhite) != 0)
                    return i;
                if (!isWhite && (board[i] & ChessPiece.IsWhite) == 0)
                    return i;
            }
            return default;
        }

        private ChessPiece[] SimplifyBoard()
        {
            ChessPiece[] board = new ChessPiece[64];
            int i = 0;
            foreach (ChessRow row in Rows)
            {
                if (row.RowTiles == null)
                    continue;
                foreach (ChessTile tile in row.RowTiles)
                {
                    board[i] = tile.PieceType;
                    i++;
                }
            }
            return board;
        }

        private void AddMoveToTurns(ChessMove move)
        {
            bool newTurn = Math.Abs(currentMove % 2) == 1;
            if (newTurn)
                Turns.Add(new TurnData()
                {
                    Turn = (currentMove + 3) / 2,
                    WhiteMove = move,
                    BlackMove = default,
                    Fill = Turns.Count % 2 == 1 ?
                        new SolidColorBrush(0xFF323240) :
                        new SolidColorBrush(0xFF2A2A40)
                });
            else
            {
                TurnData temp = Turns[Turns.Count - 1];
                Turns[Turns.Count - 1] = new TurnData()
                {
                    Turn = temp.Turn,
                    WhiteMove = temp.WhiteMove,
                    BlackMove = move,
                    Fill = temp.Fill
                };
            }
        }

        public ChessPiece[] ParseFen(string fen)
        {
            StringBuilder parsedFen = new StringBuilder();
            foreach (char c in fen)
            {
                if (c == '/' || c == ' ')
                    continue;

                if (System.Char.IsDigit(c))
                    parsedFen.Append('0', Int32.Parse(c.ToString()));
                else
                    parsedFen.Append(c);
            }

            var pieces = new ChessPiece[64];
            for (int i = 0; i < 64; i++)
                pieces[i] = ChessBoard.FenToPieceMap.GetValueOrDefault(parsedFen[i]);
            return pieces;
        }
    }
}
