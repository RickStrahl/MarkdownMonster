using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebLogAddin.Medium;
using WebLogAddin.MetaWebLogApi;
using Westwind.Utilities;
using File = System.IO.File;

namespace WeblogAddin.Test
{
    [TestClass]
    public class MediumTests
    {
        // bogus account
        private string MediumApiKey = null;

        private WeblogInfo WeblogInfo = null;

        public MediumTests()
        {
            // create a file with your test account api key
            MediumApiKey = File.ReadAllText(".\\ApiKey.txt");

            WeblogInfo = new WeblogInfo
            {
                AccessToken = MediumApiKey,
                Name = "Markdown Monster Test Blog"                 
            };
        }

        [TestMethod]
        public void GetUser()
        {
            var medium = new MediumApiClient(WeblogInfo);
            bool result = medium.GetUser();
            Assert.IsTrue(result,medium.ErrorMessage);

            Assert.IsNotNull(medium.User.id);
            Console.WriteLine(medium.User.id);
        }


        [TestMethod]
        public void UploadCompletePost()
        {
            var medium = new MediumApiClient(WeblogInfo);
            bool result = medium.GetUser();
            Assert.IsTrue(result, medium.ErrorMessage);

            // make sure there is at least one publication available
            var pubs = medium.GetPublications();
            Assert.IsNotNull(pubs, medium.ErrorMessage);
            string pubId = pubs[0].id;
            WeblogInfo.BlogId = pubId;

            var post = new Post
            {
                Tags = new string[] {"Markdown", "Test"},
                Title = "Test Post #" + DataUtils.GenerateUniqueId(),
                Body = @"<h1>New Post</h1>
<img src=""MarkdownMonster.png"" />
<p>This is a new post and text and image</p>
"
            };

            
            var mediumPost = medium.PublishCompletePost(post,documentBasePath: FileUtils.GetPhysicalPath(".\\"));

            Assert.IsNotNull(mediumPost.url, medium.ErrorMessage);
            Console.WriteLine(mediumPost.url);
            Console.WriteLine(mediumPost.id);
            ShellUtils.GoUrl(mediumPost.url);

        }

        [TestMethod]
        public void UploadPost()
        {
            var medium = new MediumApiClient(WeblogInfo);
            bool result = medium.GetUser();
            Assert.IsTrue(result, medium.ErrorMessage);

            var post = new MediumPost()
            {
                tags = new string[] {"Markdown","Test"},
                title = "Test Post #" + DataUtils.GenerateUniqueId(),
                content = "This is a **test post**.",
                contentFormat = "markdown",
                publishStatus = "draft",
                notifyFollowers = false,
                canonicalUrl= "https://weblog.west-wind.com"
            };
            post = medium.PublishPost(post);

            Assert.IsNotNull(post.url, medium.ErrorMessage);
            Console.WriteLine(post.url);
            Console.WriteLine(post.id);
            ShellUtils.GoUrl(post.url);

        }

        [TestMethod]
        public void UploadPostToPublication()
        {
            var medium = new MediumApiClient(WeblogInfo);
            bool result = medium.GetUser();
            Assert.IsTrue(result, medium.ErrorMessage);

            // make sure there is at least one publication available
            var pubs = medium.GetPublications();
            Assert.IsNotNull(pubs, medium.ErrorMessage);

            string pubId = pubs[0].id;


            var post = new MediumPost()
            {
                tags = new string[] { "Markdown", "Test" },
                title = "Test Post #" + DataUtils.GenerateUniqueId(),
                content = "This is a **test post**.",
                contentFormat = "markdown",
                publishStatus = "draft",
                notifyFollowers = false,
                canonicalUrl = "https://weblog.west-wind.com"
            };
            post = medium.PublishPost(post,pubId);

            Assert.IsNotNull(post.url, medium.ErrorMessage);
            Console.WriteLine(post.url);
            Console.WriteLine(post.id);
            ShellUtils.GoUrl(post.url);

        }

        [TestMethod]
        public void UploadImage()
        {
            var medium = new MediumApiClient(WeblogInfo);
            bool result = medium.GetUser();
            Assert.IsTrue(result, medium.ErrorMessage);

            string url = medium.PublishImage(".\\MarkdownMonster.png");

            Assert.IsNotNull(url,medium.ErrorMessage);
            Console.WriteLine(url);
            ShellUtils.GoUrl(url);
        }


        [TestMethod]
        public void GetPublications()
        {
            var medium = new MediumApiClient(WeblogInfo);
            bool result = medium.GetUser();
            Assert.IsTrue(result, medium.ErrorMessage);

            var pubs = medium.GetPublications();

            Assert.IsNotNull(pubs,medium.ErrorMessage);
            foreach (var pub in pubs)
            {
                Console.WriteLine(pub.id + " - " + pub.name + " - " + pub.description);
            }

        }
        
    }
}
