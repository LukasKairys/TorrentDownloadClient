using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentClient.Entities
{
    class Peer
    {
        public Uri PeerUri { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public BitArray Bitfield { get; set; }

        public double PerfectCompleted
        {
            get
            {
                var piecesDownloaded = 0;
                for (int i = 0; i < Bitfield.Length; i++)
                {
                    if (Bitfield[i])
                    {
                        piecesDownloaded++;
                    }
                }
                return (double)piecesDownloaded/(double)Bitfield.Length;
            }
        }

        public Peer(Uri peerUri, string hostname, int port)
        {
            PeerUri = peerUri;
            Hostname = hostname;
            Port = port;
        }
    }
}
