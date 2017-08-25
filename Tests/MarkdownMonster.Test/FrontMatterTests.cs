using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
	/// <summary>
	/// Tests the ability to parse out front matter in standard
	/// fenced and pandoc fenced formats.
	/// </summary>
	/// <remarks>If MSTest Framework v2 is used, these can become DataRows
	/// in a DataTestMethod, with each</remarks>
	[TestClass]
	public class FrontMatterTests
	{
		/// <summary>
		/// Base markdown to use for permutation testing.
		/// </summary>
		const string PermutationMarkdown = @"{lead}
title: This is the title
author: this is the author
{trail}
# Heading 1
---
This is some text
";
		/// <summary>
		/// Tests the ability to parse front matter that
		/// begins with <c>---</c> and ends with <c>---</c>
		/// </summary>
		[TestMethod]
		public void YamlDashOpenDashCloseTest()
		{
			var lead = "---";
			var trail = "---";
			YamlFrontMatterParseHasHeaderTest(lead, trail);
			YamlFrontMatterParseHasHorizontalRuleTest(lead, trail);
			YamlFrontMatterParseHasContentTest(lead, trail);
			YamlFrontMatterParseHasNoTitleTest(lead, trail);
			YamlFrontMatterParseHasNoAuthorTest(lead, trail);
		}

		/// <summary>
		/// Tests the ability to parse front matter that
		/// begins with <c>---</c> and ends with <c>...</c>
		/// </summary>
		[TestMethod]
		public void YamlDashOpenDotCloseTest()
		{
			var lead = "---";
			var trail = "...";
			YamlFrontMatterParseHasHeaderTest(lead, trail);
			YamlFrontMatterParseHasHorizontalRuleTest(lead, trail);
			YamlFrontMatterParseHasContentTest(lead, trail);
			YamlFrontMatterParseHasNoTitleTest(lead, trail);
			YamlFrontMatterParseHasNoAuthorTest(lead, trail);
		}

		/// <summary>
		/// Tests the ability to parse front matter that
		/// begins with <c>...</c> and ends with <c>---</c>
		/// </summary>
		[TestMethod]
		public void YamlDotOpenDashCloseTest()
		{
			var lead = "...";
			var trail = "---";
			YamlFrontMatterParseHasHeaderTest(lead, trail);
			YamlFrontMatterParseHasHorizontalRuleTest(lead, trail);
			YamlFrontMatterParseHasContentTest(lead, trail);
			YamlFrontMatterParseHasNoTitleTest(lead, trail);
			YamlFrontMatterParseHasNoAuthorTest(lead, trail);
		}

		/// <summary>
		/// Tests the ability to parse front matter that
		/// begins with <c>...</c> and ends with <c>...</c>
		/// </summary>
		[TestMethod]
		public void YamlDotOpenDotCloseTest()
		{
			var lead = "...";
			var trail = "...";
			YamlFrontMatterParseHasHeaderTest(lead, trail);
			YamlFrontMatterParseHasHorizontalRuleTest(lead, trail);
			YamlFrontMatterParseHasContentTest(lead, trail);
			YamlFrontMatterParseHasNoTitleTest(lead, trail);
			YamlFrontMatterParseHasNoAuthorTest(lead, trail);
		}

		/// <summary>
		/// Tests that the header (h1) following the YAML front matter
		/// appears in the generated HTML
		/// </summary>
		/// <param name="lead">The leading characters.</param>
		/// <param name="trail">The closing characters.</param>
		/// [DataTestMethod]
		/// [DataRow("---", "---")]
		/// [DataRow("---", "...")]
		/// [DataRow("...", "---")]
		/// [DataRow("...", "...")]
		public void YamlFrontMatterParseHasHeaderTest(string lead, string trail)
		{
			string html = CreateMarkdownPermutation(lead, trail);
			Assert.IsTrue(html.Contains("Heading 1"));
		}

		/// <summary>
		/// Tests that the horizontal rule following the YAML front matter
		/// appears in the generated HTML. This ensures that the separator
		/// is not read as part of the front matter.
		/// </summary>
		/// <param name="lead">The leading characters.</param>
		/// <param name="trail">The closing characters.</param>
		/// [DataTestMethod]
		/// [DataRow("---", "---")]
		/// [DataRow("---", "...")]
		/// [DataRow("...", "---")]
		/// [DataRow("...", "...")]
		public void YamlFrontMatterParseHasHorizontalRuleTest(string lead, string trail)
		{
			string html = CreateMarkdownPermutation(lead, trail);
			Assert.IsTrue(html.Contains("<hr"));
		}

		/// <summary>
		/// Tests that the body content following the YAML front matter
		/// appears in the generated HTML
		/// </summary>
		/// <param name="lead">The leading characters.</param>
		/// <param name="trail">The closing characters.</param>
		/// [DataTestMethod]
		/// [DataRow("---", "---")]
		/// [DataRow("---", "...")]
		/// [DataRow("...", "---")]
		/// [DataRow("...", "...")]
		public void YamlFrontMatterParseHasContentTest(string lead, string trail)
		{
			string html = CreateMarkdownPermutation(lead, trail);
			Assert.IsTrue(html.Contains("This is some text"));
		}

		/// <summary>
		/// Tests that the YAML front matter 'title' does not
		/// appear in the generated HTML
		/// </summary>
		/// <param name="lead">The leading characters.</param>
		/// <param name="trail">The closing characters.</param>
		/// [DataTestMethod]
		/// [DataRow("---", "---")]
		/// [DataRow("---", "...")]
		/// [DataRow("...", "---")]
		/// [DataRow("...", "...")]
		public void YamlFrontMatterParseHasNoTitleTest(string lead, string trail)
		{
			string html = CreateMarkdownPermutation(lead, trail);
			Assert.IsFalse(html.Contains("title"));
		}

		/// <summary>
		/// Tests that the YAML front matter 'author' does not
		/// appear in the generated HTML
		/// </summary>
		/// <param name="lead">The leading characters.</param>
		/// <param name="trail">The closing characters.</param>
		/// [DataTestMethod]
		/// [DataRow("---", "---")]
		/// [DataRow("---", "...")]
		/// [DataRow("...", "---")]
		/// [DataRow("...", "...")]
		public void YamlFrontMatterParseHasNoAuthorTest(string lead, string trail)
		{
			string html = CreateMarkdownPermutation(lead, trail);
			Assert.IsFalse(html.Contains("author"));
		}

		/// <summary>
		/// Arranges the permuted markdown and acts to parse it into HTML.
		/// </summary>
		/// <param name="lead">The leading characters.</param>
		/// <param name="trail">The closing characters.</param>
		/// <returns>The generated HTML for the permutation</returns>
		private static string CreateMarkdownPermutation(string lead, string trail)
		{
			var markdown = PermutationMarkdown.Replace("{lead}", lead)
											  .Replace("{trail}", trail);
			var parser = MarkdownParserFactory.GetParser(usePragmaLines: false, forceLoad: true);
			string html = parser.Parse(markdown);
			return html;
		}
	}
}