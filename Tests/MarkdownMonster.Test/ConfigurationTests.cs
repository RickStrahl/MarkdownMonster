using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class ConfigurationTests
    { 
        [TestMethod]
        public void AddRecentFileTest()
        {
            var model = new AppModel(null);
            mmApp.Model = model;
            Console.WriteLine(mmApp.Configuration.RecentDocumentsLength);
            var config = mmApp.Model.Configuration;
            Console.WriteLine(config.RecentDocuments.Count);


            for (int i = 0; i < 18; i++)
            {
                File.WriteAllText($@"c:\temp\test{i}.txt",".");
            }

            foreach (var f in config.RecentDocuments)
            {
                Console.WriteLine(f);
            }

            config.AddRecentFile(@"c:\temp\test0.txt");
            config.AddRecentFile(@"c:\temp\test1.txt");
            config.AddRecentFile(@"c:\temp\test2.txt");
            config.AddRecentFile(@"c:\temp\test3.txt");
            config.AddRecentFile(@"c:\temp\test4.txt");
            config.AddRecentFile(@"c:\temp\test6.txt");
            config.AddRecentFile(@"c:\temp\test5.txt");
            config.AddRecentFile(@"c:\temp\test7.txt");
            config.AddRecentFile(@"c:\temp\test8.txt");
            config.AddRecentFile(@"c:\temp\test9.txt");
            config.AddRecentFile(@"c:\temp\test10.txt");
            config.AddRecentFile(@"c:\temp\test11.txt");
            config.AddRecentFile(@"c:\temp\test12.txt");
            config.AddRecentFile(@"c:\temp\test13.txt");
            config.AddRecentFile(@"c:\temp\test14.txt");
            config.AddRecentFile(@"c:\temp\test15.txt");
            config.AddRecentFile(@"c:\temp\test16.txt");
            config.AddRecentFile(@"c:\temp\test17.txt");
            config.AddRecentFile(@"c:\temp\test18.txt");

            Assert.IsTrue(config.RecentDocuments.Count == config.RecentDocumentsLength);
            Assert.AreEqual(@"c:\temp\test18.txt", config.RecentDocuments.FirstOrDefault() );
            
            foreach (var f in config.RecentDocuments)
            {
                Console.WriteLine(f);
            }
        }
    }
}

