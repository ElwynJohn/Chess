using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.VisualTree;
using Chess.Models;
using Chess.ViewModels;

namespace Chess.Views
{
    public partial class BoardView : UserControl
    {
        public BoardView()
        {
            InitializeComponent();
        }

        private Panel? pStagedPanel { get; set; } = null;

        public void on_pointer_released(object sender, PointerReleasedEventArgs e)
        {
            pickup_or_drop(sender, e);
            change_rectangle_color(sender, e);
        }

        public void pickup_or_drop(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton != MouseButton.Left)
                return;

            BoardViewModel boardModel = (BoardViewModel)this.DataContext;
            Panel panel = (Panel)sender;
            ChessTile clickedTile = (ChessTile)panel.DataContext;

            if (clickedTile == null)
                return;
            if (pStagedPanel == null)
            {
                if (clickedTile.PieceType != ChessPieceType.None)
                    //if it's white's turn, check if the clickedTile is white
                    if ((boardModel.isWhitesMove
                        && (clickedTile.PieceType & ChessPieceType.IsWhite) != 0)
                        //if it's black's turn, check if the clickedTile is black
                        || (!boardModel.isWhitesMove
                        && (clickedTile.PieceType & ChessPieceType.IsWhite) == 0))
                            pStagedPanel = panel;
            }
            else
            {
                ChessTile pStagedTile = (ChessTile)pStagedPanel.DataContext;
                if (pStagedTile != null)
                {
                    MoveData move = boardModel.PiecePositions(pStagedTile, clickedTile);
                    if (boardModel.IsLegalMove(move))
                        boardModel.MakeMove(move);
                }
                pStagedPanel = null;
            }
        }

        public void change_rectangle_color(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton != MouseButton.Right)
                return;

            Panel panel = (Panel)sender;
            ChessTile oldTile = (ChessTile)panel.DataContext;

            ChessTile tile = new ChessTile
            {
                HighlightedFill = oldTile.HighlightedFill,
                NormalFill = oldTile.NormalFill,
                PieceBitmap = oldTile.PieceBitmap,
                IsHighlighted = !oldTile.IsHighlighted
            };

            panel.DataContext = tile;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
