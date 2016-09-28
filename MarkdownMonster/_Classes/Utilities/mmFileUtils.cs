using System.IO;

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

    }
}
