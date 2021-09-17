using System;
using System.IO;

namespace ClientLauncher.Services
{
    public static class LoggingService {
        public static string LogFile => Path.Combine(Context.DataPath, "launcher.log");
        public static void ClearLog() {
            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }
        }
        public static void LogError(string contents)
        {
            try
            {
                Console.WriteLine(contents);
                File.AppendAllText(LogFile, $"{contents}\n\n");
            }
            catch (Exception) { /* ignored */ }
        }   
    }
}