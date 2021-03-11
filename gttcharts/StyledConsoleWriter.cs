using System;

namespace gttcharts
{
    public static class StyledConsoleWriter
    {
        public static void WriteInfo(string text)
        {
            WriteWithColor(text, ConsoleColor.DarkCyan);
        }

        public static void WriteSuccess(string text)
        {
            WriteWithColor(text, ConsoleColor.DarkGreen);
        }

        public static void WriteWarning(string text)
        {
            WriteWithColor(text, ConsoleColor.DarkYellow);
        }

        public static void WriteError(string text)
        {
            WriteWithColor(text, ConsoleColor.DarkRed);
        }

        private static void WriteWithColor(string text, ConsoleColor color)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = currentColor;
        }
    }
}
