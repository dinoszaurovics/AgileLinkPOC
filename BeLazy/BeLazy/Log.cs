﻿using System;

namespace BeLazy
{
    internal class Log
    {
        internal static void AddLog(string message, ErrorLevels errorLevel = ErrorLevels.Information)
        {
            Console.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - " + errorLevel + " - " + message);
        }
    }

    internal enum ErrorLevels { Fatal, Critical, Error, Warning, Information }

}