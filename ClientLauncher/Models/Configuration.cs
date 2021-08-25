using System;

namespace ClientLauncher.Models
{
    public class Configuration
    {
        public string VanillaAmongUsLocation = string.Empty;
        
        public string[] AutodetectedPaths = Array.Empty<string>();
        
        public bool AutoLaunch = false;
    }
}