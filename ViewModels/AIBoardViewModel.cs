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

            IsInteractable = false;
            var t = new Task<ChessMove>(() =>
            {
                if (pvm != null)
                    while (pvm.IsPromoting)
                        System.Threading.Thread.Sleep(10);
                return GetServerMove();
            });
            t.Start();
            t.ContinueWith((x) =>
            {
                // AI doesn't require promotion options
                if (pvm != null)
                    pvm.IsPromoting = false;
                Console.WriteLine("Got move: {0}", t.Result);
                OnMoveMade(t.Result);
                IsInteractable = true;
            });
        }
    }
}
