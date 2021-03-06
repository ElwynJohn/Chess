using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

using Chess.Models;

namespace Chess.ViewModels
{
    public class BoardViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public BoardViewModel(ChessBoard board,
            bool isInteractable = true, bool displayOverlay = true)
        {
            Board = board;
            LiveBoard = board;
            board.Update += UpdateTiles;
            board.Update += (object sender, BoardUpdateEventArgs e) =>
            {
                if (e.Move !=null && isInteractable)
                    currentBoard++;
            };

            board.Update += (object sender, BoardUpdateEventArgs e) =>
            {
// King may now be in check even if a move has not been made (due to the
// possibility of a promotion).
                UpdateCheckFill();
                if (e.Move == null)
                    return;
                UpdateMoveFill(e.Move);
            };

            Rows = new ObservableCollection<ChessRow>();

            for (int y = 0; y < 8; y++)
            {
                ObservableCollection<ChessTile> rowTiles = new ObservableCollection<ChessTile>();
                for (int x = 0; x < 8; x++)
                {
                    int currentTile = y * 8 + x;
                    tiles[currentTile] = new ChessTile(Board, currentTile, displayOverlay);
                    rowTiles.Add(tiles[currentTile]);
                }
                Rows.Add(new ChessRow { RowTiles = rowTiles });
            }

            IsInteractable = isInteractable;
        }
        public void OnViewInitialised(object sender, EventArgs e)
        {
            Window.LayoutUpdated += (s, e) =>
            {
                if (!initialised)
                {
                    UpdateTileSizes();
                    TileSizeChanged?.Invoke(this, EventArgs.Empty);
                    initialised = true;
                }
            };
            Window.EffectiveViewportChanged += (s, e) =>
            {
                UpdateTileSizes();
                TileSizeChanged?.Invoke(this, EventArgs.Empty);
            };
        }
        public void UpdateTileSizes()
        {
            double boardSize;
            if (View.Bounds.Width < View.Bounds.Height)
            {
                double viewSize = View.Bounds.Width;
                boardSize = viewSize - BorderSize * 2;
                double marginThickness = (View.Bounds.Height - viewSize) / 2;
                Margin = new Thickness(0,marginThickness,0,marginThickness);
            }
            else
            {
                double viewSize = View.Bounds.Height;
                boardSize = viewSize - BorderSize * 2;
                double marginThickness = (View.Bounds.Width - viewSize) / 2;
                Margin = new Thickness(marginThickness,0,marginThickness,0);
            }
            ChessTile.TileSize = (int)(boardSize / 8);
            ChessTile.PieceSize = (int)(boardSize / 8 * PiecePxPerTilePx);
            Size = ChessTile.TileSize * 8 + BorderSize * 2;
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].NotifyPropertyChanged(nameof(ChessTile.TileSize));
                tiles[i].NotifyPropertyChanged(nameof(ChessTile.PieceSize));
                tiles[i].NotifyPropertyChanged(nameof(ChessTile.FileToDisplay));
                tiles[i].NotifyPropertyChanged(nameof(ChessTile.RankToDisplay));
            }
                NotifyPropertyChanged(nameof(Size));
                NotifyPropertyChanged(nameof(Margin));
        }

        public event EventHandler? TileSizeChanged;
        public new event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double Size { get; set; }
        public Thickness Margin { get; set; }
        public double BorderSize { get => 10; }
        public Thickness BorderSizetest { get => new Thickness(10); }
        public ChessBoard Board { get; private set; }
        public ChessBoard LiveBoard { get; private set; } // If we don't store this,
        // we would not be able to retrieve the most recent board after clicking previous move.
        public ObservableCollection<ChessRow> Rows { get; private set; }
        public bool IsInteractable { get; private set; }

        private ChessTile[] tiles = new ChessTile[64];
        private ChessTile?[] tiles_to_clear = new ChessTile?[2];
        private ChessTile[] checkedTileToClear = new ChessTile[2];
        private ChessTile? stagedTile;
        private int currentBoard = 0;
        private const double PiecePxPerTilePx = 0.7;
        private bool initialised = false;

        public void LeftClickTile(ChessTile clickedTile)
        {
            if (!IsInteractable || Board.IsPromoting)
                return;

            if (stagedTile == null)
            {
                if (Board[clickedTile.Position] != ChessPiece.None)
                    //if it's white's turn, check if the clickedTile is white
                    if ((Board.IsWhitesMove
                        && (Board[clickedTile.Position] & ChessPiece.IsWhite) != 0)
                        //if it's black's turn, check if the clickedTile is black
                        || (!Board.IsWhitesMove
                        && (Board[clickedTile.Position] & ChessPiece.IsWhite) == 0))
                            stagedTile = clickedTile;
            }
            else
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                ChessMove move = PiecePositions(stagedTile, clickedTile);
                if (Board.IsLegalMove(move))
                {
                    Stopwatch watch2 = new Stopwatch();
                    watch2.Start();
                    Board.MakeMove(move, false);
                    watch2.Stop();
                    Logger.DWrite($"Time taken to MakeMove: {watch2.ElapsedMilliseconds}");
                }
                stagedTile = null;

                watch.Stop();
                Logger.DWrite($"Time taken to move player's piece: {watch.ElapsedMilliseconds}");
            }
        }
        public void RightClickTile(ChessTile clickedTile)
            { if (clickedTile != null)
                { clickedTile.Highlighted = !clickedTile.Highlighted; } }

        public void PreviousMove()
        {
            if (currentBoard == 0)
                return;
            currentBoard--;

            IsInteractable = false;
            Board = Board.Boards[currentBoard];
            UpdateTiles(null, new BoardUpdateEventArgs(Board, null, ChessPiece.None));
            UpdateMoveFill(Board.LastMove);
            UpdateCheckFill();
        }

        public void NextMove()
        {
            if (currentBoard == Board.Boards.Count - 1)
                return;
            currentBoard++;
            if (currentBoard == Board.Boards.Count - 1)
            {
                Board = LiveBoard;
                IsInteractable = true;
            }
            else
                Board = Board.Boards[currentBoard];
            UpdateTiles(null, new BoardUpdateEventArgs(Board, null, ChessPiece.None));
            UpdateMoveFill(Board.LastMove);
            UpdateCheckFill();
        }

        public void UpdateTiles(object? sender, BoardUpdateEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (ChessRow row in Rows.AsEnumerable())
            {
                if (row.RowTiles == null)
                {
                    Logger.EWrite("rowTiles is null");
                    continue;
                }
                foreach (ChessTile tile in row.RowTiles.AsEnumerable())
                {
                    if (e.Move != null)
                        tile.Highlighted = false;
                    tile.Update(e.Board);
                }
            }
            Logger.DWrite($"Time taken to UpdateTiles: {sw.ElapsedMilliseconds}");
        }

        public ChessMove PiecePositions(ChessTile origin, ChessTile target)
        {
            byte[] positions = new byte[4];
            byte rank = 0;
            byte file = 0;
            foreach (ChessRow row in Rows)
            {
                if (row.RowTiles == null)
                    continue;
                foreach (ChessTile tile in row.RowTiles)
                {
                    if (tile == origin)
                    {
                        positions[0] = file;
                        positions[1] = rank;
                    }
                    if (tile == target)
                    {
                        positions[2] = file;
                        positions[3] = rank;
                    }
                    file++;
                }
                file = 0;
                rank++;
            }
            ChessMove move = new ChessMove()
            {
                From = ChessBoard.Pos64(positions[0], positions[1]),
                To = ChessBoard.Pos64(positions[2], positions[3]),
            };
            return move;
        }

        private void UpdateMoveFill(ChessMove? move)
        {
            foreach (ChessTile? tile in tiles_to_clear)
                if (tile != null)
                    tile.Moved = false;

            if (move == null)
                return;

            tiles[move.From].Moved = true;
            tiles[move.To].Moved = true;

            tiles_to_clear[0] = tiles[move.From];
            tiles_to_clear[1] = tiles[move.To];
        }
        private void UpdateCheckFill()
        {
            for(int i = 0; i < 2; i++)
                if (checkedTileToClear[i] != null)
                    checkedTileToClear[i].InCheck = false;

            bool[] isInCheck = { Board.IsWhiteInCheck, Board.IsBlackInCheck };
            for(int i = 0; i < 2; i++)
            {
                bool isWhite = i == 0 ? true : false;
                ChessTile kingTile = tiles[Board.FindKing(isWhite)];
                if (isInCheck[i])
                {
                    kingTile.InCheck = true;
                    checkedTileToClear[i] = kingTile;
                }
            }
        }
    }
}
