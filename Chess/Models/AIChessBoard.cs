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
            if (Status != GameStatus.InProgress || !IsWhitesMove)
                return;

            base.MakeMove(move, serverMove);

            var sw = new Stopwatch();
            var t = new Task(() =>
            {
                try
                {
                    // if the player is picking what piece to promote to,
                    // wait till they have made their choice
                    while (IsPromoting)
                        System.Threading.Thread.Sleep(10);
                    sw.Start();
                    var server_move = GetServerMove();

                    // AI doesn't require promotion options
                    IsPromoting = false;
                    sw.Stop();
                    Logger.IWrite($"Received server's move, {server_move}, in {sw.Elapsed.Minutes * 60 + sw.Elapsed.Seconds} seconds");
                    base.MakeMove(server_move, true);
                }
                catch (Exception e)
                {
                    Logger.EWrite(e);
                }
            });
            t.Start();
        }

        // Note: This method will probably block for quite a while
        private ChessMove GetServerMove()
        {
            Message mess = new Message(new byte[1], 1, BestMoveRequest);
            mess.Send();
            mess.Receive();

            var move = new ChessMove(mess.Bytes);

            return move;
        }
    }
}
