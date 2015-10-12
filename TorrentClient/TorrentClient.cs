using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TorrentClient.Entities;
using TorrentClient.Helpers;
using TorrentClient.Managers;

namespace TorrentClient
{
    public class TorrentClient
    {
        private const int ThreadsCount = 4;
        private string _torrentsPath;
        private string _downloadsPath;
        private string _peerId;
        public List<SingleFile> files;


        public TorrentClient(string torrentsPath, string downloadsPath)
        {
            _torrentsPath = torrentsPath;
            _downloadsPath = downloadsPath;

            GenerateAndSavePeerId();
        }

        public void StartDownloading()
        {
            var torrentFilePath = Directory.GetFiles(_torrentsPath).First();
            var torrent = LoadTorrent(torrentFilePath);
            files = torrent.Files;

            var parameters = GetTrackerParameters(torrent);
            var peers = HttpTracker.SendRequestToTracker(parameters, torrent);

            DownloadFromPeers(peers, torrent);
        }

        private void DownloadFromPeers(List<Peer> peers, Torrent torrent)
        {
            var peersPerThread = peers.Count/ThreadsCount;

            Task[] tasks = new Task[ThreadsCount];

            while (torrent.TorrentData.Bitfield.Any(b => b == false))
            {
                tasks[0] = Task.Run(() => BeginDownload(peers.Skip(0).Take(peersPerThread).ToList(), torrent));
                tasks[1] = Task.Run(() => BeginDownload(peers.Skip(peersPerThread).Take(peersPerThread).ToList(), torrent));
                tasks[2] = Task.Run(() => BeginDownload(peers.Skip(2 * peersPerThread).Take(peersPerThread).ToList(), torrent));
                tasks[3] = Task.Run(() => BeginDownload(peers.Skip(3 * peersPerThread).Take(peers.Count - 3 * peersPerThread).ToList(), torrent));

                Task.WaitAll(tasks);
            }

            torrent.TorrentData.FormAndSaveFiles();

        }

        private void BeginDownload(List<Peer> peers, Torrent torrent)
        {
            int peerIndex = 0;
            var isBitfieldNotFull = true;

            MessagesManager messagesManager = new MessagesManager(new MessageResolver(torrent.PiecesCount));


            while (peers.Count > peerIndex && isBitfieldNotFull)
            {
                try
                {
                    isBitfieldNotFull = torrent.TorrentData.Bitfield.Any(b => b == false);
                }
                catch
                {
                    continue;
                }

                Console.WriteLine("Thread" + Thread.CurrentThread.ManagedThreadId + "-> Trying to connect to peer: " + peers[peerIndex].PeerUri);
                PeerManager peerManager = new PeerManager(messagesManager, files);
                peerManager.ConnectToPeer(peers[peerIndex], torrent, _peerId);

                peerIndex++;
            }
        }

        private Torrent LoadTorrent(string path)
        {
            object result;

            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            result = Bencode.BencodeUtility.Decode(File.ReadAllBytes(path));
            var torrentInfo = new Torrent(result);

            return torrentInfo;        
        }

        private Dictionary<string, string> GetTrackerParameters(Torrent torrent)
        {
            var left = torrent.TotalLength;

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("info_hash", torrent.InfoHash.UrlEncode());
            parameters.Add("peer_id", _peerId);
            parameters.Add("port", "5008");
            parameters.Add("left", left.ToString());
            parameters.Add("compact", "1");
            parameters.Add("numwant", "100");

            return parameters;
        }

        private void GenerateAndSavePeerId()
        {
            _peerId =  "-MO1001-" + DateTime.Now.ToString("yyMMddHHmmss");
        }
        
    }
}
