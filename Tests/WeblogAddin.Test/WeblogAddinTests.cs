using System;
using System.Collections.Generic;
using JoeBlogs;
using MarkdownMonster;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using WeblogAddin;

namespace WeblogAddin.Test
{
    [TestClass]
    public class WeblogAddinTests
    {
        private const string ConstWeblogName = "Rick Strahl's Weblog (local)";
        private const string ConstWordPressWeblogName = "Rick's Wordpress Weblog";

        [TestMethod]
        public void SetConfigInMarkdown()
        {
            var meta = new WeblogPostMetadata()
            {
                Abstract = "THis is an abstract",
                Keywords = "Security,SSL,IIS",
                RawMarkdownBody = MarkdownWithoutPostId,
                PostId = "2",
                WeblogName = "Rick Strahl's Web Log"
            };

            var addin = new WeblogAddin.WebLogAddin();
            string markdown = addin.SetConfigInMarkdown(meta);

            Console.WriteLine(markdown);
            Assert.IsTrue(markdown.Contains("<postid>2</postid>"), "Post Id wasn't added");
        }



        [TestMethod]
        public void GetPostConfigFromMarkdown()
        {

            string markdown = MarkdownWithoutPostId;
            
            var addin = new WeblogAddin.WebLogAddin();
            var meta = addin.GetPostConfigFromMarkdown(markdown);

            Console.WriteLine(JsonConvert.SerializeObject(meta, Formatting.Indented));

            Assert.IsTrue(meta.Abstract == "Abstract");
            Assert.IsTrue(meta.Keywords == "Keywords");
            Assert.IsTrue(meta.WeblogName == "WebLogName");
        }

        

        [TestMethod]
        public void RawPostMetaWeblogTest()
        {
            
            var rawPost = new Post()
            {
                Body = "<b>Markdown Text</b>",                 
                DateCreated = DateTime.UtcNow,
                mt_keywords = "Test,NewPost",
                CustomFields = new CustomField[]
                {
                    new CustomField()
                    {
                        ID = Guid.NewGuid().ToString(),
                        Key = "mt_Markdown",
                        Value = "**Markdown Text**"
                    }
                },
                PostID = 0,
                Title = "Testing a post"
            };

            WeblogInfo weblogInfo = WeblogAddinConfiguration.Current.Weblogs[ConstWeblogName];

            var wrapper = new MetaWeblogWrapper(weblogInfo.ApiUrl,
                weblogInfo.Username,
                weblogInfo.Password);

             rawPost.PostID = wrapper.NewPost(rawPost, true);            
        }

        [TestMethod]
        
        public void RawPostWordPressTest()
        {
            var rawPost = new Post()
            {
                Body = "<b>Markdown Text</b>",
                DateCreated = DateTime.UtcNow,
                mt_keywords = "Test,NewPost",
                CustomFields = new CustomField[]
                {
                    new CustomField()
                    {
                        ID = Guid.NewGuid().ToString(),
                        Key = "mt_Markdown",
                        Value = "**Markdown Text**"
                    }
                },
                PostID = 0,
                Title = "Testing a post"
            };

            WeblogInfo weblogInfo = WeblogAddinConfiguration.Current.Weblogs[ConstWordPressWeblogName];

            var wrapper = new WordPressWrapper(weblogInfo.ApiUrl,
                weblogInfo.Username,
                weblogInfo.Password);

            rawPost.PostID = wrapper.NewPost(rawPost, true);
        }

        [TestMethod]
        public void GetCategories()
        {
            WeblogInfo weblogInfo = WeblogAddinConfiguration.Current.Weblogs[ConstWeblogName];

            var wrapper = new MetaWeblogWrapper(weblogInfo.ApiUrl,
                weblogInfo.Username,
                weblogInfo.Password);

            var categoryStrings = new List<string>();

            var categories = wrapper.GetCategories();
            foreach (var cat in categories)
            {
                categoryStrings.Add(cat.Description);
            }

            Assert.IsTrue(categoryStrings.Count > 0);

            foreach(string cat in categoryStrings)
                Console.WriteLine(cat);

        }


        string MarkdownWithoutPostId = @"### Summary

The time to look at moving your non-secure sites is now, before it's a do or die scenario like mine was. The all SSL based Web is coming without a doubt and it's time to start getting ready for it now.


<!-- Post Configuration -->
<!---
```xml
<abstract>
Abstract
</abstract>
<categories>
Categories
</categories>
<keywords>
Keywords
</keywords>
<weblog>
WebLogName
</weblog>
```
-->
<!-- End Post Configuration -->
";
    }
}