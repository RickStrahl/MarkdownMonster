using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JoeBlogs.helpers
{
    static class FileSystemHelper
    {
        /// <summary>
        /// Reads a file from a given path and returns a byte array.
        /// Should not be used with very large files (100mb+) due to possible memory issues.
        /// </summary>
        /// <param name="pathToFile"></param>
        /// <returns></returns>
        public static byte[] GetFileBytes(string pathToFile)
        {
            var fs = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
            byte[] filebytes = new byte[fs.Length];
            fs.Read(filebytes, 0, Convert.ToInt32(fs.Length));
            return filebytes;
        }
    }
}
