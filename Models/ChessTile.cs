using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Chess.Models
{
	public class ChessTile
	{
		public IBrush Fill { get; set; }
		public bool IsHighlighted { get; set; } = false;
		public IBrush HighlightedFill { get; set; }
		public IBrush NormalFill { get; set; }
		public ChessPieceType pieceType { get; set; }
		public Bitmap PieceBitmap { get; set; }
	}
}
