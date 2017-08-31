using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WeblogAddin;
using WebLogAddin.MetaWebLogApi;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MarkdownMonster.Test
{
	/// <summary>
	/// Summary description for WeblogMetaData
	/// </summary>
	[TestClass]
	public class WeblogMetaDataTests
	{

		[TestMethod]
		public void GetPostYaml()
		{

			var weblogInfo = new WeblogInfo()
			{
				Name = "West Wind Web Log",
				ApiUrl = "https://weblog.west-wind.com/metaweblog.ashx",
				BlogId = "1",

			};
			var post = new Post();

			var meta = WeblogPostMetadata.GetPostYamlConfigFromMarkdown(STR_postWithMetaData, post, weblogInfo);

			Console.WriteLine(meta.Title);
			Console.WriteLine(meta.Abstract);
			Console.WriteLine(meta.CustomFields.Count);
		}

		[TestMethod]
		public void SetPostYaml()
		{
			var meta = GetMetaData();

			meta.RawMarkdownBody = "# Weblog Title\r\nHere's my **great** markdown post body";

			string markdown = meta.SetPostYaml();

			Console.WriteLine(markdown);
		}



		[TestMethod]
		public void YamlExtractionRegExTest()
		{
			string markdown = STR_postWithMetaData;

			var match = MarkdownUtilities.YamlExtractionRegex.Match(markdown);

			Assert.IsTrue(match.Success);
		}

		[TestMethod]
		public void SetPostYamlInExistingPost()
		{
			var meta = GetMetaData();
			var title = "Tip: Updated on (" + DateTime.Now + ")";

			meta.Title = title;
			meta.RawMarkdownBody = STR_postWithMetaData;

			string markdown = meta.SetPostYaml();

			Console.WriteLine(markdown);
			Assert.IsTrue(markdown.Contains("title: 'Tip: Updated on "));
		}

		[TestMethod]
		public void SetYamlFormat()
		{
			var serializer = new SerializerBuilder()
				.WithNamingConvention(new CamelCaseNamingConvention())
				.Build();

			Dictionary<string, string> val = new Dictionary<string, string>();
			val.Add("key1", "value1");



			string yaml = serializer.Serialize(new {list = val});
			Console.WriteLine(yaml);

		}

		WeblogPostMetadata GetMetaData()
		{
			return new WeblogPostMetadata()
			{
				Title = "Tip: Create a Visual Studio Menu option to Open a Command Window",
				Abstract = "This is the abstract for this blog post... ",				
				WeblogName = "West Wind Weblog",
				PostId = "9022271",
				Categories = "ASP.NET, .NET, Visual Studio",
				Keywords = "Menu,External Tools, Command Line",
				CustomFields = new Dictionary<string, CustomField>
				{
					{
						"mt_markdown",
						new CustomField()
						{
							Id = "1",
							Key = "mt_markdown",
							Value = "markdown"

						}
					},
					{
						"wp_featured_image",
						new CustomField()
						{
							Id = "1",
							Key = "wp_featured_image",
							Value = "http://blah.com/image.png"
						}
					}

				}
			};
		}

		public static string STR_postWithMetaData = @"---
title: 'Tip: Create a Visual Studio Menu option to Open a Command Window'
abstract: 'This is the abstract for this blog post... '
keywords: Menu,External Tools, Command Line
categories: ASP.NET, .NET, Visual Studio
weblogName: West Wind Weblog
postId: 9022271
customFields:
  mt_markdown:
    iD: 1
    key: mt_markdown
    value: markdown
  wp_featured_image:
    iD: 1
    key: wp_featured_image
    value: http://blah.com/image.png
---
# Weblog Title
Here's my **greatest** markdown post body";
	}
}
