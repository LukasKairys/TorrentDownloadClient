using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using TorrentClient.Entities;
using TorrentClient.Messages;

namespace TorrentClient.Managers
{
    class PeerManager
    {
        private Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public Torrent Torrent { get; set; }
        public MessagesManager MessagesManager { get; set; }
        public List<SingleFile> Files { get; set; } 
        public Peer Peer { get; set; } 

        public PeerManager(MessagesManager messagesManager, List<SingleFile> files)
        {
            MessagesManager = messagesManager;
            Files = files;
        }

        public void ConnectToPeer(Peer peer, Torrent torrent, string clientPeerId)
        {
            Peer = peer;
            Torrent = torrent;
            Peer.Bitfield = new BitArray(torrent.PiecesCount);
            client.ReceiveTimeout = 5000;
            client.SendTimeout = 5000;

            var handShakeMessage = new HandShakeMessage(Torrent.InfoHash, clientPeerId);
            client = Connect(peer);

            if (client.Connected)
            {
                Console.WriteLine("Thread" + Thread.CurrentThread.ManagedThreadId + "-> Connected to peer, trying to communicate: " + peer.PeerUri);
                Communicate(handShakeMessage);
            }  
        }

        private void Communicate(HandShakeMessage handShakeMessage)
        {
            Peer.Bitfield = new BitArray(Torrent.PiecesCount);
            byte[] buffer = new byte[68];
            byte[] receivedByteStream = new byte[68];
            int receivedLength;

            try
            {
                handShakeMessage.Encode(buffer, 0);
                client.Send(buffer);
                client.Receive(receivedByteStream, 0, handShakeMessage.ByteLength, SocketFlags.None);

                var receivedHandShakeMessage = new HandShakeMessage();
                receivedHandShakeMessage.Decode(receivedByteStream, 0, handShakeMessage.ByteLength);

                if (Torrent.InfoHash == receivedHandShakeMessage.InfoHash)
                {
                    Console.WriteLine("Thread" + Thread.CurrentThread.ManagedThreadId + "-> HANDSHAKE SUCCESSFULL");

                    var interestedMessage = new InterestedMessage();

                    buffer = new byte[interestedMessage.ByteLength];
                    interestedMessage.Encode(buffer, 0);

                    client.Send(buffer);

                    while (MessagesManager.receivedMessages.Where(x => x is UnchokeMessage).FirstOrDefault() == null)
                    {
                        receivedByteStream = new byte[256];
                        receivedLength = client.Receive(receivedByteStream);

                        MessagesManager.GotStream(receivedByteStream, receivedLength);
                    }

                    var bitfieldMessage = ((BitfieldMessage)MessagesManager.receivedMessages.Where(x => x is BitfieldMessage).FirstOrDefault());
                    if (bitfieldMessage != null)
                    {
                        Peer.Bitfield = bitfieldMessage.Bitfield;
                    }

                    fillBitfieldFromHaveMessages((MessagesManager.receivedMessages.OfType<HaveMessage>()).ToList());

                    var pieceIndex = 0;

                    while (Torrent.TorrentData.Bitfield.Any(b => b == false))
                    {
                        try
                        {
                            pieceIndex = Torrent.TorrentData.Bitfield.FindIndex(val => val == false);
                            //if(Peer.Bitfield[pieceIndex])
                            //{
                                Torrent.TorrentData.Bitfield[pieceIndex] = true;
                                var piece = ReceivePiece(pieceIndex);
                                Torrent.TorrentData.SavePiece(pieceIndex, piece);
                            //}
                            
                        }
                        catch (Exception e)
                        {
                            Torrent.TorrentData.Bitfield[pieceIndex] = false;
                        }
                     }
                }
            }
            catch (Exception e)
            {
                // TODO: Log error
            }
        }

        private byte[] ReceivePiece(int pieceIndex)
        {
            var buffer = new byte[256];
            var receivedByteStream = new byte[Torrent.BlockSize];
            var pieceData = new byte[Torrent.PieceLength];
            var offset = 0;
            int receivedLength;
            var receivedPieceSize = 0;

            for (int j = 0; j < Torrent.BlockCount; j++)
            {
                var requestMessage = new RequestMessage(pieceIndex, Torrent.BlockSize * j, Convert.ToInt32(Torrent.BlockSize));

                buffer = new byte[256];
                requestMessage.Encode(buffer, 0);
                client.Send(buffer, 0, 17, SocketFlags.None);

                receivedByteStream = new byte[Torrent.BlockSize];
                receivedLength = client.Receive(receivedByteStream);

                MessagesManager.GotStream(receivedByteStream, receivedLength);

                while (MessagesManager.currentLength != 0)
                {
                    receivedByteStream = new byte[Torrent.BlockSize];
                    receivedLength = client.Receive(receivedByteStream);

                    MessagesManager.GotStream(receivedByteStream, receivedLength);
                }

                var pieceMessage = (PieceMessage)MessagesManager.receivedMessages.Last();

                Buffer.BlockCopy(pieceMessage.Data, 0, pieceData, offset, pieceMessage.Data.Length);
                offset += pieceMessage.Data.Length;
            }

            return pieceData;
        }

        private void fillBitfieldFromHaveMessages(List<HaveMessage> messages)
        {
            foreach (var message in messages)
            {
                Peer.Bitfield[message.PieceIndex] = true;
            }
        }

        private Socket Connect(Peer peer)
        {
            try
            {
                var ipHostInfo = Dns.GetHostEntry(peer.Hostname);
                var ipAdress = ipHostInfo.AddressList[0];
                var remoteEP = new IPEndPoint(ipAdress, peer.Port);

                var ping = new Ping().Send(ipAdress, 2000);

                if (ping.Status == IPStatus.Success)
                {
                    client.Connect(remoteEP);
                }
                
            }
            catch (Exception e)
            {
                // TODO: Log error
            }

            return client;
        }
    }
}
