using System;
using System.Collections.Generic;
using System.Linq;
using WebLogAddin.MetaWebLogApi;
using MarkdownMonster;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using WeblogAddin;

namespace WeblogAddin.Test
{
    [TestClass]
    public class WeblogAddinTests
    {
        private const string ConstWeblogName = "rk7wof4b";
        private const string ConstWordPressWeblogName = "Rick Strahl WordPress";


        public WeblogAddinTests()
        {
            
        }

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
            WeblogInfo weblogInfo = WeblogAddinConfiguration.Current.Weblogs[ConstWeblogName];

            string markdown = MarkdownWithoutPostId;
            
            var addin = new WeblogAddin.WebLogAddin();
            var meta = addin.GetPostConfigFromMarkdown(markdown,weblogInfo);

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

        [TestMethod]
        public void GetRecentPosts()
        {
            WeblogInfo weblogInfo = WeblogAddinConfiguration.Current.Weblogs[ConstWeblogName];

            var wrapper = new MetaWeblogWrapper(weblogInfo.ApiUrl,
                weblogInfo.Username,
                weblogInfo.Password);

            var posts = wrapper.GetRecentPosts(2).ToList();

            Assert.IsTrue(posts.Count == 2);

            foreach (var post in posts)
                Console.WriteLine(post.Title + "  " + post.DateCreated);
        }

        [TestMethod]
        public void GetRecentPost()
        {
            WeblogInfo weblogInfo = WeblogAddinConfiguration.Current.Weblogs[ConstWeblogName];

            var wrapper = new MetaWeblogWrapper(weblogInfo.ApiUrl,
                weblogInfo.Username,
                weblogInfo.Password);

            var posts = wrapper.GetRecentPosts(2).ToList();

            Assert.IsTrue(posts.Count == 2);

            var postId = posts[0].PostID;

            var post = wrapper.GetPost(postId.ToString());

            Assert.IsNotNull(post);
            Console.WriteLine(post.Title);

            // markdown
            Console.WriteLine(post.CustomFields?[0].Value);
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