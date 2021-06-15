using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Chess.Models;

namespace Chess.ViewModels
{
    public class BoardViewModel
    {
        public BoardViewModel() : this(String.Empty, true) {}
        public BoardViewModel(string gameRecordPath, bool isInteractable)
        {
            ChessTile[] tiles = ParseFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            Rows = new ObservableCollection<ChessRow>();
            Moves = new ObservableCollection<MoveData>(LoadGame(gameRecordPath));
            Turns = new ObservableCollection<TurnData>();
            dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chess", "GameHistorys");
            filePath = Path.Combine(dirPath, $@"{Guid.NewGuid()}.json");

            int currentTile = 0;
            bool isWhite = true;
            for (int y = 0; y < 8; y++)
            {
                ObservableCollection<ChessTile> rowTiles = new ObservableCollection<ChessTile>();
                for (int x = 0; x < 8; x++)
                {
                    if (isWhite)
                    {
                        tiles[currentTile].Fill = new SolidColorBrush(0xFFD2CACA);
                        tiles[currentTile].HighlightedFill = new SolidColorBrush(0xFFFFABCA);
                        tiles[currentTile].NormalFill = new SolidColorBrush(0xFFD2CACA);
                        rowTiles.Add(tiles[currentTile]);
                    }
                    else
                    {
                        tiles[currentTile].Fill = new SolidColorBrush(0xFF080D24);
                        tiles[currentTile].HighlightedFill = new SolidColorBrush(0xFF480D24);
                        tiles[currentTile].NormalFill = new SolidColorBrush(0xFF080D24);
                        rowTiles.Add(tiles[currentTile]);
                    }
                    currentTile++;
                    isWhite = !isWhite;
                }
                isWhite = !isWhite;
                Rows.Add(new ChessRow { RowTiles = rowTiles });
            }

            IsInteractable = true;
            for (int i = 0; i < Moves.Count; i++)
                NextMove();
            IsInteractable = isInteractable;
        }

        public ObservableCollection<ChessRow> Rows { get; private set; }
        public ObservableCollection<MoveData> Moves { get; private set; }
        public ObservableCollection<TurnData> Turns { get; private set; }
        public bool IsInteractable { get; set; }

        private string dirPath;
        private string filePath;
        private bool viewingCurrentMove = true;
        private int currentMove = -1; //currentMove points to the move that has just been made. Therefore, if NextMove() is called, currentMove + 1 is the move that should be executed (if it exists).

        public void MakeMove(MoveData move) => MakeMove(move,           true,  false, false, true);
        public void PreviousMove()          => MakeMove(new MoveData(), false, true,  false, false);
        public void NextMove()              => MakeMove(new MoveData(), false, false, true,  false);
        private void MakeMove(MoveData move, bool newMove, bool previousMove, bool nextMove, bool saveGame)
        {
            if (!IsInteractable)
                return;
            viewingCurrentMove = currentMove == Moves.Count - 1;
            if (!newMove)
            {
                if (previousMove && currentMove >= 0)
                {
                    move = new MoveData()
                    {
                        OriginFile = Moves[currentMove].TargetFile,
                        OriginRank = Moves[currentMove].TargetRank,
                        TargetFile = Moves[currentMove].OriginFile,
                        TargetRank = Moves[currentMove].OriginRank
                    };
                    currentMove--;
                }
                else if (nextMove && currentMove < Moves.Count - 1)
                    move = Moves[++currentMove];
                else
                    return;
            }
            else
            {
                if (!viewingCurrentMove)
                    return;
                Moves.Add(move);
                AddMoveToTurns(move);
                currentMove++;
            }

            ChessTile newTile = Rows[move.TargetRank].RowTiles[move.TargetFile];
            ChessTile oldTile = Rows[move.OriginRank].RowTiles[move.OriginFile];
            newTile.SetPiece(oldTile.PieceType);
            oldTile.SetPiece(ChessPieceType.None);

            if (saveGame)
                SaveGame();
        }

        private void AddMoveToTurns(MoveData move)
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

        public void SaveGame()
        {
            Directory.CreateDirectory(dirPath);
            using (StreamWriter writer = new StreamWriter(filePath))
                writer.Write(JsonSerializer.Serialize<MoveData[]>(Moves.ToArray<MoveData>()));
        }
        public static MoveData[] LoadGame(string gameRecordPath)
        {
            if (gameRecordPath == String.Empty)
                return new MoveData[0];

            MoveData[] moves;
            using (StreamReader reader = new StreamReader(gameRecordPath))
            {
                string json = reader.ReadToEnd();
                moves = JsonSerializer.Deserialize<MoveData[]>(json);
            }
            return moves;
        }

