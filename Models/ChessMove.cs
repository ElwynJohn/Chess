using static Chess.Models.ChessBoard;

namespace Chess.Models
{
    public class ChessMove
    {
        /// <summary>0x88 based origin/target positions</summary>
        public byte[] data = new byte[2];

        /// <summary>64 based origin position</summary>
        public int From
        {
            get => Pos64(data[0]);
            set => data[0] = (byte)Pos88(value);
        }
        /// <summary>64 based target position</summary>
        public int To
        {
            get => Pos64(data[1]);
            set => data[1] = (byte)Pos88(value);
        }

        public ChessMove()
        {
        }

        public ChessMove(int from, int to, bool pos0x88 = false)
        {
            System.Console.WriteLine($"ChessMove ctor: {from} {to}");
            if (pos0x88)
            {
                from = ChessBoard.Pos64(from);
                to = ChessBoard.Pos64(to);
            }
            From = from;
            To = to;
        }

        public ChessMove(byte[] bytes)
        {
            data = bytes;
        }

        public override string ToString()
        {
            if (this == default)
                return "";

            char originFile = (char)(((int)'a') + File(From));
            string originRank = (8 - Rank(From)).ToString();
            char targetFile = (char)(((int)'a') + File(To));
            string targetRank = (8 - Rank(To)).ToString();
            return originFile.ToString() + originRank + targetFile.ToString() + targetRank;
        }
    }
}
