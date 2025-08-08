using System;
using System.IO;

namespace SpookySniper.Core
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Success
    }

    public static class LogManager
    {
        private static readonly string LogFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SpookySniper", "logs", $"log_{DateTime.Now:yyyyMMdd}.txt");

        public static event Action<string, LogLevel> LogEntryAdded;

        public static void Initialize()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath));
        }

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] [{level}] {message}";

            // Write to file
            try
            {
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch { /* Ignore file write errors */ }

            // Notify UI
            LogEntryAdded?.Invoke(message, level);
        }
    }
}
