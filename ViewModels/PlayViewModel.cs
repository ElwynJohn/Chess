using Chess.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;

namespace Chess.ViewModels
{
	public class PlayViewModel : ViewModelBase
	{
		public PlayViewModel(string fen)
		{
			ChessTile[] tiles = ParseFen(fen);
			Rows = new ObservableCollection<ChessRow>();
			
			int currentTile = 0;
			bool isWhite = true;
			for (int y=0; y < 8; y++)
			{
				ObservableCollection<ChessTile> rowTiles = new ObservableCollection<ChessTile>();	
				for (int x = 0; x < 8; x++)
				{
					if (isWhite)
					{
						tiles[currentTile].Fill = new SolidColorBrush(0xFFD2CACA);
						tiles[currentTile].HighlightedFill = new SolidColorBrush(0xFFFFABCA);
						tiles[currentTile].NormalFill = new SolidColorBrush(0xFFD2CACA);
						rowTiles.Add(tiles[currentTile]);
					}
					else
					{
						tiles[currentTile].Fill = new SolidColorBrush(0xFF080D24);
						tiles[currentTile].HighlightedFill = new SolidColorBrush(0xFF480D24);
						tiles[currentTile].NormalFill = new SolidColorBrush(0xFF080D24);
						rowTiles.Add(tiles[currentTile]);
					}
					currentTile++;
					isWhite = !isWhite;
				}
				isWhite = !isWhite;
				Rows.Add(new ChessRow{ RowTiles = rowTiles });
			}
		}

		public ObservableCollection<ChessRow> Rows { get; }

		private ChessTile[] ParseFen(string fen)
		{
			Regex reg = new Regex(@"\D");
			StringBuilder parsedFen = new StringBuilder();
			int i = 0;
			while (fen[i] != ' ')
			{
				if (fen[i] =='/');
				else if (reg.Match(fen, i, 1).Success)
					parsedFen.Append(fen, i, 1);
				else
				{
					StringBuilder temp = new StringBuilder();
					temp.Append(fen[i]);
					int numEmptySquares = Int32.Parse(temp.ToString());
					parsedFen.Append('1', numEmptySquares);
				}
				i++;
			}
	
			ChessTile[] chessTiles = new ChessTile[64];
			for (i = 0; i < 64; i++)
			{
				switch(parsedFen[i])
				{
					case 'r':
						chessTiles[i] = new ChessTile 
						{
							PieceType = ChessPieceType.Castle & ChessPieceType.IsWhite,
							PieceBitmap = new Bitmap("./Assets/piece_black_castle.png")
						};
						break;
					case 'n':
						chessTiles[i] = new ChessTile 
						{
							PieceType = ChessPieceType.Knight & ChessPieceType.IsWhite,
							PieceBitmap = new Bitmap("./Assets/piece_black_knight.png")
						};
						break;
					case 'b':
						chessTiles[i] = new ChessTile 
						{
							PieceType = ChessPieceType.Bishop & ChessPieceType.IsWhite,
							PieceBitmap = new Bitmap("./Assets/piece_black_bishop.png")
						};
						break;
					case 'q':
						chessTiles[i] = new ChessTile 
						{
							PieceType = ChessPieceType.Queen & ChessPieceType.IsWhite,
							PieceBitmap = new Bitmap("./Assets/piece_black_queen.png")
						};
						break;
					case 'k':
						chessTiles[i] = new ChessTile 
						{
							PieceType = ChessPieceType.King & ChessPieceType.IsWhite,
							PieceBitmap = new Bitmap("./Assets/piece_black_king.png")
						};
						break;
					case 'p':
						chessTiles[i] = new ChessTile 
						{
							PieceType = ChessPieceType.Pawn & ChessPieceType.IsWhite,
							PieceBitmap = new Bitmap("./Assets/piece_black_pawn.png")
						};
						break;
					case 'R':
						chessTiles[i] = new ChessTile 
						{
							PieceType = ChessPieceType.Castle,
							PieceBitmap = new Bitmap("./Assets/piece_white_castle.png")
						};
						break;
					case 'N':
						chessTiles[i] = new ChessTile 
						{
							PieceType = ChessPieceType.Knight,
							PieceBitmap = new Bitmap("./Assets/piece_white_knight.png")
						};
						break;
					case 'B':
						chessTiles[i] = new ChessTile 
						{
							PieceType = ChessPieceType.Bishop,
							PieceBitmap = new Bitmap("./Assets/piece_white_bishop.png")
						};
						break;
					case 'Q':
						chessTiles[i] = new ChessTile 
						{
							PieceType = ChessPieceType.Queen,
							PieceBitmap = new Bitmap("./Assets/piece_white_queen.png")
						};
						break;
					case 'K':
						chessTiles[i] = new ChessTile 
						{
							PieceType = ChessPieceType.King,
							PieceBitmap = new Bitmap("./Assets/piece_white_king.png")
						};
						break;
					case 'P':
						chessTiles[i] = new ChessTile 
						{
							PieceType = ChessPieceType.Pawn,
							PieceBitmap = new Bitmap("./Assets/piece_white_pawn.png")
						};
						break;
					default:
						chessTiles[i] = new ChessTile
						{
							PieceType = 0,
							PieceBitmap = null 
						};
						break;
				}
			}
			return chessTiles;
		}
	}
}
