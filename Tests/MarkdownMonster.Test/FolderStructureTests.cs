using System;
using System.IO;
using MarkdownMonster.Utilities;
using MarkdownMonster.Windows;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
	[TestClass]
	public class FolderStructureTests
	{
		public TestContext TestContext { get; set; }

		private static TestContext _testContext;

		[ClassInitialize]
		public static void SetupTests(TestContext testContext)
		{
			_testContext = testContext;
		}

		[TestMethod]
		public void GetFileHierarchyTest()
		{

			var folderStructure = new FolderStructure();
			var item = folderStructure.GetFilesAndFolders(@"c:\wwapps\wwclient");

			Assert.IsNotNull(item);
			Assert.IsTrue(item.Files.Count > 0);

			Console.WriteLine("+" + item.DisplayName);
			WriteChildFiles(item, 1);
		}

	    [TestMethod]
	    public void GetFileFlatFolderTest()
	    {

	        var folderStructure = new FolderStructure();
	        var item = folderStructure.GetFilesAndFolders(@"c:\wwapps\wwclient",nonRecursive: true);

	        Assert.IsNotNull(item);
	        Assert.IsTrue(item.Files.Count > 0);

	        Console.WriteLine("+" + item.DisplayName);
	        WriteChildFiles(item, 1);
	    }

        [TestMethod]
        public void MyTestMethod()
        {

            var folderStructure = new FolderStructure();
            var item = folderStructure.GetFilesAndFolders(@"c:\wwapps\wwclient", nonRecursive: false);

            Assert.IsNotNull(item);
            Assert.IsTrue(item.Files.Count > 0);

            Console.WriteLine("+" + item.DisplayName);
            WriteChildFiles(item, 1);

            var pi = folderStructure.FindPathItemByFilename(item, @"c:\wwapps\wwclient\console\wc.ico");

            Assert.IsNotNull(pi);

            Console.WriteLine(pi);
            Console.WriteLine(pi.Parent);
            Console.WriteLine(pi.Parent.Parent);



        }

        private void WriteChildFiles(PathItem item, int level)
		{
			Console.WriteLine("    " + level);
			foreach (var fileItem in item.Files)
			{
				Console.WriteLine(new string(' ', level * 2) +
												  (fileItem.IsFolder ? '+' : ' ') +
												  fileItem.DisplayName);
				if (fileItem.IsFolder)
					WriteChildFiles(fileItem, level + 1);				
			}
		}

	    [TestMethod]
	    public void GetAssociatedIcons()
	    {
	        var icons = new AssociatedIcons();
	        var icon = icons.GetIconFromFile(@"c:\temp\test.mdd");

	        Assert.IsNotNull(icon, "Icon should not be null");

	    }
    }
}
