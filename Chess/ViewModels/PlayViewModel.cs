using System;
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

            Bvm.TileSizeChanged += UpdateTileSizes;
            Bvm.Board.Update += BoardUpdated;
        }
        public void OnViewInitialised(object sender, EventArgs e)
        {
            Window.EffectiveViewportChanged += (s, e) =>
            {
                Logger.Buffer += $"\nPlay View changed Height to {View.Bounds.Height}";
                Logger.Buffer += $"\nPlay View changed Width to {View.Bounds.Width}";
                Logger.Buffer += $"\nWindow changed Width to {Window.Bounds.Width}";
                Logger.Buffer += $"\nChess Tile Size {ChessTile.TileSize}";
                Logger.Buffer += $"\nChess Piece Size {ChessTile.PieceSize}";
                Logger.DWrite();
                UpdateTileSizes(sender, e);
            };
        }
        public void UpdateTileSizes(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(TileSize));
            NotifyPropertyChanged(nameof(PieceSize));
        }

        public double TileSize{ get => ChessTile.TileSize; }
        public double PieceSize { get => ChessTile.PieceSize; }
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
