using System;
using System.Collections.Generic;
using System.Linq;
using TorrentClient.Helpers;
using TorrentClient.Messages;

namespace TorrentClient.Managers
{
    class MessagesManager
    {
        public int currentLength;
        private byte[] bytesStream = new byte[65600];
        private int currentMessageLength;
        public List<MessageBase> receivedMessages = new List<MessageBase>();
        private MessageResolver messageResolver;

        public MessagesManager(MessageResolver messageResolver)
        {
            this.messageResolver = messageResolver;
        }

        public void GotStream(byte[] messagePart, int lengthRead)
        {
            Buffer.BlockCopy(messagePart, 0, bytesStream, currentLength, lengthRead);
            currentLength += lengthRead;

            SetCurrentMessageLength();
            ChechIfWeGotFullMessage();
        }

        private void SetCurrentMessageLength()
        {
            if (currentLength > 4)
            {
                var length = bytesStream.Take(4).ToArray();

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(length);

                currentMessageLength = BitConverter.ToInt32(length, 0) + 4;
            }
            else
            {
                currentLength = 0;
            }
        }

        private void ChechIfWeGotFullMessage()
        {
            while(currentLength >= currentMessageLength)
            {
                FormMessage();
            }
        }

        private void FormMessage()
        {
            var message = new byte[currentMessageLength];
            Buffer.BlockCopy(bytesStream, 0, message, 0, currentMessageLength);

            receivedMessages.Add(messageResolver.DecodeMessage(message, 0, currentMessageLength));

            RemoveOldMessage();
        }

        private void RemoveOldMessage()
        {
            var newBytesStream = new byte[65600];

            Buffer.BlockCopy(bytesStream, currentMessageLength, newBytesStream, 0, 65600 - currentMessageLength);
            currentLength = currentLength - currentMessageLength;
            bytesStream = newBytesStream;

            SetCurrentMessageLength();
        }
    }
}
