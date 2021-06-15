namespace Chess.Models
{
    public class MoveData
    {
        public byte OriginFile { get; set; }
        public byte OriginRank { get; set; }
        public byte TargetFile { get; set; }
        public byte TargetRank { get; set; }
        public string Move
        {
            get
            {
                if (this == default)
                    return "";

                char originFile = (char)(((int)'a') + OriginFile);
                string originRank = (8 - OriginRank).ToString();
                char targetFile = (char)(((int)'a') + TargetFile);
                string targetRank = (8 - TargetRank).ToString();
                return originFile.ToString() + originRank + targetFile.ToString() + targetRank;
            }
        }
    }
}
