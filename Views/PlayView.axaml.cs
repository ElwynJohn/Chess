using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Chess.Models;
using Chess.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace Chess.Views
{
    public partial class PlayView : UserControl
    {
        public PlayView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void on_pointer_released(object sender, PointerReleasedEventArgs e)
        {
            pickup_or_drop(sender, e);
            change_rectangle_color(sender, e);
        }

        private Panel? pStagedPanel { get; set; } = null;

        public void pickup_or_drop(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton != MouseButton.Left)
                return;

            Panel panel = (Panel)sender;

            ChessTile clickedTile = (ChessTile)panel.DataContext;

            if (clickedTile == null)
                return;
            if (pStagedPanel == null)
			{
				if (clickedTile.PieceType != ChessPieceType.None)
					pStagedPanel = panel;
			}
            else
            {
                ChessTile pStagedTile = (ChessTile)pStagedPanel.DataContext;
                if (pStagedTile != null)
                {
					PlayViewModel boardModel = (PlayViewModel)panel.FindAncestorOfType<UserControl>().DataContext;
					ChessPieceType[] boardState = SimplifyBoard(boardModel);
					byte[] pos = PiecePositions(boardModel, pStagedTile, clickedTile);
					if (IsLegalMove(boardState, pos[0], pos[1], pos[2], pos[3]))
					{
						clickedTile.SetPiece(pStagedTile.PieceType);
						pStagedTile.SetPiece(ChessPieceType.None);
					}
                 }
                pStagedPanel = null;
            }

        }

		public byte[] PiecePositions(PlayViewModel model, ChessTile origin, ChessTile target)
		{
			byte[] positions = new byte[4];
			byte rank = 0;
			byte file = 0;
			foreach (ChessRow row in model.Rows)
			{
				foreach (ChessTile tile in row.RowTiles)
				{
					if (tile == origin)
					{
						positions[0] = rank;
						positions[1] = file;
					}
					if (tile == target)
					{
						positions[2] = rank;
						positions[3] = file;
					}
					file++;
				}
				file = 0;
				rank++;
			}
			return positions;
		}

		public ChessPieceType[] SimplifyBoard(PlayViewModel model)
		{
			ChessPieceType[] board = new ChessPieceType[64];
			int i = 0;
			foreach (ChessRow row in model.Rows)
			{
				foreach (ChessTile tile in row.RowTiles)
				{
					board[i] = tile.PieceType;
					i++;
				}
			}
			return board;
		}

		public bool IsLegalMove(ChessPieceType[] board, byte originRank, byte originFile, byte targetRank, byte targetFile)
		{
			int originPos = originRank * 8 + originFile;
			int targetPos = targetRank * 8 + targetFile;
			ChessPieceType originPieceColor = board[originPos] & ChessPieceType.IsWhite;
			ChessPieceType targetPieceColor = board[targetPos] & ChessPieceType.IsWhite;

			if (board[originPos] == ChessPieceType.None)
				return false;
			if (ChessPieceType.None != board[targetPos] && (originPieceColor ^ targetPieceColor) == 0)
				return false;

			if (0 != (board[originPos] & ChessPieceType.Knight))
			{
				if (Math.Abs(originRank - targetRank) == 2 && Math.Abs(originFile - targetFile) == 1)
					return true;
				else if (Math.Abs(originRank - targetRank) == 1 && Math.Abs(originFile - targetFile) == 2)
					return true;
				else
					return false;
			}

			return true;
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
                PieceBitmap = oldTile.PieceBitmap
            };
            if (oldTile.IsHighlighted)
                tile.Fill = tile.NormalFill;
            else
                tile.Fill = tile.HighlightedFill;
            tile.IsHighlighted = !oldTile.IsHighlighted;

            panel.DataContext = tile;
        }
    }
}
