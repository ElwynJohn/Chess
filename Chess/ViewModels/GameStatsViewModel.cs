using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Chess.Models;

namespace Chess.ViewModels
{
    public class GameStatsViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public GameStatsViewModel(ChessBoard board)
        {
            Board = board;
            if (board != null)
                board.Update += UpdateStats;
        }
        public void OnViewInitialisation(object sender, EventArgs e)
        {
            View.EffectiveViewportChanged += (s, e) =>
            {
                Logger.DWrite($"GameStats View's Height: {Height}");
                NotifyPropertyChanged(nameof(Height));
            };
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public double Height { get => View.Bounds.Height; }
        public ChessBoard Board { get; set; }
        public string Result { get
            {
                if (Board.Status == GameStatus.WhiteWon)
                    return "White Won";
                if (Board.Status == GameStatus.BlackWon)
                    return "Black Won";
                if (Board.Status == GameStatus.Draw)
                    return "Draw";
                if (Board.Status == GameStatus.InProgress)
                    return "Game in Progress";
                return "Game in Progress";
            } }

        public void UpdateStats(object? sender, EventArgs e)
            => NotifyPropertyChanged(nameof(Result));
    }
}
