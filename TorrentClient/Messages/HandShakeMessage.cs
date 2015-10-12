using TorrentClient.Entities;

namespace TorrentClient.Messages
{
    class HandShakeMessage : MessageBase
    {
        public InfoHash InfoHash { get; set; }
        public string PeerId { get; set; }
        public readonly int ByteLength = 68;

        private string _protocolString = "BitTorrent protocol";
        private readonly static byte[] ZeroedBits = new byte[8];

        public HandShakeMessage()
        {
        }

        public HandShakeMessage(InfoHash infoHash, string peerId)
        {
            InfoHash = infoHash;
            PeerId = peerId;
        }

        public int Encode(byte[] buffer, int offset)
        {
            int written = offset;

            written += Write(buffer, written, (byte)_protocolString.Length);
            written += WriteAscii(buffer, written, _protocolString);
            written += Write(buffer, written, ZeroedBits);

            written += Write(buffer, written, InfoHash.Hash);
            written += WriteAscii(buffer, written, PeerId);

            return written - offset;
        }

        public override void Decode(byte[] buffer, int offset, int length)
        {
            int protocolStringLength = ReadByte(buffer, ref offset);

            if (protocolStringLength != _protocolString.Length)
                protocolStringLength = _protocolString.Length;

            _protocolString = ReadString(buffer, ref offset, protocolStringLength);

            offset += 8;
            InfoHash = new InfoHash(ReadBytes(buffer, ref offset, 20));
            PeerId = ReadString(buffer, ref offset, 20);
        }
    }
}
