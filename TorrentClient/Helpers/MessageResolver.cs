using System;
using System.Collections.Generic;
using System.Linq;
using TorrentClient.Entities;
using TorrentClient.Messages;

namespace TorrentClient.Helpers
{
    public class MessageResolver
    {
        private Dictionary<byte, Func<MessageBase>> _messageDict;
        public MessageResolver(int bitfieldLength)
        {
            _messageDict = new Dictionary<byte, Func<MessageBase>>();

            _messageDict.Add(HaveMessage.MessageId, () => new HaveMessage());
            _messageDict.Add(BitfieldMessage.MessageId, () => new BitfieldMessage(bitfieldLength));
            _messageDict.Add(InterestedMessage.MessageId, () => new InterestedMessage());
            _messageDict.Add(PieceMessage.MessageId, () => new PieceMessage());
            _messageDict.Add(UnchokeMessage.MessageId, () => new UnchokeMessage());

        }
        public MessageBase DecodeMessage(byte[] buffer, int offset, int count)
        {
            MessageBase message = new MessageBase();

            if (count < 4)
                throw new ArgumentException("A message must contain a 4 byte length prefix");

            if (_messageDict.ContainsKey(buffer[offset + 4]))
            {
                message = _messageDict[buffer[offset + 4]].Invoke();
                message.Decode(buffer, offset+5, count);
            }

            return message;
        }
    }
}
