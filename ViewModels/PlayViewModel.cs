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
            for (int y = 0; y < 8; y++)
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
                Rows.Add(new ChessRow { RowTiles = rowTiles });
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
                if (fen[i] == '/') ;
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

            var FenToPieceMap = new Dictionary<char, ChessPieceType>
            {
                {'r', ChessPieceType.Castle},
                {'n', ChessPieceType.Knight},
                {'b', ChessPieceType.Bishop},
                {'q', ChessPieceType.Queen},
                {'k', ChessPieceType.King},
                {'p', ChessPieceType.Pawn},
                {'R', ChessPieceType.Castle | ChessPieceType.IsWhite},
                {'N', ChessPieceType.Knight | ChessPieceType.IsWhite},
                {'B', ChessPieceType.Bishop | ChessPieceType.IsWhite},
                {'Q', ChessPieceType.Queen | ChessPieceType.IsWhite},
                {'K', ChessPieceType.King | ChessPieceType.IsWhite},
                {'P', ChessPieceType.Pawn | ChessPieceType.IsWhite},
            };

            ChessTile[] chessTiles = new ChessTile[64];
            for (i = 0; i < 64; i++)
                chessTiles[i] = new ChessTile(FenToPieceMap.GetValueOrDefault(parsedFen[i]));
            return chessTiles;
        }
    }
}
