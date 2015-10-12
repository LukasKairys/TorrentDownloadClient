using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TorrentClient.Entities;


namespace TorrentClient.Managers
{
    static class HttpTracker
    {
        public static List<Peer> SendRequestToTracker(Dictionary<string, string> parameters, Torrent torrent)
        {
            var uri = BuildUri(new Uri(torrent.AnnounceUrl), parameters);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (Stream resStream = response.GetResponseStream())
            {
                var byteResp = ReadFully(resStream);
                var decoded = (Dictionary<string, object>)Bencode.BencodeUtility.Decode(byteResp);

                var peersList = Decode((byte[])decoded["peers"]);

                return peersList;
            }
        }

        private static Uri BuildUri(Uri uri, Dictionary<string, string> parameters)
        {
            string result = "";
            var builder = new System.UriBuilder(uri);
            foreach (KeyValuePair<string, string> keypair in parameters)
                result += keypair.Key + "=" + keypair.Value + "&";
            builder.Query = result.Length == 0 ? result : result.Remove(result.Length - 1);
            return builder.Uri;
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static List<Peer> Decode(byte[] peers)
        {
            // "Compact Response" peers are encoded in network byte order. 
            // IP's are the first four bytes
            // Ports are the following 2 bytes
            byte[] byteOrderedData = peers;
            int i = 0;
            UInt16 port;
            StringBuilder sb = new StringBuilder(27);
            List<Peer> list = new List<Peer>((byteOrderedData.Length / 6) + 1);
            while ((i + 5) < byteOrderedData.Length)
            {
                sb.Remove(0, sb.Length);

                sb.Append(byteOrderedData[i++]);
                sb.Append('.');
                sb.Append(byteOrderedData[i++]);
                sb.Append('.');
                sb.Append(byteOrderedData[i++]);
                sb.Append('.');
                sb.Append(byteOrderedData[i++]);

                port = (UInt16)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteOrderedData, i));
                i += 2;
                string hostname = sb.ToString();
                sb.Append(':');
                sb.Append(port);

                Uri uri = new Uri("tcp://" + sb);
                list.Add(new Peer(uri, hostname, port));
            }

            return list;
        }
    }
}
