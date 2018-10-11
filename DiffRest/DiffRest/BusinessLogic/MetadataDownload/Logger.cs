using System;
using System.IO;

namespace BusinessLogic
{
    public class Logger
    {
        private static object fileLock = new object();

        public static string LogPath { get; set; }

        public static void LogDebug(string logMessage)
        {
            Log(logMessage, "DEBUG");
        }

        public static void LogInfo(string logMessage)
        {
            Log(logMessage, "INFO");
        }

        public static void LogError(string logMessage)
        {
            Log(logMessage, "ERROR");
        }

        public static void Log(string logMessage, string logLevel)
        {
            string currentTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            WriteLogFile(logLevel + "\t" + currentTime + " " + logMessage + "\n");
        }

        public static void WriteLogFile(string message)
        {
            lock (fileLock)
            {
                File.AppendAllText(LogPath + "Log.txt", message);
            }
        }
    }
}