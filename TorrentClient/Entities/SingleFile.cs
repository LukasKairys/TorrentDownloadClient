using System;
using System.Collections.Generic;
using System.IO;
using TorrentClient.Messages;

namespace TorrentClient.Entities
{
    public class SingleFile
    {
        public string Name { get; set; }
        public int FileSize { get; set; }
        public string Path = @"C:\Users\Lukas Kairys\Desktop\Downloads\";

        public void SaveFile(Byte[] data)
        {
            File.WriteAllBytes(Path + Name, data);
        }
    }
}
