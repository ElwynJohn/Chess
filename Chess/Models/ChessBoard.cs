using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            LastMove = board.LastMove;
            IsBlackInCheck = board.IsBlackInCheck;
            IsWhiteInCheck = board.IsWhiteInCheck;
            dirPath = board.dirPath;
            filePath = board.filePath;

            PiecesCaptured = new List<ChessPiece>(board.PiecesCaptured.Count);
            for (int i = 0; i < board.PiecesCaptured.Count; i++)
                PiecesCaptured.Add(board.PiecesCaptured[i]);
            for (int i = 0; i < 64; i++)
                state[i] = board[i];
        }

        // Starting fen: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        public ChessBoard(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
            string gameRecordPath = "")
        {
            SetBoardState(fen);
            state = ParseFen(fen);
            dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chess", "GameHistorys");
            filePath = Path.Combine(dirPath, $@"{Guid.NewGuid()}");

            PiecesCaptured = new List<ChessPiece>(32);
            Moves = new ObservableCollection<ChessMove>(LoadGame(gameRecordPath));
            Boards = new ObservableCollection<ChessBoard>();
            Boards.Add(new ChessBoard(this));
            Status = GameStatus.InProgress;
        }

        public event BoardUpdateEventHandler? Update;
        protected void OnUpdate() => Update?.Invoke
            (this, new BoardUpdateEventArgs(this, null, ChessPiece.None));
        protected void OnUpdate(BoardUpdateEventArgs args) => Update?.Invoke(this, args);

        // We use the same pipe instance for all board views
        public static NamedPipeClientStream message_client = new NamedPipeClientStream("ChessIPC_Messages");
        public List<ChessPiece> PiecesCaptured { get; private set; }
        public bool IsWhitesMove { get; private set; } = true;
        public bool IsPromoting { get; protected set; } = false;
        private GameStatus status = GameStatus.InProgress;
        public GameStatus Status
        {
            get => status;
            set
            {
                status = value;
                OnUpdate();
            }
        }
        public ObservableCollection<ChessBoard> Boards { get; private set; }
        // Moves is a collection of all moves made in this game, both in the
        // past and future.
        public ObservableCollection<ChessMove> Moves { get; private set; }
        // LastMove is the move that was made to get to this board state
        public ChessMove? LastMove { get; private set; } = null;
        public bool IsWhiteInCheck { get; private set; } = false;
        public bool IsBlackInCheck { get; private set; } = false;
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
        public string filePath;


        public virtual void MakeMove(ChessMove move, bool serverMove)
        {
            if (Status != GameStatus.InProgress)
                return;

            ChessPiece pieceTaken = state[move.To];
            if (pieceTaken != ChessPiece.None)
                PiecesCaptured.Add(state[move.To]);

            if (!serverMove)
                SendMoveToServer(move);
            LastMove = move;

            SyncBoardState();

            bool[] isInCheck = IsInCheck();
            IsWhiteInCheck = isInCheck[0];
            IsBlackInCheck = isInCheck[1];
            if ((state[move.To] & ChessPiece.Pawn) != 0 && Rank(move.To) == (IsWhitesMove ? 0 : 7))
                IsPromoting = true;
            else
            {
                // After making a move, we check if our opponent is in checkmate
                IsWhitesMove = !IsWhitesMove;
                if (IsInStaleMate())
                    Status = GameStatus.Draw;
                if (IsInCheckMate())
                    Status = IsWhitesMove ? GameStatus.BlackWon : GameStatus.WhiteWon;
                SaveGame();
            }
            Boards.Add(new ChessBoard(this));
            Moves.Add(move);
            OnUpdate(new BoardUpdateEventArgs(this, move, pieceTaken));
        }

        public void PromoteTo(ChessPiece promoteTo)
        {
            if (Status != GameStatus.InProgress)
                return;

            RequestPromotion(promoteTo);
            IsPromoting = false;
            SyncBoardState();
            IsWhitesMove = !IsWhitesMove;
            bool[] isInCheck = IsInCheck();
            IsWhiteInCheck = isInCheck[0];
            IsBlackInCheck = isInCheck[1];
            if (IsInStaleMate())
                Status = GameStatus.Draw;
            if (IsInCheckMate())
                Status = IsWhitesMove ? GameStatus.BlackWon : GameStatus.WhiteWon;
            Boards.RemoveAt(Boards.Count - 1);
            Boards.Add(new ChessBoard(this));
            SaveGame();
            OnUpdate();
        }

        public bool IsLegalMove(ChessMove move)
        {
            Message mess = new Message(move.data, 2, LegalMoveRequest);
            mess.Send();
            mess.Receive();

            return mess.Bytes.ToInt32() == 1;
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
            using (StreamWriter writer = new StreamWriter($"{filePath}.json"))
                writer.Write(JsonSerializer.Serialize<ChessMove[]>(Moves.ToArray<ChessMove>()));

            using (var bitmap = new Bitmap(160, 160))
            {
                using (var canvas = Graphics.FromImage(bitmap))
                {
                    canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    var whiteSquare = new SolidBrush(Color.FromArgb(0xFF,0xD2,0xCA,0xCA));
                    var blackSquare = new SolidBrush(Color.FromArgb(0xFF,0x38,0x3D,0x64));
                    int size = 20;
                    int pieceSize = 18;
                    for (int i = 0; i < 64; i++)
                    {
                        int file = i % 8;
                        int rank = i / 8;
                        int xPos = file * 20;
                        int yPos = rank * 20;
                        bool isWhite = (rank % 2 == 1) ? (i % 2 == 1) : (i % 2 == 0);
                        if (isWhite)
                            canvas.FillRectangle(whiteSquare, xPos, yPos, size, size);
                        else
                            canvas.FillRectangle(blackSquare, xPos, yPos, size, size);

                        var piecePath = ChessTile.PieceToAssetMap.GetValueOrDefault(state[i]);
                        if (piecePath == null)
                            continue;
                        using (var pieceBm = new Bitmap(piecePath))
                        {
                            // Add one to xPos and yPos because pieceSize is smaller than size.
                            // Adding one centres the piece in its containing square.
                            canvas.DrawImage(pieceBm, xPos + 1, yPos + 1, pieceSize, pieceSize);
                        }
                    }
                    canvas.Save();
                }
                bitmap.Save($"{filePath}.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        // Gets the board state from the server
        private ChessPiece[] GetBoardState()
        {
            Message mess = new Message(BoardStateRequest);
            mess.Send();
            mess.Receive();

            ChessPiece[] state = new ChessPiece[64];
            for (int i = 0; i < 64; i++)
            {
                var arrSeg = new ArraySegment<byte>(mess.Bytes, i * 4, 4);
                int piece = BitConverter.ToInt32(arrSeg);
                state[i] = (ChessPiece)piece;
            }

            return state;
        }
        // Tells the server what move we're making
        private void SendMoveToServer(ChessMove move)
        {
            Message mess = new Message(move.data, 2, MakeMoveRequest);
            mess.Send();
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

            Message mess = new Message(fen_cstr, (fen.Length + 1), SetBoardRequest);
            mess.Send();
        }
        private void RequestPromotion(ChessPiece piece)
        {
            Message mess = new Message(BitConverter.GetBytes((int)piece), sizeof(int), PromotionRequest);
            mess.Send();
        }

        public int FindKing(bool isWhite)
        {
            int king_pos = -1;
            for (int i = 0; i < 64; i++)
                if (this[i] == (ChessPiece.King | (isWhite ? ChessPiece.IsWhite : 0)))
                {
                    king_pos = i;
                    break;
                }

            return king_pos;
        }

        // @@REWORK: only the player whose turn it is, can be in check. Therefore,
        // IsInCheck should probably not calculate IsInCheck for white and black.
        // The 1st element of the return array defines white, the 2nd defines black
        public bool[] IsInCheck()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Message mess = new Message(IsInCheckRequest);
            mess.Send();
            mess.Receive();

            bool[] rv = new bool[2];
            if (mess.Length != rv.Length)
            {
                // @@Rework: should we throw an exception here?
                Logger.EWrite($"Message of type {mess.Type} should be of length {rv.Length} but is of length {mess.Length}.");
                return rv;
            }
            for (int i = 0; i < rv.Length; i++)
            {
                rv[i] = mess.Bytes[i] != 0 ? true : false;
            }
            sw.Stop();
            Logger.DWrite($"IsInCheck took {sw.ElapsedMilliseconds}ms.");

            return rv;
        }

        private bool IsInCheckMate()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            byte[] messagePayload = new byte[1] { IsWhitesMove ? (byte)1 : (byte)0 };
            Message mess = new Message(messagePayload, 1, IsInCheckmateRequest);
            mess.Send();
            mess.Receive();

            if (mess.Length != 1)
            {
                // @@Rework: should we throw an exception here?
                Logger.EWrite($"Message of type {mess.Type} should be of length 1 but is of length {mess.Length}.");
                return false;
            }

            sw.Stop();
            Logger.DWrite($"IsInCheckMate took {sw.ElapsedMilliseconds}ms.");

            return mess.Bytes[0] == 0 ? false : true;
        }

        private bool IsInStaleMate()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            byte[] messagePayload = new byte[1] { IsWhitesMove ? (byte)1 : (byte)0 };
            Message mess = new Message(messagePayload, 1, IsInStalemateRequest);
            mess.Send();
            mess.Receive();

            if (mess.Length != 1)
            {
                // @@Rework: should we throw an exception here?
                Logger.EWrite($"Message of type {mess.Type} should be of length 1 but is of length {mess.Length}.");
                return false;
            }
            sw.Stop();
            Logger.DWrite($"IsInStaleMate took {sw.ElapsedMilliseconds}ms.");

            return mess.Bytes[0] == 0 ? false : true;
        }
    }
}
