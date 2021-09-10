using System;
using System.IO;
using ClientLauncher.Models;
using ClientLauncher.MonkePatches;
using Octokit;
using Steamworks;
using ApiClient = ClientLauncher.Services.Api.ApiClient;

namespace ClientLauncher
{
    public static class Context
    {
        public static MonkePatchLoader MonkePatchLoader { get; set; }
        

        public static GitHubClient GithubClient { get; set; }
        public static ApiClient ApiClient { get; set; }

        
        public static string BepInExGithubOrg => "NuclearPowered";
        public static string BepInExGithubRepo => "BepInEx";
        public static string BucketUrl => "https://launcher.asset.polus.gg";
        
        //TODO: hardcode value later
        public static string CosmeticsUrl => "http://cosmetics.service.polus.gg:2219";
        public static string AccountServerUrl => "https://account.polus.gg";

        public static string DataPath => SteamClient.IsValid ? SteamInstallDirDataPath : AppdataDataPath;
        
        private static string AppdataDataPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".polusgg-client"
        );

        private static string SteamInstallDirDataPath => SteamApps.AppInstallDir(new AppId { Value = 1653240 });
        

        public static string ConfigPath => Path.Combine(AppdataDataPath, "config.json");
        public static string ModdedAmongUsLocation => Path.Combine(DataPath, "modded");
        public static string CachePath => Path.Combine(DataPath, "cache");


        public static bool IsLaunchedToCosmetics => SteamClient.IsValid && SteamApps.CommandLine == "--window=cosmetics";

        
        public static Configuration Configuration { get; set; } = new();
    }
}