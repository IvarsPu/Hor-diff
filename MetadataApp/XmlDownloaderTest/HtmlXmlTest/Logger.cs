namespace HtmlXmlTest
{
    using System;
    using System.IO;

    internal class Logger
    {
        internal static void Log(string logMessage)
        {
            Console.WriteLine(logMessage);
            string currentTime = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
            File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Log.txt", currentTime + " : " + logMessage + "\n");
        }
    }
}
