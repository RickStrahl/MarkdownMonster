using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        /// The filename of the document that contains the post.
        ///
        /// This should be set to ActiveDocument.Filename, but no
        /// direct access in this code.
        /// </summary>
        public string DocumentFilename {get; }

        public DateTime PublishDate {get; }

        public LocalJekyllPublisher(WeblogPostMetadata postMetadata,
            WeblogInfo weblogInfo,
            string documentFilename,
            DateTime publishDate = default)
        {
            PublishDate = publishDate == DateTime.MinValue ? DateTime.Now : publishDate;
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
            
            var postTitle = FileUtils.SafeFilename(WeblogInfo.Name).Replace(" ","-").Replace("--","-");
            var blogFileName = $"{PublishDate:yyyy-MM-dd}-{postTitle}";
            

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

                string dateString = fileNameWithoutExtension.Substring(0, at);
                if (!DateTime.TryParse(dateString, out DateTime date))
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

                var post = new Post { DateCreated = date };
                var meta = WeblogPostMetadata.GetPostYamlConfigFromMarkdown(content, post);


                if (meta.MarkdownBody.Length > 500)
                    content = meta.MarkdownBody.Substring(0, 500);
                else
                    content = meta.MarkdownBody;
                
                post.mt_excerpt = StringUtils.TextAbstract(Markdown.ToPlainText(content),180);
                
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


        public List<Post> GetPosts()
        {
            var rootPath = WeblogInfo.ApiUrl;


            return null;
        }

        
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
                    catPath += WebUtility.UrlEncode(cat) + "/";

                catPath = catPath.TrimStart('/');
            }

            var url = string.Format(baseUrl, catPath);
            return url;
        }

        public bool WritePostFile(string postText, string blogName, string blogRoot)
        {
            var jkMeta = new JekyllMetaData
            {
                Title = PostMetadata.Title,
                PostTime = PostMetadata.PostDate,
                Published = PostMetadata.PostStatus.Equals("published", StringComparison.OrdinalIgnoreCase)
            };

            if (!string.IsNullOrEmpty(PostMetadata.Categories))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var cat in PostMetadata.Categories.Split(','))
                {
                    sb.Append(cat.Trim() + " ");
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
            catch (Exception ex)
            {
                SetError($"Unable to write post files into the Jekyll output folder: {blogRoot}");
                return false;
            }

            return true;
        }

        static Regex Image_RegEx = new Regex(@"!\[\S*\]\(\S*\)", RegexOptions.Multiline);

        /// <summary>
        // Parses each of the images in the document and posts them to the server.
        /// Updates the HTML with the returned Image Urls
        /// </summary>
        /// <param name="html">HTML that contains images</param>
        /// <param name="basePath">image file name</param>
        /// <returns>update HTML string for the document with updated images</returns>
        private string CopyImages(string markdown, string blogName, string basePath, string blogPath)
        {
            var matches = Image_RegEx.Matches(markdown);
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

        public DateTime PostTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Categories separated by spaces
        /// </summary>
        public string Categories {get; set; }

        public string Tags {get; set; }

        public bool Published {get; set; }
    }
}
