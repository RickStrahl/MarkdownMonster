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
            // create 
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

        
    }
}
