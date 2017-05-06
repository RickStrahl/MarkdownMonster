using System;
using System.IO;
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
			var item = folderStructure.GetFilesAndFolders(@"c:\temp\clienttools");

			Assert.IsNotNull(item);
			Assert.IsTrue(item.Files.Count > 0);

			Console.WriteLine("+" + item.DisplayName);
			WriteChildFiles(item, 1);
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
	}
}
