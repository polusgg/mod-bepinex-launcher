using System;
using System.IO;

namespace ClientLauncher.Models
{
    public class GameInstall
    {
        public string Location { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".polusgg");
    }
}