using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;

namespace Chess.ViewModels
{
    public class GamePanelViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public GamePanelViewModel(BoardViewModel board)
        {
            Moves = new MoveHistoryViewModel(board.Turns, board);
            Stats = new GameStatsViewModel(board);
            Board = board;
            if (board != null)
                Board.GameOver += OnGameOver;

            Content = Moves;
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MoveHistoryViewModel Moves { get; init; }
        public GameStatsViewModel Stats { get; init; }
        public BoardViewModel Board { get; init; }
        private ViewModelBase? content;
        public ViewModelBase? Content
        {
            get { return content; }
            set {
                content = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(StatsFill));
                NotifyPropertyChanged(nameof(MovesFill)); }
        }
        public IBrush MovesFill { get
            {
                if (Content == Moves)
                    return lightFill;
                return fill;
            } }
        public IBrush StatsFill { get
            {
                if (Content == Stats)
                    return lightFill;
                return fill;
            } }

        private IBrush lightFill = new SolidColorBrush(0xFF323240);
        private IBrush fill = new SolidColorBrush(0xFF28283D);

        public void OnGameOver(object? sender, EventArgs e)
        {
            Content = Stats;
        }
    }
}
