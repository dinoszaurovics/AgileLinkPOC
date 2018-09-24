using System;

namespace AgileLink
{
    internal class Log
    {
        internal static void AddLog(string message, ErrorLevels errorLevel = ErrorLevels.Information)
        {
            Console.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - " + errorLevel + " - " + message);
        }
    }
}