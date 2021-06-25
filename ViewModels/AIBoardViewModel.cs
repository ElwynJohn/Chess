using Chess.Models;
using System.IO.Pipes;
using System;
using System.Threading.Tasks;

namespace Chess.ViewModels
{
    public class AIBoardViewModel : BoardViewModel
    {
        public AIBoardViewModel() : this(String.Empty, true, true) { }
        public AIBoardViewModel(string gameRecordPath, bool isInteractable, bool displayOverlay)
            : base(gameRecordPath, isInteractable, displayOverlay)
        {
            try { client.Connect(100); }
            catch (TimeoutException) { }
        }

        public NamedPipeClientStream client = new NamedPipeClientStream("ChessIPC");

        public override void MakeMove(ChessMove move)
        {
            base.MakeMove(move);

            if (client.IsConnected)
            {
                byte[] buf = new byte[2];
                client.Write(move.data, 0, 2);

                Task t = client.ReadAsync(buf, 0, 2);
                // We shouldn't be able to do anything while the server is thinking
                // @@FIXME This seems to have weird behaviour if you try to move
                // a piece while the server is thinking
                IsInteractable = false;

                t.ContinueWith((readTask) =>
                {
                    ChessMove server_move = new ChessMove(buf);
                    Console.WriteLine("Got move: {0}", server_move);
                    board[server_move.To] = board[server_move.From];
                    board[server_move.From] = ChessPiece.None;
                    isWhitesMove = !isWhitesMove;
                    Moves.Add(server_move);
                    AddMoveToTurns(server_move);
                    currentMove++;
                    IsInteractable = true;
                });
            }
            Console.WriteLine(board);
        }
    }
}
