using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;

namespace TorrentClient.Entities
{
    public class Torrent
    {
        public string AnnounceUrl { get; set; }
        public int PieceLength { get; set; }
        public string Name { get; set; }
        public int PiecesCount { get; set; }
        public int BlockCount { get; set; }

        public int BlockSize = 16384;
        public List<string> PiecesHash { get; set; }
        public List<SingleFile> Files = new List<SingleFile>();
        public InfoHash InfoHash { get; set; }
        public Int64 TotalLength { get; set; }
        public TorrentData TorrentData { get; set; }


        public Torrent(object torrentObj)
        {
            var torrentDic = (Dictionary<string, object>) torrentObj;
            var infoDic = (Dictionary<string, object>) torrentDic["info"];
            var piecesString = (System.Text.Encoding.Default.GetString((byte[])infoDic["pieces"]));
            var infoBencodedValue = Bencode.BencodeUtility.Encode(torrentDic["info"]);        

            AnnounceUrl = System.Text.Encoding.Default.GetString((byte[]) torrentDic["announce"]);
            PieceLength = Convert.ToInt32(infoDic["piece length"]);
            Name = System.Text.Encoding.Default.GetString((byte[])infoDic["name"]);
            PiecesCount = ((byte[])infoDic["pieces"]).Length / 20;
            PiecesHash = Enumerable.Range(0, piecesString.Length / 20).Select(i => piecesString.Substring(i * 20, 20)).ToList();
            BlockCount = (PieceLength / BlockSize) + ((PieceLength % BlockSize) > 0 ? 1 : 0);

            using (SHA1Managed sha1 = new SHA1Managed())
                InfoHash = new InfoHash(sha1.ComputeHash(infoBencodedValue.Select(i => i).ToArray()));

            try
            {
                var filesDic = (List<object>)infoDic["files"];
                foreach (var fileObj in filesDic)
                {
                    var file = (Dictionary<string, object>)fileObj;
                    var singleFile = new SingleFile
                    {
                        FileSize = Convert.ToInt32(file["length"]),
                        Name = System.Text.Encoding.Default.GetString((byte[])(((List<object>)file["path"])[0]))
                    };

                    Files.Add(singleFile);
                    TotalLength += Convert.ToInt32(file["length"]);
                }
            }
            catch
            {
                TotalLength = (Int64)infoDic["length"];
            }

            var emptyBitfield = new List<bool>(PiecesCount);
            for (int i = 0; i < PiecesCount; i++)
            {
                emptyBitfield.Add(false);
            }

            TorrentData = new TorrentData
            {
                TotalReceivedData = new byte[TotalLength],
                Files = Files,
                PiecesCount = PiecesCount,
                Bitfield = emptyBitfield,
                PieceSize = PieceLength
            };
        }
    }
}
