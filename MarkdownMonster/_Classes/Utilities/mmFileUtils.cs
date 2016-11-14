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

    }
}
