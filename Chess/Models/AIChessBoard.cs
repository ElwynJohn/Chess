using System;
using System.Threading.Tasks;
using System.Diagnostics;

using Chess.Models;
using Chess.ViewModels;
using static Chess.Models.Message.MessageType;

namespace Chess.Models
{
    public class AIChessBoard : ChessBoard
    {
        // Starting fen: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        public AIChessBoard(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
            string gameRecordPath = "") : base(fen, gameRecordPath) {  }

        public override void MakeMove(ChessMove move, bool serverMove = false)
        {
            if (Status != GameStatus.InProgress)
                return;

            base.MakeMove(move, serverMove);

            var sw = new Stopwatch();
            var t = new Task<ChessMove>(() =>
            {
                // if the player is picking what piece to promote to,
                // wait till they have made their choice
                while (IsPromoting)
                        System.Threading.Thread.Sleep(10);
                sw.Start();
                return GetServerMove();
            });
            t.Start();
            t.ContinueWith((x) =>
            {
                // AI doesn't require promotion options
                IsPromoting = false;
                sw.Stop();
                Logger.IWrite($"Received server's move, {t.Result}, in {sw.Elapsed.Minutes * 60 + sw.Elapsed.Seconds} seconds");
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
