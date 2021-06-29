using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Avalonia.Media;
using Chess.Models;
using static Chess.Models.Message.MessageType;

namespace Chess.ViewModels
{
    public class BoardViewModel
    {
        // We use the same pipe instance for all board views
        public static NamedPipeClientStream message_client = new NamedPipeClientStream("ChessIPC_Messages");

        public ChessBoard board;
        public ChessTile[] tiles = new ChessTile[64];
        public BoardViewModel() : this(String.Empty, true, true) { }
        public BoardViewModel(string gameRecordPath, bool isInteractable, bool displayOverlay)
        {
            if (!message_client.IsConnected)
            {
                try { message_client.Connect(200); }
                catch (TimeoutException) { Console.WriteLine($"Timed out connecting to client in {this}"); };
            }

            string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            board = new ChessBoard(this, fen);
            SetBoardState(fen);

            Rows = new ObservableCollection<ChessRow>();
            Moves = new ObservableCollection<ChessMove>(LoadGame(gameRecordPath));
            Boards = new ObservableCollection<ChessBoard>();
            Boards.Add(new ChessBoard(board));
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
            Status = GameStatus.InProgress;
        }

        public event EventHandler? GameOver;
        protected void OnGameOver() => GameOver?.Invoke(this, new EventArgs());

        public ObservableCollection<ChessRow> Rows { get; private set; }
        public ObservableCollection<ChessMove> Moves { get; private set; }
        public ObservableCollection<ChessBoard> Boards { get; private set; }
        public ObservableCollection<TurnData> Turns { get; private set; }
        public bool IsInteractable { get; set; }
        public bool isWhitesMove { get; protected set; } = true;
        public GameStatus Status { get; private set; }

        private string dirPath;
        private string filePath;
        private bool viewingCurrentMove = true;
        protected int currentBoard = 0;
        protected int currentMove = -1; //currentMove points to the move that has just been made.
                                        //Therefore, if NextMove() is called, currentMove + 1 is the move that should be
                                        //executed (if it exists).

        // Tells the server what move we're making
        public void SendMoveToServer(ChessMove move)
        {
            Message mess_out = new Message(move.data, 2, MakeMoveRequest);
            mess_out.Send(message_client);

            Message mess_in = new Message(MakeMoveReply);
            mess_in.Receive(message_client);
        }

        public void SyncBoardState()
        {
            var state = GetBoardState();
            for (int i = 0; i < 64; i++)
                board[i] = state[i];
        }

        public void SetBoardState(string fen)
        {
            byte[] fen_cstr = new byte[fen.Length + 1];
            int idx = 0;
            foreach (char c in fen)
                fen_cstr[idx++] = (byte)c;
            fen_cstr[idx] =  0;
            Message request = new Message(fen_cstr, (uint)(fen.Length + 1), SetBoardRequest);
            request.Send(message_client);

            Message reply = new Message(SetBoardReply);
            reply.Receive(message_client);
        }

        // Gets the board state from the server
        public ChessPiece[] GetBoardState()
        {
            Message request = new Message(BoardStateRequest);
            request.Send(message_client);

            Message reply = new Message(BoardStateReply);
            reply.Receive(message_client);

            ChessPiece[] state = new ChessPiece[64];
            for (int i = 0; i < 64; i++)
            {
                var arrSeg = new ArraySegment<byte>(reply.Bytes, i * 4, 4);
                int piece = BitConverter.ToInt32(arrSeg);
                state[i] = (ChessPiece)piece;
            }

            return state;
        }

        // Note: This method will probably block for quite a while
        public ChessMove GetServerMove()
        {
            Message request = new Message(new byte[1], 1, BestMoveRequest);
            request.Send(message_client);

            Message reply = new Message(BestMoveReply);
            reply.Receive(message_client);

            var move = new ChessMove(reply.Bytes);

            return move;
        }

        // When user tries to make move
        //    Check legal -> Send to server
        //    Sync board state
        // Get move from server
        // sync board state

        public void PreviousMove()
        {
            if (currentBoard == 0)
                return;
            currentBoard--;

            IsInteractable = false;
            for (int i = 0; i < 64; i++)
                board[i] = Boards[currentBoard][i];
        }

        public void NextMove()
        {
            if (currentBoard == Boards.Count - 1)
                return;
            currentBoard++;
            if (currentBoard == Boards.Count - 1)
                IsInteractable = true;
            for (int i = 0; i < 64; i++)
                board[i] = Boards[currentBoard][i];
        }

        // @@Rework Convert this to an event
        public void OnMoveMade(ChessMove move)
        {
            SyncBoardState();
            Boards.Add(new ChessBoard(board));
            Moves.Add(move);
            AddMoveToTurns(move);
            currentMove++;
            currentBoard++;

            isWhitesMove = !isWhitesMove;

            // After making a move, we check if our opponent is in checkmate
            if (IsInCheckMate(board))
            {
                Status = isWhitesMove ? GameStatus.BlackWon : GameStatus.WhiteWon;
                OnGameOver();
            }
            SaveGame();
        }

        // Sends move to server, syncs board state, gets server move, syncs
        // board state
        public virtual void MakeMove(ChessMove move)
        {
            SendMoveToServer(move);
            OnMoveMade(move);
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
            };
            return move;
        }


        // IsLegalMove takes considerChecks as an argument because a pinned
        // piece can still give check: i.e. a piece that is pinned against the king
        // can still move to kill the enemy king even if doing so leaves its own
        // king in check.
        public bool IsLegalMove(ChessMove move)
        {
            Message mess_out = new Message(move.data, 2, LegalMoveRequest);
            mess_out.Send(message_client);

            Message mess_in = new Message(LegalMoveReply);
            mess_in.Receive(message_client);

            int response = 0;

            // @@Implement Maybe add some exception or something here if the
            // message response type isnt what we expect
            if (mess_in.Type == LegalMoveReply)
                response = BitConverter.ToInt32(mess_in.Bytes);

            return response == 1;
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

        public bool IsInCheckMate(ChessBoard board)
        {
            for (int i = 0; i < 64; i++)
            {
                if (isWhitesMove && (board[i] & ChessPiece.IsWhite) == 0)
                    continue;
                if (!isWhitesMove && (board[i] & ChessPiece.IsWhite) != 0)
                    continue;

                Message request = new Message(BitConverter.GetBytes(i), sizeof(int), GetMovesRequest);
                request.Send(message_client);

                Message reply = new Message(GetMovesReply);
                reply.Receive(message_client);
                // If the message length is non-zero then we've found at least
                // one legal move
                if (reply.Length > 0)
                    return false;
            }
            return true;
        }

        protected void AddMoveToTurns(ChessMove move)
        {
            bool newTurn = Math.Abs(currentMove % 2) == 1;
            if (newTurn)
                Turns.Add(new TurnData()
                {
                    Turn = (currentMove + 3) / 2,
                    WhiteMove = move,
                    BlackMove = default,
                    Fill = Turns.Count % 2 == 1 ?
                        new SolidColorBrush(0xFF2A2A40) :
                        new SolidColorBrush(0xFF323240)
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
