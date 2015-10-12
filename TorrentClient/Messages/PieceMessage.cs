using System;

namespace TorrentClient.Messages
{
    public class PieceMessage : MessageBase
    {

        internal static readonly byte MessageId = 7;
        public const int messageLength = 9;

        private int dataOffset;
        private int pieceIndex;
        private int startOffset;
        private int requestLength;

        public byte[] Data;

        public PieceMessage()
        {
           Data = new byte[0];
        }

        public override void Decode(byte[] buffer, int offset, int length)
        {
            this.pieceIndex = ReadInt(buffer, ref offset);
            this.startOffset = ReadInt(buffer, ref offset);
            this.requestLength = length - offset;

            this.dataOffset = offset;

            this.Data = new byte[requestLength];
            Buffer.BlockCopy(buffer, offset, this.Data, 0, requestLength);
        }

        public override bool Equals(object obj)
        {
            PieceMessage msg = obj as PieceMessage;
            return (msg == null) ? false : (this.pieceIndex == msg.pieceIndex
                                            && this.startOffset == msg.startOffset
                                            && this.requestLength == msg.requestLength);
        }

        public override int GetHashCode()
        {
            return (this.requestLength.GetHashCode()
                ^ this.dataOffset.GetHashCode()
                ^ this.pieceIndex.GetHashCode()
                ^ this.startOffset.GetHashCode());
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("PieceMessage ");
            sb.Append(" Index ");
            sb.Append(this.pieceIndex);
            sb.Append(" Offset ");
            sb.Append(this.startOffset);
            sb.Append(" Length ");
            sb.Append(this.requestLength);
            return sb.ToString();
        }
    }
}