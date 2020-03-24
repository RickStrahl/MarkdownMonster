using System;
using System.Text;
using Westwind.Utilities;

namespace mmcli.CommandLine
{

    /// <summary>
    /// Console Helper class that provides coloring to individual commeands
    /// </summary>
    public static class ConsoleHelper
    {

        /// <summary>
        /// WriteLine with color
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public static void WriteLine(string text, ConsoleColor color = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Write with color
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public static void Write(string text, ConsoleColor color = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = oldColor;
        }


        /// <summary>
        /// Write a Success Line - green
        /// </summary>
        /// <param name="text"></param>
        public static void WriteSuccess(string text)
        {
            WriteLine(text, ConsoleColor.Green);
        }

        
        /// <summary>
        /// Write a Error Line - Red
        /// </summary>
        /// <param name="text"></param>
        public static void WriteError(string text)
        {
            WriteLine(text, ConsoleColor.Red);
        }


        /// <summary>
        /// Write a Info Line - dark cyan
        /// </summary>
        /// <param name="text"></param>
        public static void WriteInfo(string text)
        {
            WriteLine(text, ConsoleColor.DarkCyan);
        }

        /// <summary>
        /// Write a Warning Line - Yellow
        /// </summary>
        /// <param name="text"></param>
        public static void WriteWarning(string text)
        {
            WriteLine(text, ConsoleColor.Yellow);
        }

        public static void WriteWrappedHeader(string headerText, char wrapperChar = '-', ConsoleColor headerColor = ConsoleColor.Yellow)
        {
            string line = new StringBuilder().Insert(0, wrapperChar.ToString(), headerText.Length).ToString();    

           Console.WriteLine(line);
            WriteLine(headerText,headerColor);
            Console.WriteLine(line);
        }
    }
}
