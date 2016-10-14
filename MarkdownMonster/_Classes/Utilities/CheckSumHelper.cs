using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Westwind.Utilities;

namespace MarkdownMonster._Classes.Utilities
{
    public class ChecksumHelper
    {
        /// <summary>
        /// Creates an SHA256 checksum of a file
        /// </summary>
        /// <param name="file"></param>        
        /// <returns></returns>
        public static string GetChecksumFromFile(string file)
        {
            if (!File.Exists(file))
                return null;

            try
            {
                byte[] checkSum;
                using (FileStream stream = File.Open(file,FileMode.Open,FileAccess.Read,FileShare.ReadWrite)) 
                {
                    var md = new MD5CryptoServiceProvider();
                    checkSum = md.ComputeHash(stream);
                }

                return StringUtils.BinaryToBinHex(checkSum);
            }
            catch
            {
                return null;
            }
        }        
    }
}
