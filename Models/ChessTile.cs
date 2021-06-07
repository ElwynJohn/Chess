using Avalonia.Media;

namespace Chess.Models
{
	public class ChessTile
	{
		public IBrush Fill { get; set; }
		public bool IsHighlighted { get; set; } = false;
		public IBrush HighlightedFill { get; set; }
		public IBrush NormalFill { get; set; }
	}
}
