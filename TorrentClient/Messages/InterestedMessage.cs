namespace TorrentClient.Messages
{
    class InterestedMessage : MessageBase
    {
        internal static readonly byte MessageId = 2;
        private const int messageLength = 1;

        public int Encode(byte[] buffer, int offset)
        {
            int written = offset;

            written += Write(buffer, written, messageLength);
            written += Write(buffer, written, MessageId);

            return written - offset;
        }

        /// <summary>
        /// Returns the length of the message in bytes
        /// </summary>
        public int ByteLength
        {
            get { return (messageLength + 4); }
        }

        public override string ToString()
        {
            return "InterestedMessage";
        }

        public override bool Equals(object obj)
        {
            return (obj is InterestedMessage);
        }
    }
}
