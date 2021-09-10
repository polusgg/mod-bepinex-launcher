using System;
using System.IO;

namespace ClientLauncher.Services
{
    public static class LoggingService
    {
        public static void LogError(string contents)
        {
            try
            {
                Console.WriteLine(contents);
                File.WriteAllText(Path.Combine(Context.DataPath, "launcher.log"), contents);
            }
            catch (Exception) { /* ignored */ }
        }   
    }
}