using System;
using System.Threading.Tasks;

using Chess.Models;
using Chess.ViewModels;
using static Chess.Models.Message.MessageType;

namespace Chess.Models
{
    public class AIChessBoard : ChessBoard
    {
        public AIChessBoard(string fen = "r/1/b/q/k/b/2/p/1/ppppp/1/2/n/2/n/2/1/p/6/8/3/K/4/7/r/7/q w - - 0 1",
            string gameRecordPath = "") : base(fen, gameRecordPath) {  }

        public override void MakeMove(ChessMove move, bool serverMove = false)
        {
            base.MakeMove(move, serverMove);

            var t = new Task<ChessMove>(() =>
            {
                // if the player is picking what piece to promote to,
                // wait till they have made their choice
                while (IsPromoting)
                        System.Threading.Thread.Sleep(10);
                return GetServerMove();
            });
            t.Start();
            t.ContinueWith((x) =>
            {
                // AI doesn't require promotion options
                IsPromoting = false;
                Console.WriteLine("Got move: {0}", t.Result);
                base.MakeMove(t.Result, true);
            });
        }

        // Note: This method will probably block for quite a while
        private ChessMove GetServerMove()
        {
            Message request = new Message(new byte[1], 1, BestMoveRequest);
            request.Send(message_client);

            Message reply = new Message(BestMoveReply);
            reply.Receive(message_client);

            var move = new ChessMove(reply.Bytes);

            return move;
        }
    }
}
