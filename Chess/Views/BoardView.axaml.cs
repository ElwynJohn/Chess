using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Chess.Models;
using Chess.ViewModels;

namespace Chess.Views
{
    public partial class BoardView : UserControl
    {
        public BoardView()
        {
            Initialized += Util.InitialiseViewModelBase;
            Initialized += (o, e) =>
            {
                var vm = DataContext as ViewModelBase;
                if (vm == null)
                {
                    Logger.EWrite("Cast failed!");
                    return;
                }
                EffectiveViewportChanged += (o, e) => Logger.IWrite($"New height is {vm.Window.Height}");
                EffectiveViewportChanged += (o, e) => Logger.IWrite($"New width is {vm.Window.Width}");
            };
            InitializeComponent();
        }

        private ChessTile? pStagedTile { get; set; } = null;

        public void on_pointer_released(object sender, PointerReleasedEventArgs e)
        {
            BoardViewModel? boardModel = (BoardViewModel?)this.DataContext;
            ChessTile? clickedTile = (ChessTile?)((Panel)sender).DataContext;
            if (clickedTile == null)
                return;
            if (e.InitialPressMouseButton == MouseButton.Left)
                boardModel?.LeftClickTile(clickedTile);
            else if (e.InitialPressMouseButton == MouseButton.Right)
                boardModel?.RightClickTile(clickedTile);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
