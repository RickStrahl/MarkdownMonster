using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WeblogAddin;
using WebLogAddin.MetaWebLogApi;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Westwind.Utilities;

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

			var meta = WeblogPostMetadata.GetPostYamlConfigFromMarkdown(STR_postWithMetaData, post);

			Console.WriteLine(meta.Title);
			Console.WriteLine(meta.Abstract);
			Console.WriteLine(meta.CustomFields.Count);
		}

		[TestMethod]
		public void SetPostYaml()
		{
			var meta = GetMetaData();

			meta.RawMarkdownBody = "# Weblog Title\r\nHere's my **great** markdown post body";

			string markdown = meta.SetPostYamlFromMetaData();

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

			string markdown = meta.SetPostYamlFromMetaData();

			Console.WriteLine(markdown);
			Assert.IsTrue(markdown.Contains("title: 'Tip: Updated on "));
		}

		[TestMethod]
		public void SetYamlFormat()
		{
			var serializer = new SerializerBuilder()
				.WithNamingConvention(CamelCaseNamingConvention.Instance)
				.Build();

			Dictionary<string, string> val = new Dictionary<string, string>();
			val.Add("key1", "value1");



			string yaml = serializer.Serialize(new {list = val});
			Console.WriteLine(yaml);

		}


        [TestMethod]
        public void ManualYamlParsingTest()
        {

            var yaml = @"---
title: WPF Images from the Clipboard and Image Control Woes
weblogName: West Wind Web Log
postDate: 2020-08-26T09:48:18.6897325-10:00
categories: .NET, Clipboard
keywords: clipboard, image, crash
customFields:
  mt_github:
    key: mt_github
    value: https://somesite.com
custom-field.10: This is a field that's not part of the schema
---";
            var s = new YamlStream();
            s.Load(new StringReader(yaml));

            var root = (YamlMappingNode) s.Documents[0].RootNode;
            foreach (var entry in root.Children)
            {
                Console.WriteLine(entry.Key + ": " + entry.Value?.ToString());
            }
        }

        [TestMethod]
        public void ParseYamlIntoMetaTest()
        {
             var yaml = @"---
title: Using .NET Core Tools to Create Reusable and Shareable Tools & Apps
featuredImageUrl: https://jekyll.west-wind.com/assets/2020-08-05-Using-NET-Core-Tools-to-Create-Reusable-and-Shareable-Tools-&-Apps/banner.png
abstract: Dotnet Tools offer a simple way to create, publish and consume what are essentially .NET Core applications that can be published and shared using the existing NuGet infrastructure for packaging and distribution. It's y quick and easy to build tools that you can share either publicly or privately.  In this article I make a case for why you might use or build a Dotnet Tool and show how create, build, publish and consume Dotnet tools as well as showing some examples of useful tools I've build and published.
keywords: Dotnet Tool
categories: .NET Core, ASP.NET Core
weblogName: West Wind Web Log
postId: 1900072
permalink: https://weblog.west-wind.com/posts/2020/Aug/05/Using-NET-Core-Tools-to-Create-Reusable-and-Shareable-Tools-Apps
postDate: 2020-08-05T00:14:17.5009200-10:00
customFields:
  mt_githuburl:
    key: mt_githuburl
    value: https://github.com/RickStrahl/BlogPosts/blob/master/2020-08/DotnetTools/DotnetTools.md
---";


            var meta = WeblogPostMetadata.ParseFromYaml(yaml);

            Assert.IsNotNull(meta);

            Console.WriteLine(JsonSerializationUtils.Serialize(meta, false, true));
        }

          [TestMethod]
        public void SerializeYamlFromMetaTest()
        {
             var yaml = @"---
title: Using .NET Core Tools to Create Reusable and Shareable Tools & Apps
featuredImageUrl: https://jekyll.west-wind.com/assets/2020-08-05-Using-NET-Core-Tools-to-Create-Reusable-and-Shareable-Tools-&-Apps/banner.png
abstract: Dotnet Tools offer a simple way to create, publish and consume what are essentially .NET Core applications that can be published and shared using the existing NuGet infrastructure for packaging and distribution. It's y quick and easy to build tools that you can share either publicly or privately.  In this article I make a case for why you might use or build a Dotnet Tool and show how create, build, publish and consume Dotnet tools as well as showing some examples of useful tools I've build and published.
keywords: Dotnet Tool
categories: .NET Core, ASP.NET Core
weblogName: West Wind Web Log
postId: 1900072
permalink: https://weblog.west-wind.com/posts/2020/Aug/05/Using-NET-Core-Tools-to-Create-Reusable-and-Shareable-Tools-Apps
postDate: 2020-08-05T00:14:17.5009200-10:00
extraValue1: This value is an extra value
extraValue2: ""This is:more than I frank's money bargained for""
customFields:
  mt_githuburl:
    key: mt_githuburl
    value: https://github.com/RickStrahl/BlogPosts/blob/master/2020-08/DotnetTools/DotnetTools.md
---";

            var meta = WeblogPostMetadata.ParseFromYaml(yaml);

            Assert.IsNotNull(meta);
            Console.WriteLine(JsonSerializationUtils.Serialize(meta, false, true));


            yaml = meta.SerializeToYaml(meta, true);

            Assert.IsNotNull(yaml);
            Assert.IsTrue(yaml.Contains("title: "));

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
