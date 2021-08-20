using System;

namespace ClientLauncher.Models
{
    public class Configuration
    {
        public string VanillaAmongUsLocation = string.Empty;
        
        public string[] AutodetectedPaths = Array.Empty<string>();
        
        public bool AutoLaunch = false;
        
        //TODO: remove from configuration
        public string CosmeticsServer = "http://rose.hall.ly:2219";
    }
}