using System;
using NLog;

namespace InsightlyToMailChimp.Core
{
    public class LogHelper
    {
        private static readonly Logger Logger;

        static LogHelper()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }

        public static void Debug(string message, bool writeToConsole = true, bool newLine = true)
        {
            Logger.Debug(message);

            if (writeToConsole)
            {
                if (newLine)
                {
                    Console.WriteLine(message);
                }
                else
                {
                    Console.Write(message);
                }
            }
        }

        public static void Error(string message, bool writeToConsole = true, bool newLine = true)
        {
            Logger.Error(message);

            if (writeToConsole)
            {
                if (newLine)
                {
                    Console.WriteLine(message);
                }
                else
                {
                    Console.Write(message);
                }
            }
        }

        public static void DebugHeader(string message)
        {
            message = $"{Environment.NewLine}--------- {message} ---------{Environment.NewLine}";

            Logger.Debug(message);

            Console.WriteLine(message);
        }
    }
}