using System;
using System.IO;

namespace ClientLauncher.Services
{
    public static class LoggingService
    {
        private static string LogFile => Path.Combine(Context.DataPath, "launcher.log");

        public static void Log(string contents)
        {
            try
            {
                Console.WriteLine(contents);
                File.AppendAllText(LogFile, $"{contents}\n\n");
            }
            catch (Exception) { /* ignored */ }
        }
        
        public static void CreateLogFile()
        {
            File.WriteAllText(LogFile, $"--- Log file created at {DateTime.Now:G} ---\n");
        }
    }
}
