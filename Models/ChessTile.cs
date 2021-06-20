using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Avalonia.Media;
using Avalonia.Media.Imaging;

using Chess.ViewModels;

namespace Chess.Models
{
    public class ChessTile : INotifyPropertyChanged
    {
        public byte Position { get; set; }
        public bool DisplayOverlay { get; set; }
        // FileToDisplay and RankToDisplay are the strings that are displayed
        // on the board. Only the left and bottom tiles should display a string.
        public string FileToDisplay
        {
            get
            {
                if (!DisplayOverlay || Position / 8 != 7)
                    return "";
                return ((char)('a' + Position % 8)).ToString();
            }
        }
        public string RankToDisplay
        {
            get
            {
                if (!DisplayOverlay || Position % 8 != 0)
                    return "";
                return (8 - Position / 8).ToString();
            }
        }
        public IBrush? TextToDisplayColour { get; set; }
        public IBrush? Fill
        {
            get
            {
                if (IsHighlighted)
                    return HighlightedFill;
                return NormalFill;
            }
        }
        private bool isHighlighted = false;
        public bool IsHighlighted
        {
            get { return isHighlighted; }
            set { isHighlighted = value; NotifyPropertyChanged("Fill"); }
        }
        public IBrush? HighlightedFill { get; set; }
        public IBrush? NormalFill { get; set; }
        private ChessBoard board;
        public ChessPiece PieceType
        {
            get { return board[Position]; }
            set { board[Position] = value; NotifyPropertyChanged(); }
        }
        private Bitmap? pPieceBitmap;
        public Bitmap? PieceBitmap
        {
            get { return pPieceBitmap; }
            set { pPieceBitmap = value; NotifyPropertyChanged(); }
        }
        public string? AssetPath { get; set; }

        public void Update()
        {
            AssetPath = PieceToAssetMap.GetValueOrDefault(board[Position]);
            if (AssetPath != null)
                PieceBitmap = new Bitmap(AssetPath);
            else
                PieceBitmap = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Dictionary<ChessPiece, string> PieceToAssetMap = new Dictionary<ChessPiece, string>
        {
            {ChessPiece.Castle, "./Assets/piece_black_castle.png"},
            {ChessPiece.Knight, "./Assets/piece_black_knight.png"},
            {ChessPiece.Bishop, "./Assets/piece_black_bishop.png"},
            {ChessPiece.Queen, "./Assets/piece_black_queen.png"},
            {ChessPiece.King, "./Assets/piece_black_king.png"},
            {ChessPiece.Pawn, "./Assets/piece_black_pawn.png"},
            {ChessPiece.Castle | ChessPiece.IsWhite, "./Assets/piece_white_castle.png"},
            {ChessPiece.Knight | ChessPiece.IsWhite, "./Assets/piece_white_knight.png"},
            {ChessPiece.Bishop | ChessPiece.IsWhite, "./Assets/piece_white_bishop.png"},
            {ChessPiece.Queen | ChessPiece.IsWhite, "./Assets/piece_white_queen.png"},
            {ChessPiece.King | ChessPiece.IsWhite, "./Assets/piece_white_king.png"},
            {ChessPiece.Pawn | ChessPiece.IsWhite, "./Assets/piece_white_pawn.png"},
        };

        public ChessTile(ChessBoard board, int pos, bool displayOverlay = true)
        {
            this.board = board;

            Position = (byte)pos;
            DisplayOverlay = displayOverlay;

            int file = pos % 8;
            int rank = pos / 8;

            bool isWhite = (rank % 2 == 1) ? (pos % 2 == 0) : (pos % 2 == 1);

            if (isWhite)
            {
                NormalFill = new SolidColorBrush(0xFFD2CACA);
                HighlightedFill = new SolidColorBrush(0xFFFFABCA);
                TextToDisplayColour = new SolidColorBrush(0xFF383D64);
            }
            else
            {
                NormalFill = new SolidColorBrush(0xFF383D64);
                HighlightedFill = new SolidColorBrush(0xFF682D44);
                TextToDisplayColour = new SolidColorBrush(0xFFD2CACA);
            }

            AssetPath = PieceToAssetMap.GetValueOrDefault(board[pos]);
            if (AssetPath != null)
                PieceBitmap = new Bitmap(AssetPath);
        }
    }
}
