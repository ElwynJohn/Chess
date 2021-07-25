using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Diagnostics;
using Avalonia.Media;
using Chess.Models;
using static Chess.Models.Message.MessageType;
using static Chess.Models.ChessBoard;

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
                if (e.Move == null)
                    return;

                Logger.DWrite($"Move.From: {e.Move.From}\n");
                Logger.DWrite($"Move.To: {e.Move.To}\n");

                foreach (ChessTile? tile in tiles_to_clear)
                    if (tile != null)
                        tile.IsHighlighted = false;

                tiles[e.Move.From].HighlightedFill = new SolidColorBrush(0xFF00FFFF);
                tiles[e.Move.From].IsHighlighted = true;
                tiles[e.Move.To].HighlightedFill = new SolidColorBrush(0xFF00FFFF);
                tiles[e.Move.To].IsHighlighted = true;

                tiles_to_clear[0] = tiles[e.Move.From];
                tiles_to_clear[1] = tiles[e.Move.To];
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

        public new event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ChessBoard Board { get; private set; }
        public ChessBoard LiveBoard { get; private set; } // If we don't store this,
        // we would not be able to retrieve the most recent board after clicking previous move.
        public ObservableCollection<ChessRow> Rows { get; private set; }
        public bool IsInteractable { get; private set; }

        private ChessTile[] tiles = new ChessTile[64];
        private ChessTile?[] tiles_to_clear = new ChessTile?[2];
        private ChessTile? stagedTile;
        private int currentBoard = 0;

        public void LeftClickTile(ChessTile clickedTile)
        {
            if (!IsInteractable)
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
                    Board.MakeMove(move, false);
                stagedTile = null;

                watch.Stop();
                Logger.DWrite($"Time taken to move player's piece: {watch.ElapsedMilliseconds}");
            }
        }
        public void RightClickTile(ChessTile clickedTile)
            { if (clickedTile != null)
                { clickedTile.IsHighlighted = !clickedTile.IsHighlighted; } }

        public void PreviousMove()
        {
            if (currentBoard == 0)
                return;
            currentBoard--;

            IsInteractable = false;
            Board = Board.Boards[currentBoard];
            UpdateTiles(null, new BoardUpdateEventArgs(Board, null, ChessPiece.None));
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
        }

        public void UpdateTiles(object? sender, BoardUpdateEventArgs e)
        {
            foreach (ChessRow row in Rows.AsEnumerable())
            {
                if (row.RowTiles == null)
                {
                    Logger.EWrite("rowTiles is null");
                    continue;
                }
                foreach (ChessTile tile in row.RowTiles.AsEnumerable())
                    tile?.Update(e.Board);
            }
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
    }
}
