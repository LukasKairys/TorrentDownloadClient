namespace TorrentClient.Messages
{
    class UnchokeMessage : MessageBase
    {
        internal static readonly byte MessageId = 1;

        public override string ToString()
        {
            return "UnChokeMessage";
        }

        public override bool Equals(object obj)
        {
            return (obj is UnchokeMessage);
        }
    }
}
