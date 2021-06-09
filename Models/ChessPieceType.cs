namespace Chess.Models
{
    public enum ChessPieceType
    {
        None = 0,
        Pawn = 1 << 0,
        Knight = 1 << 1,
        Bishop = 1 << 2,
        Castle = 1 << 3,
        Queen = 1 << 4,
        King = 1 << 5,
        IsWhite = 1 << 6
    }
}
