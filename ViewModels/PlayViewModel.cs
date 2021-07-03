using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Chess.ViewModels
{
    public class PlayViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public PlayViewModel(BoardViewModel board)
        {
            Board = board;
            Board.pvm = this;
            Menu = new MenuViewModel();
            GamePanel = new GamePanelViewModel(board);
        }

        public BoardViewModel Board { get; }
        public MenuViewModel Menu { get; }
        public GamePanelViewModel GamePanel { get; }

        public new event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool isPromoting = false;
        public bool IsPromoting
        {
            get => isPromoting;
            set
            {
                isPromoting = value;
                NotifyPropertyChanged();
            }
        }
    }
}
