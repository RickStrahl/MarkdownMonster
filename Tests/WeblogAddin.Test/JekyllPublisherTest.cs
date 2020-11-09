using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebLogAddin.LocalJekyll;
using WebLogAddin.MetaWebLogApi;
using YamlDotNet.RepresentationModel;

namespace WeblogAddin.Test
{
    [TestClass]
    public class JekyllPublisherTests
    {
        //private const string STR_MM_POST_FILE_NAME = "c:\\users\\rstrahl\\DropBox\\Markdown Monster Weblog Posts\\2020-05\\Jekyll-Test-Blog-Post\\JekyllTestBlogPost.md";
        private const string STR_MM_POST_FILE_NAME = @"C:\Users\rstrahl\DropBox\Markdown Monster Weblog Posts\2020-11\Model-Driven-Mixed-Reality\ModelDrivenMixedReality.md";
        
        private const string STR_JEKYLL_PROJECT_FOLDER = "C:\\projects\\Test\\jekyll\\help";

        private const string STR_JEKYLL_POST_ID = "2020-11-09-Model-Driven-Reality";

        [TestMethod]
        public void PublishTest()
        {
            var webLogInfo = new WeblogInfo
            {
                ApiUrl = STR_JEKYLL_PROJECT_FOLDER,
                Name = "Jekyll Test Blog"
            };

            var rawMd = System.IO.File.ReadAllText(STR_MM_POST_FILE_NAME);

            var post = new Post();  // filled from meta data but not used here
            var meta = WeblogPostMetadata.GetPostYamlConfigFromMarkdown(rawMd);
            

            var pub = new LocalJekyllPublisher(meta, webLogInfo,STR_MM_POST_FILE_NAME);
            pub.PublishPost(false);
        }

        [TestMethod]
        public void GetPosts()
        {
            var webLogInfo = new WeblogInfo
            {
                ApiUrl = STR_JEKYLL_PROJECT_FOLDER,
                Name = "Jekyll Test Blog"
            };

            var rawMd = System.IO.File.ReadAllText(STR_MM_POST_FILE_NAME);

            var post = new Post();  // filled from meta data but not used here
            var meta = WeblogPostMetadata.GetPostYamlConfigFromMarkdown(rawMd, post);
            

            var pub = new LocalJekyllPublisher(meta, webLogInfo,null);
            var posts = pub.GetRecentPosts(20).ToList();

            Console.WriteLine(posts.Count);

            foreach (var pst in posts)
            {
                Console.WriteLine($"{pst.Title} -  {pst.mt_excerpt}") ;
            }
        }

        [TestMethod]
        public void GetPostTest()
        {
            var weblogInfo = new WeblogInfo
            {
                ApiUrl = STR_JEKYLL_PROJECT_FOLDER,
                Name = "Jekyll Test Blog"
            };


            var pub = new LocalJekyllPublisher(null, weblogInfo,null);
            
            var post = pub.GetPost(STR_JEKYLL_POST_ID);
            Assert.IsNotNull(post);
            Assert.IsNotNull(post.Body);
        }

        [TestMethod]
        public void CreateDownloadPostTest()
        {
            var weblogInfo = new WeblogInfo
            {
                ApiUrl = STR_JEKYLL_PROJECT_FOLDER,
                Name = "Jekyll Test Blog"
            };


            var pub = new LocalJekyllPublisher(null, weblogInfo,null);
            
            var post = pub.GetPost(STR_JEKYLL_POST_ID);
            Assert.IsNotNull(post);
            Assert.IsNotNull(post.Body);

            pub.CreateDownloadedPostOnDisk(post,weblogInfo.Name);

        }

       
    }
}
