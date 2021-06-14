using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Chess.Models
{
    public class ChessTile : INotifyPropertyChanged
    {
        public IBrush? Fill { get; set; }
        public bool IsHighlighted { get; set; } = false;
        public IBrush? HighlightedFill { get; set; }
        public IBrush? NormalFill { get; set; }
        private ChessPieceType pPieceType;
        public ChessPieceType PieceType
        {
            get { return pPieceType; }
            set { pPieceType = value; NotifyPropertyChanged(); }
        }
        private Bitmap? pPieceBitmap;
        public Bitmap? PieceBitmap
        {
            get { return pPieceBitmap; }
            set { pPieceBitmap = value; NotifyPropertyChanged(); }
        }
        public string? AssetPath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Dictionary<ChessPieceType, string> PieceToAssetMap = new Dictionary<ChessPieceType, string>
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

        public void SetPiece(ChessPieceType piece)
        {
            PieceType = piece;
            AssetPath = PieceToAssetMap.GetValueOrDefault(piece);
            if (AssetPath != null)
                PieceBitmap = new Bitmap(AssetPath);
            else
                PieceBitmap = null;
        }

        public ChessTile()
        {

        }

        public ChessTile(ChessTile tile, ChessPieceType piece)
        {
            Fill = tile.Fill;
            HighlightedFill = tile.HighlightedFill;
            NormalFill = tile.NormalFill;

            PieceType = piece;

            AssetPath = PieceToAssetMap.GetValueOrDefault(piece);
            if (AssetPath != null)
                PieceBitmap = new Bitmap(AssetPath);
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
