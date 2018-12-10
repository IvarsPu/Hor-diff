using System;
using System.IO;

namespace BusinessLogic
{
    public class Logger
    {
        private  object fileLock = new object();

        public Logger(string rootPath)
        {
            LogPath = rootPath;
        }

        public  string LogPath { get; set; }

        public  void LogDebug(string logMessage)
        {
            Log(logMessage, "DEBUG");
        }

        public  void LogInfo(string logMessage)
        {
            Log(logMessage, "INFO");
        }

        public  void LogError(string logMessage)
        {
            Log(logMessage, "ERROR");
        }

        public void Log(string logMessage, string logLevel)
        {
            string currentTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            WriteLogFile(logLevel + "\t" + currentTime + " " + logMessage + "\n");
        }

        public  void WriteLogFile(string message)
        {
            lock (fileLock)
            {
                File.AppendAllText(LogPath + "Log.txt", message);
            }
        }
    }
}