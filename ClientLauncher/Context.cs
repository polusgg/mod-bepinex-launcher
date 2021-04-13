using System;
using System.IO;
using ClientLauncher.Models;
using Octokit;

namespace ClientLauncher
{
    public static class Context
    {
        public static GitHubClient? GithubClient { get; set; }

        public static string BepInExGithubOrg { get; } = "NuclearPowered";
        public static string BepInExGithubRepo { get; } = "BepInEx";
        
        
        public static string PreloaderGithubOrg { get; } = "Polusgg";
        public static string PreloaderGithubRepo { get; } = "mod-client-preloader";

        public static string BucketUrl { get; } = "https://polusgg-mod-client.nyc3.digitaloceanspaces.com";
        public static string PolusModPrefix { get; } = "polusgg";

        public static string DataPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".polusgg-client-launcher");

        public static string ConfigPath => Path.Combine(DataPath, "config.json");
        public static Configuration Configuration { get; set; } = new();
    }
}