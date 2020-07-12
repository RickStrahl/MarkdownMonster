using System;
using System.Text.RegularExpressions;

namespace mmcli.CommandLine
{

    /// <summary>
    /// Console Color Helper class that provides coloring to individual commands
    /// </summary>
    public static class ColorConsole
    {
        /// <summary>
        /// WriteLine with color
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public static void WriteLine(string text, ConsoleColor? color = null)
        {
            var oldColor = System.Console.ForegroundColor;

            if (color != null)
                Console.ForegroundColor = color.Value;

            Console.WriteLine(text);

            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Writes out a line with a specific color as a string
        /// </summary>
        /// <param name="text">Text to write</param>
        /// <param name="color">A console color. Must match ConsoleColors collection names (case insensitive)</param>
        public static void WriteLine(string text, string color)
        {
            if (string.IsNullOrEmpty(color))
            {
                WriteLine(text);
                return;
            }

            if (!Enum.TryParse(color, true, out ConsoleColor col))
            {
                WriteLine(text);
            }
            else
            {
                WriteLine(text, col);
            }
        }

        /// <summary>
        /// Write with color
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public static void Write(string text, ConsoleColor? color = null)
        {
            var oldColor = Console.ForegroundColor;

            if (color != null)
                Console.ForegroundColor = color.Value;

            Console.Write(text);

            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Writes out a line with color specified as a string
        /// </summary>
        /// <param name="text">Text to write</param>
        /// <param name="color">A console color. Must match ConsoleColors collection names (case insensitive)</param>
        public static void Write(string text, string color)
        {
            if (string.IsNullOrEmpty(color))
            {
                Write(text);
                return;
            }

            if (!ConsoleColor.TryParse(color, true, out ConsoleColor col))
            {
                Write(text);
            }
            else
            {
                Write(text, col);
            }
        }

        #region Wrappers and Templates


        /// <summary>
        /// Writes a line of header text wrapped in a in a pair of lines of dashes:
        /// -----------
        /// Header Text
        /// -----------
        /// and allows you to specify a color for the header. The dashes are colored
        /// </summary>
        /// <param name="headerText">Header text to display</param>
        /// <param name="wrapperChar">wrapper character (-)</param>
        /// <param name="headerColor">Color for header text (yellow)</param>
        /// <param name="dashColor">Color for dashes (gray)</param>
        public static void WriteWrappedHeader(string headerText,
                                                char wrapperChar = '-',
                                                ConsoleColor headerColor = ConsoleColor.Yellow,
                                                ConsoleColor dashColor = ConsoleColor.DarkGray)
        {
            if (string.IsNullOrEmpty(headerText))
                return;

            string line = new string(wrapperChar, headerText.Length);

            WriteLine(line,dashColor);
            WriteLine(headerText, headerColor);
            WriteLine(line,dashColor);
        }

        /// <summary>
        /// Allows a string to be written with embedded color values using:
        /// This is [red]Red[/red] text and this is [cyan]Blue[/blue] text
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="color">Base text color</param>
        public static void WriteEmbeddedColorLine(string text, ConsoleColor? color = null)
        {
            if (color == null)
                color = Console.ForegroundColor;

            if (string.IsNullOrEmpty(text))
            {
                WriteLine(string.Empty);
                return;
            }

            int at = text.IndexOf("[");
            int at2 = text.IndexOf("]");
            if (at == -1 || at2 <= at)
            {
                WriteLine(text, color);
                return;
            }

            while (true)
            {
                var match = Regex.Match(text,"\\[.*?\\].*?\\[/.*?\\]");
                if (match.Length < 1)
                {
                    Write(text, color);
                    break;
                }

                // write up to expression
                Write(text.Substring(0, match.Index), color);

                // strip out the expression
                string highlightText = ExtractString(text, "]", "[");
                string colorVal = ExtractString(text, "[", "]");

                Write(highlightText, colorVal);

                // remainder of string
                text = text.Substring(match.Index + match.Value.Length);
            }

            Console.WriteLine();
        }

        #endregion

        #region Success, Error, Info, Warning Wrappers

        /// <summary>
        /// Write a Success Line - green
        /// </summary>
        /// <param name="text">Text to write out</param>
        public static void WriteSuccess(string text)
        {
            WriteLine(text, ConsoleColor.Green);
        }
        
        /// <summary>
        /// Write a Error Line - Red
        /// </summary>
        /// <param name="text">Text to write out</param>
        public static void WriteError(string text)
        {
            WriteLine(text, ConsoleColor.Red);
        }

        /// <summary>
        /// Write a Warning Line - Yellow
        /// </summary>
        /// <param name="text">Text to Write out</param>
        public static void WriteWarning(string text)
        {
            WriteLine(text, ConsoleColor.DarkYellow);
        }


        /// <summary>
        /// Write a Info Line - dark cyan
        /// </summary>
        /// <param name="text">Text to write out</param>
        public static void WriteInfo(string text)
        {
            WriteLine(text, ConsoleColor.DarkCyan);
        }

        #endregion
     
        
        private static string ExtractString(this string source,
            string beginDelim,
            string endDelim,
            bool allowMissingEndDelimiter = false,
            bool returnDelimiters = false)
        {
            int at1, at2;

            if (string.IsNullOrEmpty(source))
                return string.Empty;

            at1 = source.IndexOf(beginDelim, 0, source.Length, StringComparison.OrdinalIgnoreCase);
            if (at1 == -1)
                return string.Empty;

            at2 = source.IndexOf(endDelim, at1 + beginDelim.Length, StringComparison.OrdinalIgnoreCase);


            if (at1 > -1 && at2 > 1)
            {
                if (!returnDelimiters)
                    return source.Substring(at1 + beginDelim.Length, at2 - at1 - beginDelim.Length);

                return source.Substring(at1, at2 - at1 + endDelim.Length);
            }

            return string.Empty;
        }
    }

}
