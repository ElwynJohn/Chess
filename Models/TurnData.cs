using Avalonia.Media;

namespace Chess.Models
{
    public class TurnData
    {
        public int Turn { get; set; }
        public MoveData WhiteMove { get; set; }
        public MoveData BlackMove { get; set; }
        public IBrush? Fill { get; set; }
    }
}
