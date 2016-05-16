using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class ApplicationUpdaterTest  {

        [TestMethod]
        public void CheckVersionFrequencyTest()
        {
            var updates = new ApplicationUpdates();
            updates.LastUpdateCheck =

            mmApp.Configuration.ApplicationUpdates.LastUpdateCheck = DateTime.Now.AddDays(-8);
            mmApp.Configuration.ApplicationUpdates.UpdateFrequency =3;

            var updater = new ApplicationUpdater("0.11");
            Assert.IsTrue(updater.IsNewVersionAvailable(true),"Should show a new version");

            mmApp.Configuration.ApplicationUpdates.LastUpdateCheck = DateTime.Now.AddDays(-2);
            mmApp.Configuration.ApplicationUpdates.UpdateFrequency = 8;

            updater = new ApplicationUpdater("0.11");
            Assert.IsFalse(updater.IsNewVersionAvailable(true),"Should not show a new version because not time to check yet");
        }

        [TestMethod]
        public void DownloadInstallerTest()
        {
            var updater = new ApplicationUpdater("0.11");

            Console.WriteLine(updater.DownloadStoragePath);

            if (File.Exists(updater.DownloadStoragePath))
                File.Delete(updater.DownloadStoragePath);
            
            updater.Download();

            Assert.IsTrue(File.Exists(updater.DownloadStoragePath),"File should have been downloaded to download path");
        }

        [TestMethod]
        public void CheckAndDownloadInstallerTest()
        {
            mmApp.Configuration.ApplicationUpdates.LastUpdateCheck = DateTime.Now.AddDays(-8);
            mmApp.Configuration.ApplicationUpdates.UpdateFrequency = 3;
            
            var updater = new ApplicationUpdater("0.11");

            if (File.Exists(updater.DownloadStoragePath))
                File.Delete(updater.DownloadStoragePath);

            updater.CheckDownloadExecute(true);

            Assert.IsTrue(File.Exists(updater.DownloadStoragePath), "File should have been downloaded to download path");
        }


    }
}
