using System;
using System.Net;
using TorrentClient.Messages;

namespace TorrentClient.Entities
{
    public class HaveMessage : MessageBase
    {
        internal static readonly byte MessageId = 4;

        public int PieceIndex
        {
            get { return this.pieceIndex; }
        }
        private int pieceIndex;

        public override void Decode(byte[] buffer, int offset, int length)
        {
            this.pieceIndex = ReadInt(buffer, ref offset);
        }

        static public int ReadInt(byte[] buffer, ref int offset)
        {
            int ret = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, offset));
            offset += 4;
            return ret;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("HaveMessage ");
            sb.Append(" Index ");
            sb.Append(this.pieceIndex);
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            HaveMessage msg = obj as HaveMessage;

            if (msg == null)
                return false;

            return (this.pieceIndex == msg.pieceIndex);
        }
    }
}
