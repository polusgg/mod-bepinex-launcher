using System;
using System.IO;
using ClientLauncher.Models;
using Octokit;
using Steamworks;
using ApiClient = ClientLauncher.Services.Api.ApiClient;

namespace ClientLauncher
{
    public static class Context
    {
        public static GitHubClient GithubClient { get; set; }
        public static ApiClient ApiClient { get; set; }

        
        public static string BepInExGithubOrg => "NuclearPowered";
        public static string BepInExGithubRepo => "BepInEx";
        public static string BucketUrl => "https://launcher.asset.polus.gg";

        
        public static string DataPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".polusgg-client"
        );

        public static string ConfigPath => Path.Combine(DataPath, "config.json");
        public static string ModdedAmongUsLocation => Path.Combine(DataPath, "modded");


        // public static bool IsLaunchedToCosmetics => SteamApps.CommandLine == "--window=cosmetics";
        public static bool IsLaunchedToCosmetics => false;

        
        public static Configuration Configuration { get; set; } = new();
    }
}