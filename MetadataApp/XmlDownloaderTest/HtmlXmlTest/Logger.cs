namespace HtmlXmlTest
{
    using System;
    using System.IO;

    internal class Logger
    {
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
            Console.WriteLine(logMessage);
            string currentTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Log.txt", logLevel + "\t" + currentTime + " " + logMessage + "\n");
        }
        
        internal static void LogProgress(string logMessage)
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
            Console.Write(logMessage);
        }
    }
}
