using System.Collections.Generic;
using ClientLauncher.Models;

namespace ClientLauncher.Services.GameLocator
{
    public static class GameAutoDetectService
    {
        public static List<GameInstall> LocateGameInstallsAsync()
        {
            var list = new List<GameInstall>();
            if (SteamLocatorService.TryGetGameLocation(out var steamGameLocation))
                list.Add(new GameInstall
                {
                    Location = steamGameLocation 
                });
            
            if (EpicGamesLocatorService.TryGetGameLocation(out var epicGameLocation))
                list.Add(new GameInstall
                {
                    Location = epicGameLocation
                });

            return list;
        }
    }
}