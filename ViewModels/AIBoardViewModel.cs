using Chess.Models;
using System.IO.Pipes;
using System;

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
                client.Read(buf, 0, 2);
                ChessMove server_move = new ChessMove(buf);
                Console.WriteLine("Got move: {0}", server_move);
                board[server_move.To] = board[server_move.From];
                board[server_move.From] = ChessPiece.None;
                isWhitesMove = !isWhitesMove;
                Moves.Add(server_move);
                AddMoveToTurns(server_move);
                currentMove++;
            }
            Console.WriteLine(board);
        }
    }
}
