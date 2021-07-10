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
                board.GameOver += OnGameOver;
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ChessBoard Board { get; set; }
        public string Result { get
            {
                if (Board.Status == GameStatus.WhiteWon)
                    return "White Won";
                if (Board.Status == GameStatus.BlackWon)
                    return "Black Won";
                if (Board.Status == GameStatus.InProgress)
                    return "Game in Progress";
                return "Game in Progress";
            } }

        public void OnGameOver(object? sender, EventArgs e)
            => NotifyPropertyChanged(nameof(Result));
    }
}
