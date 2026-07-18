using System;
using System.IO;

namespace BookStoreManagement.Helpers
{
    public static class Logger
    {
        private static readonly string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        public static void Error(Exception ex, string message = "")
        {
            try
            {
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string logFile = Path.Combine(logDirectory, $"error_{DateTime.Now:yyyyMMdd}.log");
                string logMessage = $"[{DateTime.Now:HH:mm:ss}] ERROR: {message}\n" +
                                    $"Exception: {ex.Message}\n" +
                                    $"StackTrace: {ex.StackTrace}\n" +
                                    new string('-', 80) + "\n";

                File.AppendAllText(logFile, logMessage);
            }
            catch
            {
                // Fallback, swallow logger exceptions to avoid infinite loops
            }
        }
    }
}
