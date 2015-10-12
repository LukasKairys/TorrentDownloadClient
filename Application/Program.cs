using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorrentClient;

namespace Application
{
    class Program
    {
        static void Main(string[] args)
        {
            string torrentsPath = @"C:\Users\Lukas Kairys\Desktop\Torrents";
            string downloadsPath = @"C:\Users\Lukas Kairys\Desktop\Downloads\";

            var client = new TorrentClient.TorrentClient(torrentsPath, downloadsPath);
            client.StartDownloading();
        }
    }
}
