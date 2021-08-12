using System;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Chess.Models;
using Chess.ViewModels;

namespace Chess.Views
{
    public partial class BoardView : ViewBase
    {
        public BoardView()
        {
            Initialized += Util.InitialiseViewModelBase;
            Initialized += OnInitialised;
            InitializeComponent();
        }

        public void OnInitialised(object? sender, EventArgs e)
        {
            var bvm = DataContext as BoardViewModel;
            if (bvm == null)
            {
                Logger.EWrite("Cast failed!");
                return;
            }
            var uc = this.FindAncestorOfType<UserControl>();
            var pvm = uc.DataContext as PlayViewModel;
            bvm.pvm = pvm;
            EffectiveViewportChanged += bvm.UpdateTileSizes;
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
