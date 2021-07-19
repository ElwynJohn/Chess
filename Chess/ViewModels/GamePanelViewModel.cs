using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Chess.Models;

namespace Chess.ViewModels
{
    public class GamePanelViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public GamePanelViewModel(BoardViewModel bvm, ChessTimer? whiteTimer, ChessTimer? blackTimer)
        {
            Moves = new MoveHistoryViewModel(bvm);
            Stats = new GameStatsViewModel(bvm.Board);
            Board = bvm.Board;
            if (bvm?.Board != null)
                Board.Update += OnBoardUpdate;
            DeadWhitePieces = new ObservableCollection<Bitmap>();
            DeadWhitePawns = new ObservableCollection<Bitmap>();
            DeadBlackPieces = new ObservableCollection<Bitmap>();
            DeadBlackPawns = new ObservableCollection<Bitmap>();
            WhiteTimer = whiteTimer;
            BlackTimer = blackTimer;
            if (WhiteTimer != null)
                WhiteTimer.Update += OnTimeChanged;
            if (BlackTimer != null)
                BlackTimer.Update += OnTimeChanged;

            Content = Moves;
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MoveHistoryViewModel Moves { get; init; }
        public GameStatsViewModel Stats { get; init; }
        public ChessBoard Board { get; init; }
        public ChessTimer? WhiteTimer { get; }
        public ChessTimer? BlackTimer { get; }
        // We display pawns seperately and below other pieces. This is so that
        // we can better make use of the available space. This is why we have a
        // collection for pawns as well as other pieces.
        public ObservableCollection<Bitmap> DeadWhitePieces { get; }
        public ObservableCollection<Bitmap> DeadWhitePawns { get; }
        public ObservableCollection<Bitmap> DeadBlackPieces { get; }
        public ObservableCollection<Bitmap> DeadBlackPawns { get; }
        private ViewModelBase? content;
        public ViewModelBase? Content
        {
            get { return content; }
            set {
                content = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(StatsFill));
                NotifyPropertyChanged(nameof(MovesFill)); }
        }
        public IBrush MovesFill { get
            {
                if (Content == Moves)
                    return lightFill;
                return fill;
            } }
        public IBrush StatsFill { get
            {
                if (Content == Stats)
                    return lightFill;
                return fill;
            } }

        private IBrush lightFill = new SolidColorBrush(0xFF323240);
        private IBrush fill = new SolidColorBrush(0xFF28283D);

        public void OnTimeChanged(object? sender, EventArgs e)
        {
            ChessTimer? timer = sender as ChessTimer;
            if (timer == null)
                return;
            if (timer.IsWhite)
                NotifyPropertyChanged(nameof(WhiteTimer));
            if (!timer.IsWhite)
                NotifyPropertyChanged(nameof(BlackTimer));
        }

        public void OnBoardUpdate(object? sender, BoardUpdateEventArgs e)
        {
            // This is a hack to get around an Avalonia bug. When the bug gets
            // fixed, this method can just be "Content = Stats". Everything
            // else can be removed.
            if (e.PieceTaken != ChessPiece.None)
            {
                Bitmap? bitmap = ChessTile.BitmapFromChessPiece(e.PieceTaken);
                if (bitmap != null)
                    if ((e.PieceTaken & ChessPiece.IsWhite) == ChessPiece.IsWhite)
                        if ((e.PieceTaken & ChessPiece.Pawn) == ChessPiece.Pawn)
                            DeadWhitePawns.Add(bitmap);
                        else
                            DeadWhitePieces.Add(bitmap);
                    else
                        if ((e.PieceTaken & ChessPiece.Pawn) == ChessPiece.Pawn)
                            DeadBlackPawns.Add(bitmap);
                        else
                            DeadBlackPieces.Add(bitmap);
            }

            if (Board.Status == GameStatus.InProgress)
            {
                if (Content == Stats)
                {
                    Content = Moves;
                    Content = Stats;
                }
                else
                {
                    Content = Stats;
                    Content = Moves;
                }
            }
            else
            {
                Content = Stats;
            }
        }
    }
}
