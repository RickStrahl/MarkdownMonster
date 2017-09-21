using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster;

namespace MarkdownMonster.Test
{
	[Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
	public class HtmlToPdfGenerationTests
	{
		// Use this file so we have some resource dependencies
		private string SourceMdFile = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SampleMarkdown.md");

		private string GeneratedHtmlFile;
		private readonly string OutputPdfFile;
		private string WkhtopExecutionPath;

		public HtmlToPdfGenerationTests()
		{
			OutputPdfFile = Path.Combine(Path.GetTempPath(), "test.pdf");
			GeneratedHtmlFile = GenerateHtmlDocument();
			WkhtopExecutionPath = @"c:\program files (x86)\Markdown Monster\";
		}

		public string GenerateHtmlDocument()
		{
			var doc = new MarkdownDocument();
			if (!doc.Load(SourceMdFile))
				throw new UnauthorizedAccessException("Unable to create Html from Markdown document: " + SourceMdFile);

			if (doc.RenderHtmlToFile() == null)
				throw new UnauthorizedAccessException("Unable to create Html from Markdown document: " + SourceMdFile);

			GeneratedHtmlFile = doc.HtmlRenderFilename;
			return doc.HtmlRenderFilename;
		}

		[TestMethod]
		public void GeneratePdfDefaultSettingsTest()
		{
			var gen = new HtmlToPdfGeneration()
			{
				DisplayPdfAfterGeneration = false
			};
			
			string file = GeneratedHtmlFile;
			Assert.IsTrue(File.Exists(file), "Input File doesn't exist");
			Console.WriteLine(file + " " + File.Exists(file));

			gen.ExecutionPath = WkhtopExecutionPath;
			Assert.IsTrue(File.Exists(gen.ExecutionPath + "wkhtmltopdf.exe"));

			bool result = gen.GeneratePdfFromHtml(file, OutputPdfFile);

			Console.WriteLine("Execution Command: " + gen.FullExecutionCommand);
			Console.WriteLine("Output Generated: " + gen.ExecutionOutputText);

			Assert.IsTrue(result, "Result should be true: " + gen.ErrorMessage);
		}

		[TestMethod]
		public void GeneratePdfCommonSettingsTest()
		{
			var gen = new HtmlToPdfGeneration()
			{
				DisplayPdfAfterGeneration = true,
				FooterText = "Blog Post - [page] of [topage]",
				Title = "My great document",			
				ShowFooterPageNumbers = false
			};
			
			string file = GeneratedHtmlFile;
			Assert.IsTrue(File.Exists(file), "Input File doesn't exist");
			Console.WriteLine(file + " " + File.Exists(file));

			gen.ExecutionPath = WkhtopExecutionPath;

			Assert.IsTrue(File.Exists(gen.ExecutionPath + "wkhtmltopdf.exe"));

			bool result = gen.GeneratePdfFromHtml(file, OutputPdfFile);
			
			Console.WriteLine("Execution Command: " + gen.FullExecutionCommand);
			Console.WriteLine("Output Generated: " + gen.ExecutionOutputText);
			
			Assert.IsTrue(result,"Result should be true: "  + gen.ErrorMessage);
		}

		[TestMethod]
		public void GeneratePdfWithMarginsTest()
		{
			var gen = new HtmlToPdfGeneration()
			{
				DisplayPdfAfterGeneration = true,				
				ShowFooterPageNumbers = false							
			};
			gen.Margins.MarginLeft = 50;
			gen.Margins.MarginRight = 50;



			string file = GeneratedHtmlFile;
			Assert.IsTrue(File.Exists(file), "Input File doesn't exist");
			Console.WriteLine(file + " " + File.Exists(file));

			

			Assert.IsTrue(File.Exists(gen.ExecutionPath + "wkhtmltopdf.exe"));

			bool result = gen.GeneratePdfFromHtml(file, OutputPdfFile);


			Console.WriteLine("Execution Command: " + gen.FullExecutionCommand);
			Console.WriteLine("Output Generated: " + gen.ExecutionOutputText);


			Assert.IsTrue(result, "Result should be true: " + gen.ErrorMessage);
		}
	}
}
