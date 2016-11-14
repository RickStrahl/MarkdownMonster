using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MarkdownMonster 
{
    /// <summary>
    /// Internal File Utilities class
    /// </summary>
    public class mmFileUtils
    {
        /// <summary>
        /// Method checks for existance of full filename and tries
        /// to check for file in the initial startup folder.
        /// </summary>
        /// <param name="file">Name of file - fully qualified or local folder file</param>
        /// <returns>filename or null if file doesn't exist</returns>
        public static string FixupDocumentFilename(string file)
        {
            if (File.Exists(file))
                return file;

            var newFile = Path.Combine(App.initialStartDirectory, file);
            if (File.Exists(newFile))
                return newFile;

            return null;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint GetLongPathName(string ShortPath, StringBuilder sb, int buffer);

        /// <summary>
        /// This function returns the actual filename of a file
        /// that exists on disk. If you provide a path/file name
        /// that is not proper cased as input, this function fixes
        /// it up and returns the file using the path and file names
        /// as they exist on disk.
        /// </summary>
        /// <param name="filename">A filename to check</param>
        /// <returns></returns>
	    public static string GetPhysicalPath(string filename)
        {
            try
            {
                StringBuilder sb = new StringBuilder(1500);
                uint result = GetLongPathName(filename, sb, sb.Capacity);
                if (result > 0)
                    filename = sb.ToString();
            }
            catch { }

            return filename;
        }


        /// <summary>
        /// Extracts a string from between a pair of delimiters. Only the first 
        /// instance is found.
        /// </summary>
        /// <param name="source">Input String to work on</param>
        /// <param name="StartDelim">Beginning delimiter</param>
        /// <param name="endDelim">ending delimiter</param>
        /// <param name="CaseInsensitive">Determines whether the search for delimiters is case sensitive</param>
        /// <returns>Extracted string or ""</returns>
        public static string ExtractString(string source,
                                           string beginDelim,
                                           string endDelim,
                                           bool caseSensitive = false,
                                           bool allowMissingEndDelimiter = false,
                                           bool returnDelimiters = false)
        {
            int at1, at2;

            if (string.IsNullOrEmpty(source))
                return string.Empty;

            if (caseSensitive)
            {
                at1 = source.IndexOf(beginDelim);
                if (at1 == -1)
                    return string.Empty;

                at2 = source.IndexOf(endDelim, at1 + beginDelim.Length);                
            }
            else
            {
                //string Lower = source.ToLower();
                at1 = source.IndexOf(beginDelim, 0, source.Length, StringComparison.OrdinalIgnoreCase);
                if (at1 == -1)
                    return string.Empty;
                
                at2 = source.IndexOf(endDelim, at1 + beginDelim.Length, StringComparison.OrdinalIgnoreCase);                
            }

            if (allowMissingEndDelimiter && at2 < 0)
                return source.Substring(at1 + beginDelim.Length);

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
