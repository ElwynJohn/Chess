using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Chess.Models
{
    public class ChessTimer : Stopwatch
    {
        public ChessTimer(ChessBoard board, bool isWhite, int gameTime) : base()
        {
            this.board = board;
            GameTime = gameTime;
            IsWhite = isWhite;

            board.Update += (object sender, BoardUpdateEventArgs e) =>
            {
                if (IsWhite & board.IsWhitesMove)
                    Start();
                else if (!IsWhite & !board.IsWhitesMove)
                    Start();
                else
                    Stop();
            };

            Task t = new Task(() => Timer());
            t.Start();
        }

        public event EventHandler? Update;
        private void OnUpdate() => Update?.Invoke(this, EventArgs.Empty);

        // Number of seconds since the start of the game
        public int ElapsedSeconds { get => Elapsed.Seconds + Elapsed.Minutes * 60; }
        // Is this timer timing the white player
        public bool IsWhite { get; init; }
        // Amount of time that this player has at the start of the game
        public int GameTime { get; init; }
        public int SecondsLeft { get => GameTime - ElapsedSeconds; }

        private ChessBoard board;

        public override string ToString()
        {
            string minsLeft = (SecondsLeft / 60).ToString();
            string secsLeft = (SecondsLeft % 60).ToString();
            while (secsLeft.Length < 2)
                secsLeft = "0" + secsLeft;
            while (minsLeft.Length < 2)
                minsLeft = "0" + minsLeft;

            return minsLeft + ":" + secsLeft;
        }

        private void Timer()
        {
            int prevSec = 0;
            while (ElapsedSeconds < GameTime)
            {
                System.Threading.Thread.Sleep(50);
                if (prevSec != ElapsedSeconds)
                {
                    prevSec = ElapsedSeconds;
                    OnUpdate();
                }
            }
            if (board.Status == GameStatus.InProgress)
            {
                if (IsWhite)
                    board.Status = GameStatus.BlackWon;
                else
                    board.Status = GameStatus.WhiteWon;
            }
        }
    }
}
