using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebLogAddin.Medium;
using Westwind.Utilities;

namespace WeblogAddin.Test
{
    [TestClass]
    public class MediumTests
    {
        // bogus account
        private string MediumApiKey = null;

        public MediumTests()
        {
            // create a file with your test account api key
            MediumApiKey = File.ReadAllText(".\\ApiKey.txt");
        }

        [TestMethod]
        public void GetUser()
        {
            var medium = new MediumApiClient(MediumApiKey);
            bool result = medium.GetUser();
            Assert.IsTrue(result,medium.ErrorMessage);

            Assert.IsNotNull(medium.User.id);
            Console.WriteLine(medium.User.id);
        }


        [TestMethod]
        public void UploadCompletePost()
        {
            var medium = new MediumApiClient(MediumApiKey);
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
                content = @"<h1>New Post</h1>
<img src=""MarkdownMonster.png"" />
<p>This is a new post and text and image</p>
",
                contentFormat = "html",
                publishStatus = "draft",
                notifyFollowers = false,
                canonicalUrl = "https://weblog.west-wind.com"
            };
            post = medium.PublishCompletePost(post,publicationId: pubId, 
                                              documentBasePath: FileUtils.GetPhysicalPath(".\\"));

            Assert.IsNotNull(post.url, medium.ErrorMessage);
            Console.WriteLine(post.url);
            Console.WriteLine(post.id);
            ShellUtils.GoUrl(post.url);

        }

        [TestMethod]
        public void UploadPost()
        {
            var medium = new MediumApiClient(MediumApiKey);
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
            var medium = new MediumApiClient(MediumApiKey);
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
            var medium = new MediumApiClient(MediumApiKey);
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
            var medium = new MediumApiClient(MediumApiKey);
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
