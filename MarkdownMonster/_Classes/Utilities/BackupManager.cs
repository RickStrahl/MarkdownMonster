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
    public class BackupManager
    {
        AppModel Model { get;  }

        public BackupManager()
        {
            Model = mmApp.Model;
        }

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

        public bool BackupToFolder(string outputFolder)
        {
            var id = "_" + DataUtils.GenerateUniqueId();
            outputFolder = Path.GetFullPath(outputFolder);
            string common = Model.Configuration.CommonFolder;

            FileUtils.CopyDirectory(common, outputFolder, deepCopy: true);

            FileUtils.DeleteFiles(outputFolder, "*Copy.*", recursive: true);
            FileUtils.DeleteFiles(outputFolder, "*Backup*.*", recursive: true);
            FileUtils.DeleteFiles(outputFolder, "*.bak", recursive: true);
            
            

            return true;
        }


        public bool BackupToGist()
        {
            return true;
        }
    }
}
