using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Chess.Models
{
    public class ChessTile
    {
        public IBrush? Fill { get; set; }
        public bool IsHighlighted { get; set; } = false;
        public IBrush? HighlightedFill { get; set; }
        public IBrush? NormalFill { get; set; }
        public ChessPieceType PieceType { get; set; }
        public Bitmap? PieceBitmap { get; set; }
        public string? AssetPath { get; set; }
        public Dictionary<ChessPieceType, string> PieceToAssetMap = new Dictionary<ChessPieceType, string>
        {
          {ChessPieceType.Castle, "./Assets/piece_black_castle.png"},
          {ChessPieceType.Knight, "./Assets/piece_black_knight.png"},
          {ChessPieceType.Bishop, "./Assets/piece_black_bishop.png"},
          {ChessPieceType.Queen, "./Assets/piece_black_queen.png"},
          {ChessPieceType.King, "./Assets/piece_black_king.png"},
          {ChessPieceType.Pawn, "./Assets/piece_black_pawn.png"},
          {ChessPieceType.Castle | ChessPieceType.IsWhite, "./Assets/piece_white_castle.png"},
          {ChessPieceType.Knight | ChessPieceType.IsWhite, "./Assets/piece_white_knight.png"},
          {ChessPieceType.Bishop | ChessPieceType.IsWhite, "./Assets/piece_white_bishop.png"},
          {ChessPieceType.Queen | ChessPieceType.IsWhite, "./Assets/piece_white_queen.png"},
          {ChessPieceType.King | ChessPieceType.IsWhite, "./Assets/piece_white_king.png"},
          {ChessPieceType.Pawn | ChessPieceType.IsWhite, "./Assets/piece_white_pawn.png"},
        };

        public ChessTile()
        {

        }

        public ChessTile(ChessPieceType piece)
        {
            PieceType = piece;

            AssetPath = PieceToAssetMap.GetValueOrDefault(piece);
            if (AssetPath != null)
                PieceBitmap = new Bitmap(AssetPath);
        }
    }
}
