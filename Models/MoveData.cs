namespace Chess.Models
{
    public class MoveData
    {
        public byte OriginFile { get; set; }
        public byte OriginRank { get; set; }
        public byte TargetFile { get; set; }
        public byte TargetRank { get; set; }
        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[4]
            {
                OriginFile, OriginRank, TargetFile, TargetRank
            };
            return bytes;
        }
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

        public MoveData()
        {
        }

        public MoveData(byte[] bytes)
        {
            OriginFile = bytes[0];
            OriginRank = bytes[1];
            TargetFile = bytes[2];
            TargetRank = bytes[3];
        }
    }
}
