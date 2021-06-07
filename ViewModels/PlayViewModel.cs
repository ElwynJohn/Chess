using Chess.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
						rowTiles.Add(new ChessTile{ Fill="White" });
					else
						rowTiles.Add(new ChessTile{ Fill="#080D24" });
					isWhite = !isWhite;
				}
				isWhite = !isWhite;
				Rows.Add(new ChessRow{ RowTiles = rowTiles });
			}
		}

		public ObservableCollection<ChessRow> Rows { get; }
	}
}
