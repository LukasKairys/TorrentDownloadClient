using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace TorrentClient.Entities
{
    public class TorrentData
    {
        private object receivedDatalock = new object();
        private object bitfieldLock = new object();
        private byte[] totalReceivedData;
        private List<bool> bitfield;

        public byte[] TotalReceivedData
        {
            get
            {
                byte[] result;
                lock (receivedDatalock)
                {
                    result = totalReceivedData;
                }
                return result;
            }
            set
            {
                lock (receivedDatalock)
                {
                    totalReceivedData = value;
                }
            }
        }

        public List<SingleFile> Files { get; set; }
        public int PiecesCount { get; set; }
        public int PieceSize { get; set; }
        public List<bool> Bitfield
        {
            get
            {
                List <bool> result;
                lock (bitfieldLock)
                {
                    result = bitfield;
                }
                return result;
            }
            set
            {
                lock (bitfieldLock)
                {
                    bitfield = value;
                }
            }
        }

        public void FormAndSaveFiles()
        {
            var usedData = 0;

            foreach (var file in Files)
            {
                file.SaveFile(TotalReceivedData.Skip(usedData).Take(file.FileSize).ToArray());
                usedData += file.FileSize;
            }
        }

        public void SavePiece(int pieceIndex, byte[] data)
        {
            var dataSize = data.Length;
            var offset = pieceIndex*PieceSize;

            if (offset + data.Length > TotalReceivedData.Length)
            {
                dataSize = TotalReceivedData.Length - offset;
            }

            Buffer.BlockCopy(data, 0, TotalReceivedData, offset, dataSize);

            Console.WriteLine("Thread" + Thread.CurrentThread.ManagedThreadId + "-> Piece received: " + 
                pieceIndex + ", File downloaded: " + (Math.Round((((double)bitfield.Count(x => x) / (double)bitfield.Count)), 4)) * 100 + "%");
        }
    }
}
