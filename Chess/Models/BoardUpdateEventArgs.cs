using System;

namespace Chess.Models
{
    public delegate void BoardUpdateEventHandler(object sender, BoardUpdateEventArgs e);

    public class BoardUpdateEventArgs : EventArgs
    {
        public BoardUpdateEventArgs(ChessBoard board, ChessMove? move, ChessPiece pieceTaken)
        {
            Board = new ChessBoard(board);
            Move = move;
            PieceTaken = pieceTaken;
        }
        public ChessBoard Board { get; }
        public ChessMove? Move { get; }
        public ChessPiece PieceTaken { get; }
    }
}
