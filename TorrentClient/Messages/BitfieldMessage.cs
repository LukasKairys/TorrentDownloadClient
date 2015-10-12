using System;
using System.Collections;

namespace TorrentClient.Messages

{
    public class BitfieldMessage : MessageBase
    {
        internal static readonly byte MessageId = 5;

        public BitArray Bitfield { get; set; } 

        public BitfieldMessage(int piecesCount)
        {
            Bitfield = new BitArray(piecesCount);
        }


        public override void Decode(byte[] buffer, int offset, int length)
        {
            var bitfieldBytes = new byte[length];
            Buffer.BlockCopy(buffer, offset, bitfieldBytes, 0, length-offset);

            Bitfield = new BitArray(bitfieldBytes);
        }
        public override string ToString()
        {
            return "BitfieldMessage";
        }
    }
}
