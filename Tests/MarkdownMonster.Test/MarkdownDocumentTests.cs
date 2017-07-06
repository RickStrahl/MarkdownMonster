using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    /// <summary>
    /// Summary description for MarkdownDocumentatTests
    /// </summary>
    [TestClass]
    public class MarkdownDocumentTests
    {

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        public string SampleMarkdownFile { get; set; }

        public MarkdownDocumentTests()
        {
            
        }

        string GetSampleMarkdownFile()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SupportFiles", "SampleMarkdown.md");
        }

        [TestMethod]
        public void OpenFile()
        {
            var sampleMarkdownFile = GetSampleMarkdownFile();
            
            var doc = new MarkdownDocument();
            Assert.IsTrue(doc.Load(sampleMarkdownFile));
            Assert.IsTrue(!string.IsNullOrEmpty(doc.CurrentText));
        }

        [TestMethod]
        public void OpenSaveFile()
        {
            var sampleMarkdownFile = GetSampleMarkdownFile();
            var saveFile = sampleMarkdownFile.Replace(".md", "_saved.md");

            var doc = new MarkdownDocument();
            Assert.IsTrue(doc.Load(sampleMarkdownFile));

            Assert.IsTrue(!string.IsNullOrEmpty(doc.CurrentText));
            
            Assert.IsTrue(doc.Save(saveFile));
            Assert.IsTrue(File.Exists(saveFile));

            File.Delete(saveFile);
        }


        [TestMethod]
        public void OpenSaveEncryptedFile()
        {
            var sampleMarkdownFile = GetSampleMarkdownFile();
            var saveFile = sampleMarkdownFile.Replace(".md", "_saved.md");
            var password = new SecureString();
            password.AppendChar('s');
            password.AppendChar('e');
            password.AppendChar('k');
            password.AppendChar('r');
            password.AppendChar('i');
            password.AppendChar('t');


            var doc = new MarkdownDocument();
            Assert.IsTrue(doc.Load(sampleMarkdownFile));
            Assert.IsTrue(!string.IsNullOrEmpty(doc.CurrentText));

            string text = doc.CurrentText;

            Assert.IsTrue(doc.Save(saveFile,password: password));
            Assert.IsTrue(File.Exists(saveFile));

            Console.WriteLine(File.ReadAllText(saveFile));

            doc = new MarkdownDocument();
            Assert.IsTrue(doc.Load(saveFile, password: password));
            Assert.IsTrue(text == doc.CurrentText,"Decrypted text doesn't match");

            

            File.Delete(saveFile);
        }
    }
}
