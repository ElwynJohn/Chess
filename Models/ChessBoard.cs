using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using Chess.ViewModels;
using static Chess.Models.Message.MessageType;

namespace Chess.Models
{
    public class ChessBoard
    {
        public ChessBoard(ChessBoard board)
        {
            IsWhitesMove = board.IsWhitesMove;
            IsPromoting = board.IsPromoting;
            Status = board.Status;
            Boards = board.Boards;
            Moves = board.Moves;
            dirPath = board.dirPath;
            filePath = board.filePath;

            for (int i = 0; i < 64; i++)
                state[i] = board.state[i];
        }

        public ChessBoard(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
            string gameRecordPath = "")
        {
            if (!message_client.IsConnected)
            {
                try { message_client.Connect(200); }
                catch (TimeoutException) { Console.WriteLine
                    ($"Timed out connecting to client in {this}"); };
            }

            Console.WriteLine(fen);
            SetBoardState(fen);

            state = ParseFen(fen);
            dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chess", "GameHistorys");
            filePath = Path.Combine(dirPath, $@"{Guid.NewGuid()}.json");

            Moves = new ObservableCollection<ChessMove>(LoadGame(gameRecordPath));
            Boards = new ObservableCollection<ChessBoard>();
            Boards.Add(new ChessBoard(this));
            Status = GameStatus.InProgress;
        }

        public event EventHandler? GameOver;
        protected void OnGameOver() => GameOver?.Invoke(this, new EventArgs());
        public event BoardUpdateEventHandler? Update;
        protected void OnUpdate(ChessMove move) => Update?.Invoke
            (this, new BoardUpdateEventArgs(this, move));

        // We use the same pipe instance for all board views
        public static NamedPipeClientStream message_client = new NamedPipeClientStream("ChessIPC_Messages");
        public bool IsWhitesMove { get; private set; } = true;
        public bool IsPromoting { get; protected set; }
        public GameStatus Status { get; private set; } = GameStatus.InProgress;
        public ObservableCollection<ChessBoard> Boards { get; private set; }
        public ObservableCollection<ChessMove> Moves { get; private set; }
        /// <summary>64 based representation of the board</summary>
        private ChessPiece[] state = new ChessPiece[64];
        /// <summary>Get or set the board state by 64 (default) or 0x88 based
        /// index.</summary>
        public ChessPiece this[int pos, bool pos0x88 = false]
        {
            get => state[pos0x88 ? Pos64(pos) : pos];
            private set => state[pos0x88 ? Pos64(pos) : pos] = value;
        }
        /// <summary>Get or set the board by file-rank.</summary>
        public ChessPiece this[int file, int rank]
        {
            get => state[Pos64(file, rank)];
            private set { state[Pos64(file, rank)] = value; }
        }

        private string dirPath;
        private string filePath;


        // Sends move to server, syncs board state, gets server move, syncs
        // board state
        public virtual void MakeMove(ChessMove move, bool serverMove)
        {
            if (!serverMove)
                SendMoveToServer(move);

            SyncBoardState();

            if ((state[move.To] & ChessPiece.Pawn) != 0 && Rank(move.To) == (IsWhitesMove ? 0 : 7))
                IsPromoting = true;
            else
            {
                // After making a move, we check if our opponent is in checkmate
                IsWhitesMove = !IsWhitesMove;
                if (IsInCheckMate(this))
                {
                    Status = IsWhitesMove ? GameStatus.BlackWon : GameStatus.WhiteWon;
                    OnGameOver();
                }
                SaveGame();
            }
            Boards.Add(new ChessBoard(this));
            Moves.Add(move);
            OnUpdate(move);
        }

        public void PromoteTo(ChessPiece promoteTo)
        {
            RequestPromotion(promoteTo);
            IsPromoting = false;
            SyncBoardState();
            IsWhitesMove = !IsWhitesMove;
            if (IsInCheckMate(this))
            {
                Status = IsWhitesMove ? GameStatus.BlackWon : GameStatus.WhiteWon;
                OnGameOver();
            }
            SaveGame();
            OnUpdate(null);
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

        public static Dictionary<char, ChessPiece> FenToPieceMap = new Dictionary<char, ChessPiece>
        {
            {'r', ChessPiece.Castle},
            {'n', ChessPiece.Knight},
            {'b', ChessPiece.Bishop},
            {'q', ChessPiece.Queen},
            {'k', ChessPiece.King},
            {'p', ChessPiece.Pawn},
            {'R', ChessPiece.Castle | ChessPiece.IsWhite},
            {'N', ChessPiece.Knight | ChessPiece.IsWhite},
            {'B', ChessPiece.Bishop | ChessPiece.IsWhite},
            {'Q', ChessPiece.Queen | ChessPiece.IsWhite},
            {'K', ChessPiece.King | ChessPiece.IsWhite},
            {'P', ChessPiece.Pawn | ChessPiece.IsWhite},
        };
        public static ChessPiece[] ParseFen(string fen)
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

        public static int Pos64(int pos0x88)
        {
            return (pos0x88 + (pos0x88 & 7)) >> 1;
        }

        public static int Pos88(int pos64)
        {
            return pos64 + (pos64 & ~7);
        }

        public static int Pos64(int file, int rank)
        {
            return file + 8 * rank;
        }

        public static int Pos88(int file, int rank)
        {
            return file + 16 * rank;
        }

        public static int File(int pos, bool pos0x88 = false)
        {
            return pos % (pos0x88 ? 16 : 8);
        }

        public static int Rank(int pos, bool pos0x88 = false)
        {
            return pos / (pos0x88 ? 16 : 8);
        }

        public override string ToString()
        {
            StringBuilder rv = new StringBuilder();
            for (int i = 0; i < 64; i++)
            {
                int file = File(i);
                char c = FenToPieceMap.FirstOrDefault(x => x.Value.Equals(this[i])).Key;
                rv.Append(c != '\0' ? c : '0');
                if (file == 7)
                    rv.Append("\n");
                else
                    rv.Append(" ");
            }
            rv.Remove(rv.Length - 1, 1); // Get rid of trailing newline;
            return rv.ToString();
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

        // Gets the board state from the server
        private ChessPiece[] GetBoardState()
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

        // Tells the server what move we're making
        private void SendMoveToServer(ChessMove move)
        {
            Message mess_out = new Message(move.data, 2, MakeMoveRequest);
            mess_out.Send(message_client);

            Message mess_in = new Message(MakeMoveReply);
            mess_in.Receive(message_client);
        }
        private void SyncBoardState()
        {
            var state = GetBoardState();
            for (int i = 0; i < 64; i++)
                this[i] = state[i];
        }
        private void SetBoardState(string fen)
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
        private void RequestPromotion(ChessPiece piece)
        {
            Message request = new Message(BitConverter.GetBytes((int)piece), sizeof(int), PromotionRequest);
            request.Send(message_client);

            Message reply = new Message(PromotionReply);
            reply.Receive(message_client);
        }

        private bool IsInCheckMate(ChessBoard board)
        {
            for (int i = 0; i < 64; i++)
            {
                if (board[i] == ChessPiece.None)
                    continue;
                if (IsWhitesMove && (board[i] & ChessPiece.IsWhite) == 0)
                    continue;
                if (!IsWhitesMove && (board[i] & ChessPiece.IsWhite) != 0)
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


    }
}
