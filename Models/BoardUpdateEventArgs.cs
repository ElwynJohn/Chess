using System;

namespace Chess.Models
{
    public delegate void BoardUpdateEventHandler(object sender, BoardUpdateEventArgs e);

    public class BoardUpdateEventArgs : EventArgs
    {
        public BoardUpdateEventArgs(ChessBoard board, ChessMove move)
        {
            Board = new ChessBoard(board);
            Move = move;
        }
        public ChessBoard? Board { get; }
        public ChessMove? Move { get; }
    }
}
