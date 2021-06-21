using Avalonia.Media;

namespace Chess.Models
{
    public class TurnData
    {
        public int Turn { get; set; }
        public ChessMove? WhiteMove { get; set; }
        public ChessMove? BlackMove { get; set; }
        public IBrush? Fill { get; set; }
    }
}