        public MoveData PiecePositions(ChessTile origin, ChessTile target)
        {
            byte[] positions = new byte[4];
            byte rank = 0;
            byte file = 0;
            foreach (ChessRow row in Rows)
            {
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
            MoveData move = new MoveData()
            {
                OriginFile = positions[0],
                OriginRank = positions[1],
                TargetFile = positions[2],
                TargetRank = positions[3]
            };
            return move;
        }

        public ChessPieceType[] SimplifyBoard()
        {
            ChessPieceType[] board = new ChessPieceType[64];
            int i = 0;
            foreach (ChessRow row in Rows)
            {
                foreach (ChessTile tile in row.RowTiles)
                {
                    board[i] = tile.PieceType;
                    i++;
                }
            }
            return board;
        }

        public bool IsLegalMove(MoveData move)
        {
            ChessPieceType[] board = SimplifyBoard();
            byte originFile = move.OriginFile;
            byte originRank = move.OriginRank;
            byte targetFile = move.TargetFile;
            byte targetRank = move.TargetRank;
            int originPos = originRank * 8 + originFile;
            int targetPos = targetRank * 8 + targetFile;
            ChessPieceType originPieceColor = board[originPos] & ChessPieceType.IsWhite;
            ChessPieceType targetPieceColor = board[targetPos] & ChessPieceType.IsWhite;

            if (board[originPos] == ChessPieceType.None)
                return false;
            // Prevent self capture
            if (ChessPieceType.None != board[targetPos] && (originPieceColor ^ targetPieceColor) == 0)
                return false;

            // Only Knights can jump over pieces
            if ((board[originPos] & ChessPieceType.Knight) == 0)
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
                    if (board[checkPos] != ChessPieceType.None)
                        return false;
                }
            }

            if (0 != (board[originPos] & ChessPieceType.Knight))
            {
                if (Math.Abs(originRank - targetRank) == 2 && Math.Abs(originFile - targetFile) == 1)
                    return true;
                else if (Math.Abs(originRank - targetRank) == 1 && Math.Abs(originFile - targetFile) == 2)
                    return true;
                else
                    return false;
            }

            if ((board[originPos] & (ChessPieceType.Bishop | ChessPieceType.Queen)) != 0)
            {
                if (Math.Abs(targetRank - originRank) == Math.Abs(targetFile - originFile))
                    return true;
            }

            if ((board[originPos] & (ChessPieceType.Castle | ChessPieceType.Queen)) != 0)
            {
                if (targetRank == originRank)
                    return true;
                if (targetFile == originFile)
                    return true;
            }

            if ((board[originPos] & ChessPieceType.Pawn) != 0)
            {
                // Pawns can only move forwards
                if ((board[originPos] & ChessPieceType.IsWhite) != 0)
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

                if (targetFile == originFile && board[targetPos] == ChessPieceType.None)
                    return true;

                // Can't move horizontally
                if (targetRank == originRank)
                    return false;

                // Can take pieces diagonally 1 square in front
                if (board[targetPos] == ChessPieceType.None)
                    return false;
                if ((Math.Abs(targetFile - originFile) == 1 && Math.Abs(targetRank - originRank) == 1))
                    return true;
            }

            if ((board[originPos] & ChessPieceType.King) != 0)
            {
                if (Math.Abs(targetRank - originRank) <= 1 && Math.Abs(targetFile - originFile) <= 1)
                    return true;
            }

            return false;
        }

        private ChessTile[] ParseFen(string fen)
        {
            Regex reg = new Regex(@"\D");
            StringBuilder parsedFen = new StringBuilder();
            int i = 0;
            while (fen[i] != ' ')
            {
                if (fen[i] == '/') ;
                else if (reg.Match(fen, i, 1).Success)
                    parsedFen.Append(fen, i, 1);
                else
                {
                    StringBuilder temp = new StringBuilder();
                    temp.Append(fen[i]);
                    int numEmptySquares = Int32.Parse(temp.ToString());
                    parsedFen.Append('1', numEmptySquares);
                }
                i++;
            }

            var FenToPieceMap = new Dictionary<char, ChessPieceType>
            {
                {'r', ChessPieceType.Castle},
                {'n', ChessPieceType.Knight},
                {'b', ChessPieceType.Bishop},
                {'q', ChessPieceType.Queen},
                {'k', ChessPieceType.King},
                {'p', ChessPieceType.Pawn},
                {'R', ChessPieceType.Castle | ChessPieceType.IsWhite},
                {'N', ChessPieceType.Knight | ChessPieceType.IsWhite},
                {'B', ChessPieceType.Bishop | ChessPieceType.IsWhite},
                {'Q', ChessPieceType.Queen | ChessPieceType.IsWhite},
                {'K', ChessPieceType.King | ChessPieceType.IsWhite},
                {'P', ChessPieceType.Pawn | ChessPieceType.IsWhite},
            };

            ChessTile[] chessTiles = new ChessTile[64];
            for (i = 0; i < 64; i++)
                chessTiles[i] = new ChessTile(FenToPieceMap.GetValueOrDefault(parsedFen[i]));
            return chessTiles;
        }
    }
}
