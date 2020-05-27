using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Parsers;
using MarkdownMonster;
using WeblogAddin;
using WebLogAddin.MetaWebLogApi;
using Westwind.Utilities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using File = System.IO.File;

// WIP: Add Local File Jekyll Publisher
// From Rocky's code

namespace WebLogAddin.LocalJekyll
{
    public class LocalJekyllPublisher
    {
        /// <summary>
        /// Markdown Monster Weblog Post Meta Data
        /// </summary>
        public WeblogPostMetadata PostMetadata { get; }
        
        /// <summary>
        /// Markdown Monster Weblog information for a specific
        /// blog. Holds name, type of blog, post URL or path.
        /// For Jekyll the ApiUrl holds the Jekyll project root.
        /// </summary>
        public WeblogInfo WeblogInfo { get; }

        /// <summary>
        /// The filename of the document that contains the Markdown Monster post.
        ///
        /// Used as a path reference so that related resources can be found. The
        /// actual post content is retrieved from the postMetaData.
        /// </summary>
        public string DocumentFilename {get; }


        public LocalJekyllPublisher(WeblogPostMetadata postMetadata,
            WeblogInfo weblogInfo,
            string documentFilename)
        {
            PostMetadata = postMetadata;
            WeblogInfo = weblogInfo;
            DocumentFilename = documentFilename;
        }

        #region Top Level API
        /// <summary>
        /// Publishes post
        /// </summary>
        /// <param name="pushToGit"></param>
        /// <returns></returns>
        public bool PublishPost(bool pushToGit = false)
        {
            var blogPath = WeblogInfo.ApiUrl;

            string blogRoot = null;
            try
            {
                blogRoot = Path.GetFullPath(Path.Combine(blogPath, "..\\"));
            }
            catch {}


            if (string.IsNullOrEmpty(blogRoot) || !Directory.Exists(blogRoot))
            {
                SetError($"Blog root directory does not exist: {blogRoot}");
                return false;
            }
            
            var postTitle = FileUtils.SafeFilename(PostMetadata.Title).Replace(" ","-").Replace("--","-");
            var blogFileName = $"{PostMetadata.PostDate:yyyy-MM-dd}-{postTitle}";
            

            // normalize the path
            if (!blogPath.EndsWith("\\"))
                blogPath += "\\";
            
            var postText = PostMetadata.MarkdownBody;
            if (string.IsNullOrWhiteSpace(postText))
                return false;

            // fix up image paths to point at /assets/blog-post-name/ and copy files there
            // updates the markdown text with the new image URLs
            postText = CopyImages(postText, blogFileName, Path.GetDirectoryName(DocumentFilename), blogPath);

            WritePostFile(postText, blogFileName, blogPath);


            PostMetadata.PostId = blogFileName;

            return true;
        }


        /// <summary>
        /// Returns a list of posts
        /// </summary>
        /// <param name="numberOfPosts">The number of most recent posts to retrieve</param>
        /// <returns></returns>
        public IEnumerable<Post> GetRecentPosts(int numberOfPosts = 20)
        {
            var posts = new List<Post>();

            string blogPath = null;
            try
            {
                blogPath = Path.GetFullPath(WeblogInfo.ApiUrl);
            }
            catch
            {
                SetError($"Invalid Jekyll Blog Path: {WeblogInfo.ApiUrl}.");
                return posts;
            }

            var blogRoot = Path.GetFullPath(Path.Combine(blogPath, "..\\"));

            var postFiles = Directory.GetFiles(Path.Combine(blogPath, "_posts"), "*.m*")
                .OrderByDescending(s => s)
                .Take(numberOfPosts);


            foreach (var file in postFiles)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                
                var at = StringUtils.IndexOfNth(fileNameWithoutExtension,'-',3);
                if (at < -1 || fileNameWithoutExtension.Length < at + 2) 
                    continue;


                //title = title.Substring(at + 1);

                string content = null;
                try
                {
                    content = File.ReadAllText(file);
                    if (string.IsNullOrEmpty(content))
                        continue;
                }
                catch
                {
                    continue;
                }

                var post = new Post();
                var meta = WeblogPostMetadata.GetPostYamlConfigFromMarkdown(content, post);

                string dateString = MarkdownUtilities.ExtractYamlValue(meta.YamlFrontMatter,"date");
                DateTime date;
                if (!DateTime.TryParse(dateString, out date))
                {
                    dateString = file.Substring(0, 10);
                    if (!DateTime.TryParse(dateString, out date))
                        date = DateTime.Now.Date;
                }

                // use the parsed value
                post.DateCreated = date;


                if (meta.MarkdownBody.Length > 500)
                    content = meta.MarkdownBody.Substring(0, 500);
                else
                    content = meta.MarkdownBody;

                content = Markdown.ToPlainText(content);
                post.mt_excerpt = StringUtils.TextAbstract(content,180);
                
                var title = StringUtils.ExtractString(content, "title:", "\n")?.Trim(new char[] {' ', '\'', '\"', '\n' , '\r'});

                
                posts.Add(post);
            }
            

