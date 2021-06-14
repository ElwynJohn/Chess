namespace Chess.Models
{
    public readonly struct MoveData
    {
        public readonly byte OriginFile { get; init; }
        public readonly byte OriginRank { get; init; }
        public readonly byte TargetFile { get; init; }
        public readonly byte TargetRank { get; init; }
    }
}
