using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TorrentClient.Messages
{
    public class MessageBase
    {
        public int Write(byte[] buffer, int offset, int value)
        {
            offset += Write(buffer, offset, (byte)(value >> 24));
            offset += Write(buffer, offset, (byte)(value >> 16));
            offset += Write(buffer, offset, (byte)(value >> 8));
            offset += Write(buffer, offset, (byte)(value));
            return 4;
        }

        static public int ReadInt(byte[] buffer, ref int offset)
        {
            int ret = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, offset));
            offset += 4;
            return ret;
        }

        public int Write(byte[] buffer, int offset, byte value)
        {
            buffer[offset] = value;
            return 1;
        }

        public byte ReadByte(byte[] buffer, ref int offset)
        {
            byte b = buffer[offset];
            offset++;
            return b;
        }

        public byte[] ReadBytes(byte[] buffer, ref int offset, int count)
        {
            byte[] result = new byte[count];
            Buffer.BlockCopy(buffer, offset, result, 0, count);
            offset += count;
            return result;
        }

        public string ReadString(byte[] buffer, ref int offset, int count)
        {
            string s = System.Text.Encoding.ASCII.GetString(buffer, offset, count);
            offset += count;
            return s;
        }

        public int WriteAscii(byte[] buffer, int offset, string text)
        {
            for (int i = 0; i < text.Length; i++)
                Write(buffer, offset + i, (byte)text[i]);
            return text.Length;
        }
        public int Write(byte[] buffer, int offset, byte[] value)
        {
            return Write(buffer, offset, value, 0, value.Length);
        }

        public int Write(byte[] dest, int destOffset, byte[] src, int srcOffset, int count)
        {
            Buffer.BlockCopy(src, srcOffset, dest, destOffset, count);
            return count;
        }

        public virtual void Decode(byte[] buffer, int offset, int length)
        {
            
        }

    }
}