            return posts;
        }

        /// <summary>
        /// Returns a post and its meta data
        /// </summary>
        /// <param name="postIdTag"></param>
        /// <returns></returns>
        public Post GetPost(string postIdTag)
        {
            return null;
        }

        #endregion


        //public string FixImagePaths(string postText, string blogName, string blogRoot)
        //{
        //    var pattern = @"\!\[(?:.*)\]\((?:.*)\)";
        //    blogName = blogName.Replace(".md", "");
        //    var newPath = $"{{{{ site.url }}}}/assets/{blogName}/";

        //    var match = Regex.Match(postText, pattern, RegexOptions.Multiline);
        //    while (match.Success)
        //    {
        //        if (!match.ToString().Contains("://"))
        //        {
        //            var original = match.ToString();
        //            var newText = original.Replace("](", $"]({newPath}");
        //            postText = postText.Replace(original, newText);
        //        }

        //        match = match.NextMatch();
        //    }

        //    if (!postText.Contains("featured-image: https"))
        //        postText = postText.Replace("featured-image: ",
        //            $"featured-image: https://blog.lhotka.net/assets/{blogName}/");
        //    return postText;
        //}

        public bool WritePostFile(string postText, string blogName, string blogRoot)
        {
            var jkMeta = new JekyllMetaData
            {
                Title = PostMetadata.Title,
                Date = PostMetadata.PostDate.Date,
                Published = PostMetadata.PostStatus.Equals("published", StringComparison.OrdinalIgnoreCase),
                Permalink = PostMetadata.Permalink,
                FeaturedImageUrl = PostMetadata.FeaturedImageUrl
            };

            if (!string.IsNullOrEmpty(PostMetadata.Categories))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var cat in PostMetadata.Categories.Split(','))
                {
                    sb.Append(cat.ToLower().Trim() + " ");
                }
                jkMeta.Categories = sb.ToString().TrimEnd();
            }

            if (!string.IsNullOrEmpty(PostMetadata.Keywords))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var key in PostMetadata.Keywords.Split(','))
                {
                    sb.Append(key.Trim() + " ");
                }
                jkMeta.Tags = sb.ToString().TrimEnd();
            }

            


            var serializer = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            
            string yaml = serializer.Serialize(jkMeta);

            var folder = Path.Combine(blogRoot,"_posts");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            try
            {
                var file = Path.Combine(folder, blogName + ".markdown");

                System.IO.File.WriteAllText(file, "---\n" + yaml + "---\n" + postText);
            }
            catch 
            {
                SetError($"Unable to write post files into the Jekyll output folder: {blogRoot}");
                return false;
            }

            return true;
        }

        static Regex Image_RegEx = new Regex(@"!\[\S*\]\(\S*\)", RegexOptions.Multiline);

        /// Updates the HTML with the returned Image Urls
        /// </summary>
        /// <param name="html">HTML that contains images</param>
        /// <param name="basePath">image file name</param>
        /// <returns>update HTML string for the document with updated images</returns>
        private string CopyImages(string markdown, string blogName, string basePath, string blogPath)
        {
            var matches = Image_RegEx.Matches(markdown);

            bool firstImage = true;
            foreach(Match match in matches)
            {
                var matchedText = match.Value;
                string imageUrl = StringUtils.ExtractString(matchedText, "](", ")");

                if(imageUrl.StartsWith("http://") ||
                   imageUrl.StartsWith("https://") ||
                   imageUrl.StartsWith("/") )
                {
                    continue;  // Absolute or site relative paths - leave as is
                }

                string imageFilename = Path.GetFileName(imageUrl);

                var newUrl = $"/assets/{blogName}/{imageFilename}";

                if (firstImage)
                {
                    firstImage = false;
                    if(string.IsNullOrEmpty(PostMetadata.FeaturedImageUrl))
                        PostMetadata.FeaturedImageUrl = WeblogInfo.WebLogRootSitePath?.TrimEnd('/') + newUrl;
                }

                var oldFile = Path.Combine(basePath, imageUrl);
                var newFile = Path.Combine(blogPath, "assets", blogName, imageUrl);

                oldFile = FileUtils.NormalizePath(oldFile);
                newFile = FileUtils.NormalizePath(newFile);

                
                if (File.Exists(oldFile))
                {
                    var targetFolder = Path.GetDirectoryName(newFile);
                    if (!Directory.Exists(targetFolder))
                        Directory.CreateDirectory(targetFolder);
                    else
                        FileUtils.DeleteFiles(targetFolder, "*.*", false);

                    System.IO.File.Copy(oldFile, newFile, true);
                }

                string replaceText = matchedText.Replace("](" + imageUrl + ")", "](" + newUrl + ")");
                markdown = markdown.Replace(matchedText, replaceText);
            }

            return markdown;
        }

        /// <summary>
        /// Generates a POST URL using a template ("http://localhost/{0}") where the
        /// templated text is filled with /cat/cat/yyyy/mm/dd/title-cased/
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public string GetPostUrl(string baseUrl = "http://localhost:4000/{0}")
        {
            var catPath = "";

            if (PostMetadata.Categories != null)
            {
                var cats = PostMetadata.Categories.Split(new char[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);

                foreach (string cat in cats)
                    catPath += WebUtility.UrlEncode(cat.ToLower()).Replace(" ","-") + "/";

                catPath = catPath.TrimStart('/');
            }

            
            var postTitle = FileUtils.SafeFilename(PostMetadata.Title).Replace(" ","-").Replace("--","-");
            catPath += $"{PostMetadata.PostDate:yyyy/MM/dd}/{postTitle}";


            var url = string.Format(baseUrl, catPath);
            return url;
        }

        /// <summary>
        // Parses each of the images in the document and posts them to the server.

        #region

        public string ErrorMessage { get; set; }

        protected void SetError()
        {
            SetError("CLEAR");
        }

        protected void SetError(string message)
        {
            if (message == null || message == "CLEAR")
            {
                ErrorMessage = string.Empty;
                return;
            }
            ErrorMessage += message;
        }

        protected void SetError(Exception ex, bool checkInner = false)
        {
            if (ex == null)
                ErrorMessage = string.Empty;

            Exception e = ex;
            if (checkInner)
                e = e.GetBaseException();

            ErrorMessage = e.Message;
        }
        #endregion  

    }

    //  
    public class JekyllMetaData
    {
        public string Layout { get; set; } = "post";
        public string Title { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        /// <summary>
        /// Categories separated by spaces
        /// </summary>
        public string Categories {get; set; }

        public string Tags {get; set; }

        public bool Published {get; set; }

        public string Permalink { get; set; }

        public string FeaturedImageUrl {get; set; }
    }
}
