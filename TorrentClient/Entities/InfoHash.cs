using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using TorrentClient.Helpers;

namespace TorrentClient.Entities
{
    public class InfoHash : IEquatable<InfoHash>
    {
        static Dictionary<char, byte> base32DecodeTable;

        static InfoHash()
        {
            base32DecodeTable = new Dictionary<char, byte>();
            string table = "abcdefghijklmnopqrstuvwxyz234567";
            for (int i = 0; i < table.Length; i++)
                base32DecodeTable[table[i]] = (byte)i;
        }

        byte[] hash;

        internal byte[] Hash
        {
            get { return hash; }
        }

        public InfoHash(byte[] infoHash)
        {
            if (infoHash.Length != 20)
                throw new ArgumentException("Infohash must be exactly 20 bytes long");
            hash = (byte[])infoHash.Clone();
        }

        public string UrlEncode()
        {
            return UriHelper.UrlEncode(Hash);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InfoHash);
        }

        public bool Equals(InfoHash other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            // Equality is based generally on checking 20 positions, checking 4 should be enough
            // for the hashcode as infohashes are randomly distributed.
            return Hash[0] | (Hash[1] << 8) | (Hash[2] << 16) | (Hash[3] << 24);
        }

        public override string ToString()
        {
            return BitConverter.ToString(hash);
        }

        public static bool operator ==(InfoHash left, InfoHash right)
        {
            if ((object)left == null)
                return (object)right == null;
            if ((object)right == null)
                return false;
            return ByteMatch(left.Hash, right.Hash);
        }

        public static bool operator !=(InfoHash left, InfoHash right)
        {
            return !(left == right);
        }

        public static bool ByteMatch(byte[] array1, byte[] array2)
        {
            if (array1 == null)
                throw new ArgumentNullException("array1");
            if (array2 == null)
                throw new ArgumentNullException("array2");

            if (array1.Length != array2.Length)
                return false;

            return ByteMatch(array1, 0, array2, 0, array1.Length);
        }

        public static bool ByteMatch(byte[] array1, int offset1, byte[] array2, int offset2, int count)
        {
            if (array1 == null)
                throw new ArgumentNullException("array1");
            if (array2 == null)
                throw new ArgumentNullException("array2");

            // If either of the arrays is too small, they're not equal
            if ((array1.Length - offset1) < count || (array2.Length - offset2) < count)
                return false;

            // Check if any elements are unequal
            for (int i = 0; i < count; i++)
                if (array1[offset1 + i] != array2[offset2 + i])
                    return false;

            return true;
        }

    }
}

