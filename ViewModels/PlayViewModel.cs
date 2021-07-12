using System.ComponentModel;
using System.Runtime.CompilerServices;
using Chess.Models;

namespace Chess.ViewModels
{
    public class PlayViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public PlayViewModel(BoardViewModel bvm, GamePanelViewModel gamePanel)
        {
            Bvm = bvm;
            Menu = new MenuViewModel();
            GamePanel = gamePanel;

            Bvm.Board.Update += BoardUpdated;
        }

        public BoardViewModel Bvm { get; }
        public MenuViewModel Menu { get; }
        public GamePanelViewModel GamePanel { get; }
        public bool IsPromoting { get => Bvm.Board.IsPromoting; }

        public new event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void BoardUpdated(object sender, BoardUpdateEventArgs e)
            => NotifyPropertyChanged(nameof(IsPromoting));
    }
}
