using System;
using System.IO;

namespace Metadataload.Controllers
{
    internal class Logger
    {
        private static object fileLock = new object();

        internal static string LogPath { get; set; }

        internal static void LogDebug(string logMessage)
        {
            Log(logMessage, "DEBUG");
        }

        internal static void LogInfo(string logMessage)
        {
            Log(logMessage, "INFO");
        }

        internal static void LogError(string logMessage)
        {
            Log(logMessage, "ERROR");
        }

        internal static void Log(string logMessage, string logLevel)
        {
            //if (Console.CursorLeft > 0)
            //
            //    int currentLineCursor = Console.CursorTop;
            //    Console.SetCursorPosition(0, Console.CursorTop);
            //    Console.Write(new string(' ', Console.WindowWidth));
            //    Console.SetCursorPosition(0, currentLineCursor);
            //}
            
            //Console.WriteLine(logMessage);

            string currentTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            WriteLogFile(logLevel + "\t" + currentTime + " " + logMessage + "\n");

            /*       if (logMessage.StartsWith("Object reference not set to an instance of an object"))
                   {
                       System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
                       WriteLogFile(t.ToString());
                   } */
        }

        internal static void LogProgress(string logMessage)
        {
            HomeController.processes[1].Status = logMessage;
            
            //int currentLineCursor = Console.CursorTop;
            //Console.SetCursorPosition(0, Console.CursorTop);
            //Console.Write(new string(' ', Console.WindowWidth));
            //Console.SetCursorPosition(0, currentLineCursor);
            //Console.Write(logMessage);
        }

        private static void WriteLogFile(string message)
        {
            lock (fileLock)
            {
                File.AppendAllText(LogPath + "Log.txt", message);
            }
        }
    }
}