using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Westwind.Utilities;

namespace MarkdownMonster.Utilities
{

    /// <summary>
    /// Allows you to back the Markdown Monster Common Folder that
    /// contains all the Markdown Monster and Addin settings to
    /// a folder or zip file.
    /// </summary>
    public class BackupManager
    {
        AppModel Model { get;  }

        public BackupManager()
        {
            Model = mmApp.Model;
        }

        /// <summary>
        /// Backs up the Markdown Monster Common folder to a zip file.
        /// </summary>
        /// <param name="outputZipFile"></param>
        /// <returns></returns>
        public bool BackupToZip(string outputZipFile)
        {
            var id = "_" + DataUtils.GenerateUniqueId();
            string tempPath = Path.Combine(Path.GetTempPath(), id);
            
            try
            {
                BackupToFolder(tempPath);

                if (File.Exists(outputZipFile))
                    File.Delete(outputZipFile);
                ZipFile.CreateFromDirectory(tempPath, outputZipFile, CompressionLevel.Optimal, false);
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath,true);
            }

            return true;
        }

        /// <summary>
        /// Backs up the Markdown Monster Common folder to a new folder
        /// </summary>
        /// <param name="outputFolder"></param>
        /// <returns></returns>
        public bool BackupToFolder(string outputFolder)
        {
            var id = "_" + DataUtils.GenerateUniqueId();
            outputFolder = @"\\?\" + Path.GetFullPath(outputFolder);
            string common = Model.Configuration.CommonFolder;

            try
            {
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);
            }
            catch
            {
                return false;
            }

            FileUtils.CopyDirectory(common, outputFolder, recursive: true);

            FileUtils.DeleteFiles(outputFolder, "*Copy.*", recursive: true);
            FileUtils.DeleteFiles(outputFolder, "*Backup*.*", recursive: true);
            FileUtils.DeleteFiles(outputFolder, "*.bak", recursive: true);

            if( Directory.Exists(Path.Combine(outputFolder,"backups") ))
                Directory.Delete(Path.Combine(outputFolder, "backups"),true);

            return true;
        }


        public bool BackupToGist()
        {
            return true;
        }
    }
}
