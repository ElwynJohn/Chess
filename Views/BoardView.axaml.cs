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
					BoardViewModel boardModel = (BoardViewModel)panel.FindAncestorOfType<UserControl>().DataContext;
					ChessPieceType[] boardState = SimplifyBoard(boardModel);
					MoveData move = PiecePositions(boardModel, pStagedTile, clickedTile);
					if (IsLegalMove(boardState, move))
						MakeMove(boardModel, move);
                 }
                pStagedPanel = null;
            }

        }

		public MoveData PiecePositions(BoardViewModel model, ChessTile origin, ChessTile target)
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
						positions[0] = file;
						positions[1] = rank;
					}
					if (tile == target)
					{
						positions[2] = file;
						positions[3] = rank;
					}
					file++;
				}
				file = 0;
				rank++;
			}
            MoveData move = new MoveData()
            {
                OriginFile = positions[0],
                OriginRank = positions[1],
                TargetFile = positions[2],
                TargetRank = positions[3]
            };
			return move;
		}

		public ChessPieceType[] SimplifyBoard(BoardViewModel model)
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

		public bool IsLegalMove(ChessPieceType[] board, MoveData move)
		{
            byte originFile = move.OriginFile;
            byte originRank = move.OriginRank;
            byte targetFile = move.TargetFile;
            byte targetRank = move.TargetRank;
			int originPos = originRank * 8 + originFile;
			int targetPos = targetRank * 8 + targetFile;
			ChessPieceType originPieceColor = board[originPos] & ChessPieceType.IsWhite;
			ChessPieceType targetPieceColor = board[targetPos] & ChessPieceType.IsWhite;

			if (board[originPos] == ChessPieceType.None)
				return false;
            // Prevent self capture
			if (ChessPieceType.None != board[targetPos] && (originPieceColor ^ targetPieceColor) == 0)
				return false;

            // Only Knights can jump over pieces
            if ((board[originPos] & ChessPieceType.Knight) == 0)
            {
                var dirVec = new int[]
                {
                    targetFile - originFile,
                    targetRank - originRank,
                };

                for (int i = 0; i < 2; i++)
                    if (dirVec[i] != 0)
                        dirVec[i] /= Math.Abs(dirVec[i]);

                int checkPos = originPos;
                int checkFile = originFile;
                int checkRank = originRank;
                while (checkPos >= 0 && checkPos < board.GetLength(0))
                {
                    checkFile += dirVec[0];
                    checkRank += dirVec[1];
                    checkPos = checkFile + 8 * checkRank;
                    if (checkPos == targetPos)
                        break;
                    if (board[checkPos] != ChessPieceType.None)
                        return false;
                }
            }

			if (0 != (board[originPos] & ChessPieceType.Knight))
			{
				if (Math.Abs(originRank - targetRank) == 2 && Math.Abs(originFile - targetFile) == 1)
					return true;
				else if (Math.Abs(originRank - targetRank) == 1 && Math.Abs(originFile - targetFile) == 2)
					return true;
				else
					return false;
			}

            if ((board[originPos] & (ChessPieceType.Bishop | ChessPieceType.Queen)) != 0)
            {
                if (Math.Abs(targetRank - originRank) == Math.Abs(targetFile - originFile))
                    return true;
            }

            if ((board[originPos] & (ChessPieceType.Castle | ChessPieceType.Queen)) != 0)
            {
                if (targetRank == originRank)
                    return true;
                if (targetFile == originFile)
                    return true;
            }

            if ((board[originPos] & ChessPieceType.Pawn) != 0)
            {
                // Pawns can only move forwards
                if ((board[originPos] & ChessPieceType.IsWhite) != 0)
                {
                    if (targetRank > originRank)
                        return false;
                }
                else
                {
                    if (targetRank < originRank)
                        return false;
                }

                // Allow moving 2 squares at the start
                if (originRank == 1 || originRank == 6)
                {
                    if (Math.Abs(targetRank - originRank) > 2)
                        return false;
                }
                else
                {
                    if (Math.Abs(targetRank - originRank) > 1)
                        return false;
                }

                if (targetFile == originFile && board[targetPos] == ChessPieceType.None)
                    return true;

                // Can't move horizontally
                if (targetRank == originRank)
                    return false;

                // Can take pieces diagonally 1 square in front
                if (board[targetPos] == ChessPieceType.None)
                    return false;
                if ((Math.Abs(targetFile - originFile) == 1 && Math.Abs(targetRank - originRank) == 1))
                    return true;
            }

            if ((board[originPos] & ChessPieceType.King) != 0)
            {
                if (Math.Abs(targetRank - originRank) <= 1 && Math.Abs(targetFile - originFile) <= 1)
                    return true;
            }

			return false;
		}

		public void MakeMove(BoardViewModel model, MoveData move)
		{
			ChessTile oldTile = model.Rows[move.OriginRank].RowTiles[move.OriginFile];
			model.Rows[move.TargetRank].RowTiles[move.TargetFile].SetPiece(oldTile.PieceType);
			oldTile.SetPiece(ChessPieceType.None);
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
