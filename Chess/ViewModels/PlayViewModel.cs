using System.ComponentModel;
using System.Runtime.CompilerServices;
using Chess.Models;

namespace Chess.ViewModels
{
    public class PlayViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public PlayViewModel(BoardViewModel bvm, GamePanelViewModel gamePanel, MenuViewModel menuVM)
        {
            Bvm = bvm;
            Menu = menuVM;
            GamePanel = gamePanel;

            Bvm.Board.Update += BoardUpdated;
        }

        public int Height {get => ChessTile.Height;}
        public int Width {get => ChessTile.Width;}

        public BoardViewModel Bvm { get; }
        public MenuViewModel Menu { get; }
        public GamePanelViewModel GamePanel { get; }
        public bool IsPromoting { get => Bvm.Board.IsPromoting; }

        public new event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void BoardUpdated(object sender, BoardUpdateEventArgs e)
            => NotifyPropertyChanged(nameof(IsPromoting));
    }
}
