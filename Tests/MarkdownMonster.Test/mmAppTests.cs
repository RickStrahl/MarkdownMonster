using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class mmAppTests
    {
        [TestMethod]
        public void EncryptDecryptStringWithMachineKey()
        {
            string password = "super@Seekrit";

            var encrypted = mmApp.EncryptString(password);
            Assert.IsNotNull(encrypted);
            Console.WriteLine(encrypted);

            var decrypted = mmApp.DecryptString(encrypted);
            Console.WriteLine(decrypted);

            Assert.IsTrue(password == decrypted, "initial and two-way values don't match");
        }

        [TestMethod]
        public void VersionForDisplayTest()
        {
            Assert.AreEqual(mmApp.GetVersionForDisplay("1.70.0.0"), "1.70");
            Assert.AreEqual(mmApp.GetVersionForDisplay("1.70.10.0"), "1.70.10");
            Assert.AreEqual(mmApp.GetVersionForDisplay("1.70.1.0"), "1.70.1");
            Assert.AreEqual(mmApp.GetVersionForDisplay("1.70.1.12"), "1.70.1.12");
        }

        [TestMethod]
        public void FindImageEditorTest()
        {
            var editor = mmFileUtils.FindImageEditor();
            Console.WriteLine(editor);
            Assert.IsNotNull(editor);
        }


        [TestMethod]
        public void ApplicationInsightLogTest()
        {
            mmApp.InitializeLogging();
            mmApp.Log($"Test Message {DateTime.Now}" , new ApplicationException("Nothing to do nowhere to hide"));
            mmApp.ShutdownLogging();
        }
    }
}
