namespace TorrentClient.Messages
{
    class RequestMessage : MessageBase
    {
        internal static readonly byte MessageId = 6;
        private const int messageLength = 13;

        internal const int BlockSize = (1 << 14);

        private int startOffset;
        private int pieceIndex;
        private int requestLength;

        public RequestMessage(int pieceIndex, int startOffset, int requestLength)
        {
            this.pieceIndex = pieceIndex;
            this.startOffset = startOffset;
            this.requestLength = requestLength;
        }


        #region Methods

        public int Encode(byte[] buffer, int offset)
        {
            int written = offset;

            written += Write(buffer, written, messageLength);
            written += Write(buffer, written, MessageId);
            written += Write(buffer, written, pieceIndex);
            written += Write(buffer, written, startOffset);
            written += Write(buffer, written, requestLength);

            return written - offset;
        }

        public override bool Equals(object obj)
        {
            RequestMessage msg = obj as RequestMessage;
            return (msg == null) ? false : (this.pieceIndex == msg.pieceIndex
                                            && this.startOffset == msg.startOffset
                                            && this.requestLength == msg.requestLength);
        }

        public override string ToString()
        {

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("RequestMessage ");
            sb.Append(" Index ");
            sb.Append(this.pieceIndex);
            sb.Append(" Offset ");
            sb.Append(this.startOffset);
            sb.Append(" Length ");
            sb.Append(this.requestLength);
            return sb.ToString();
        }

        #endregion
    }
}
