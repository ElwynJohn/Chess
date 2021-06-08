using Chess.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Chess.ViewModels
{
	public class PlayViewModel : ViewModelBase
	{
		public PlayViewModel()
		{
			Rows = new ObservableCollection<ChessRow>();
			
			bool isWhite = true;
			for (int y=0; y < 8; y++)
			{
				ObservableCollection<ChessTile> rowTiles = new ObservableCollection<ChessTile>();	
				for (int x = 0; x < 8; x++)
				{
					if (isWhite)
						rowTiles.Add(new ChessTile
						{
							Fill = new SolidColorBrush(0xFFD2CACA),
							HighlightedFill = new SolidColorBrush(0xFFFFABCA),
							NormalFill = new SolidColorBrush(0xFFD2CACA),
							PieceBitmap = new Bitmap("./Assets/piece_white_horse.png")
						});
					else
						rowTiles.Add(new ChessTile
						{ 
							Fill = new SolidColorBrush(0xFF080D24), 
							HighlightedFill = new SolidColorBrush(0xFF480D24),
							NormalFill = new SolidColorBrush(0xFF080D24),
							PieceBitmap = new Bitmap("./Assets/piece_white_castle.png")
						});
					isWhite = !isWhite;
				}
				isWhite = !isWhite;
				Rows.Add(new ChessRow{ RowTiles = rowTiles });
			}
		}

		public ObservableCollection<ChessRow> Rows { get; }
	}
}
