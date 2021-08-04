using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.IO;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Chess.Models
{
    public class ChessTile : INotifyPropertyChanged
    {
        public ChessTile(ChessBoard board, int pos, bool displayOverlay = true)
        {
            Position = (byte)pos;
            DisplayOverlay = displayOverlay;

            if (IsWhite)
            {
                NormalFill = WhiteFill;
                TextToDisplayColour = new SolidColorBrush(0xFF383D64);
            }
            else
            {
                NormalFill = BlackFill;
                TextToDisplayColour = new SolidColorBrush(0xFFD2CACA);
            }
            SetNewPiece(board[pos]);
        }

        public byte Position { get; init; }
        public int File { get => Position % 8; }
        public int Rank { get => Position / 8; }
        public bool IsWhite { get => (Rank % 2 == 1) ? (Position % 2 == 1) : (Position % 2 == 0); }
        public bool DisplayOverlay { get; set; }
        // FileToDisplay and RankToDisplay are the strings that are displayed
        // on the board. Only the left and bottom tiles should display a string.
        public string FileToDisplay
        {
            get
            {
                if (!DisplayOverlay || Rank != 7)
                    return "";
                return ((char)('a' + File)).ToString();
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
        // @@Rework: might be nice to have all application colours retrieved from a
        // centralised location such as a "Theme" class. Maybe we could have a
        // factory return different instantiations of the Theme class based on the
        // enum you give it such as Theme.Dark, or Theme.Light.
        public static SolidColorBrush WhiteFill { get; }
            = new SolidColorBrush(0xFFDBCEE1);
        public static SolidColorBrush BlackFill { get; }
            = new SolidColorBrush(0XFF2E3673);
        public static SolidColorBrush WhiteHighlightFill { get; }
            = new SolidColorBrush(0XFFB8E6D2);
        public static SolidColorBrush BlackHighlightFill { get; }
            = new SolidColorBrush(0XFF4A877E);
        public static SolidColorBrush WhiteMoveFill { get; }
            = new SolidColorBrush(0XFFC597D9);
        public static SolidColorBrush BlackMoveFill { get; }
            = new SolidColorBrush(0XFF6C4081);
        public static SolidColorBrush WhiteCheckFill { get; }
            = new SolidColorBrush(0XFFD95555);
        public static SolidColorBrush BlackCheckFill { get; }
            = new SolidColorBrush(0XFFA13030);
        public IBrush? TextToDisplayColour { get; set; }
        public IBrush? Fill
        {
            get
            {
                if (IsHighlighted)
                    return IsWhite ? WhiteHighlightFill : BlackHighlightFill;
                return NormalFill;
            }
        }
        private bool isHighlighted = false;
        public bool IsHighlighted
        {
            get { return isHighlighted; }
            set { isHighlighted = value; NotifyPropertyChanged("Fill"); }
        }
        // @@Rework: Consider making NormalFill/normalFill an enum that specifies the tile
        // colour state (default or move or check or highlighted). Have the Fill getter
        // return a colour based on the enum and IsWhite. This allows code
        // that sets NormalFill to not have to worry about whether the tile is white.
        public IBrush? normalFill;
        public IBrush? NormalFill { get => normalFill; set {
            normalFill = value;
            if (!IsHighlighted)
                NotifyPropertyChanged("Fill");
        }}
        private Bitmap? pPieceBitmap;
        public Bitmap? PieceBitmap
        {
            get { return pPieceBitmap; }
            set { pPieceBitmap = value; NotifyPropertyChanged(); }
        }
        public string? AssetPath { get; set; }

        public void Update(ChessBoard? board)
        {
            if (board == null)
                return;
            SetNewPiece(board[Position]);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetNewPiece(ChessPiece piece)
            => PieceBitmap = BitmapFromChessPiece(piece);

        public static Bitmap? BitmapFromChessPiece(ChessPiece piece)
        {
            string? assetPath = PieceToAssetMap.GetValueOrDefault(piece);
            if (assetPath != null)
                return new Bitmap(assetPath);
            return null;
        }
        public static Dictionary<ChessPiece, string> PieceToAssetMap =
            new Dictionary<ChessPiece, string>
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
    }
}
