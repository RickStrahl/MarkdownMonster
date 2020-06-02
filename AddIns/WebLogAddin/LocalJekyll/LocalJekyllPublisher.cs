using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
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
            if (!Directory.Exists(WeblogInfo.ApiUrl))
            {
                SetError("Invalid or missing Weblog root path: " + WeblogInfo.ApiUrl);
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

            // Write Root Path identifier file so root paths resolve to this folder
            // in the previewer.
            var markerFile = Path.Combine(blogPath, ".markdownmonster");
            if (!File.Exists(markerFile))
            {
                try{ File.WriteAllText(markerFile, " "); } catch {}
            }
                

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

                if (!Directory.Exists(blogPath) || !Directory.Exists(Path.Combine(blogPath,"_posts")))
                    throw new DirectoryNotFoundException();
            }
            catch
            {
                SetError($"Invalid Jekyll Blog Path: {WeblogInfo.ApiUrl}.");
                return posts;
            }

           var postFiles = Directory.GetFiles(Path.Combine(blogPath, "_posts"), "*.m*")
                .OrderByDescending(s => s)
                .Take(numberOfPosts);


            foreach (var file in postFiles)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                
                var at = StringUtils.IndexOfNth(fileNameWithoutExtension,'-',3);
                if (at < -1 || fileNameWithoutExtension.Length < at + 2) 
                    continue;

                var post = new Post();
                var meta = GetPostMetaDataFromFile(file,post);
                if ( meta == null || post == null)
                    continue;

                post.PostId = fileNameWithoutExtension;
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
            if (string.IsNullOrEmpty(WeblogInfo.ApiUrl))
                return null;

            var postFileStem= Path.Combine(WeblogInfo.ApiUrl, "_posts", postIdTag);
            var postFile = postFileStem + ".markdown";

            if (!File.Exists(postFile))
            {
                postFile = Path.ChangeExtension(postFile,"md");
                if (!File.Exists(postFile))
                    return null;
            }

            var post = new Post();
            var postMeta = GetPostMetaDataFromFile(postFile,post);
            if (postMeta == null)
                return null;

            post.Body = postMeta.RawMarkdownBody;
            post.PostId = postIdTag;

            return post;
        }


        /// <summary>
        /// Builds and launches the Jekyll site using the `LaunchCommand`
        /// </summary>
        public void BuildAndLaunchSite()
        {
            //mmFileUtils.OpenTerminal(weblogInfo.ApiUrl);
            if (WeblogInfo.LaunchCommand != null)
            {
                ShellUtils.ExecuteCommandLine(WeblogInfo.LaunchCommand);
            }
        }

        /// <summary>
        /// Creates a new post on disk and returns the filename
        /// </summary>
        /// <param name="post"></param>
        /// <param name="weblogName"></param>
        /// <returns></returns>
        public string  CreateDownloadedPostOnDisk(Post post, string weblogName = null)
		{
            if (post == null)
                return null;

			string filename = FileUtils.SafeFilename(post.Title);

            if (string.IsNullOrEmpty(weblogName))
                weblogName = WeblogInfo.Name;
            

			var mmPostFolder = Path.Combine(WeblogAddinConfiguration.Current.PostsFolder,
				"Downloaded",weblogName,
				filename);

			if (!Directory.Exists(mmPostFolder))
				Directory.CreateDirectory(mmPostFolder);


			var outputFile = Path.Combine(mmPostFolder, StringUtils.ToCamelCase(filename) + ".md");

            string body = post.Body;
			string featuredImage = null;
            string categories = null;
			if (post.Categories != null && post.Categories.Length > 0)
				categories = string.Join(",", post.Categories);



			// Create the new post by creating a file with title preset
			var meta = new WeblogPostMetadata()
			{
				Title = post.Title,
                RawMarkdownBody = body,
                Categories = categories,
				Keywords = post.mt_keywords,
				Abstract = post.mt_excerpt,
				PostId = post.PostId?.ToString(),
				WeblogName = weblogName,
				FeaturedImageUrl = featuredImage,
				PostDate = post.DateCreated,
				PostStatus = post.PostStatus,
				Permalink = post.Permalink
            };

            var postMarkdown = meta.SetPostYamlFromMetaData();

            var jekyllPostFolder = Path.Combine(WeblogInfo.ApiUrl, "assets", meta.PostId);
            postMarkdown = CopyImagesToLocal(postMarkdown,jekyllPostFolder,mmPostFolder); 

			try
			{
				File.WriteAllText(outputFile, postMarkdown);
			}
			catch (Exception ex)
            {
                this.SetError($@"Couldn't write new file at:
{outputFile}

{ex.Message}");
					
				return null;
			}
            mmApp.Configuration.LastFolder = Path.GetDirectoryName(outputFile);

            return outputFile;
        }

        #endregion


        /// <summary>
        /// Retrieves Weblog Metadata and Post Data from a Jekyll post on disk
        /// </summary>
        /// <param name="jekyllPostFilename">Full path to a Jekyll post on disk</param>
        /// <returns></returns>
        private WeblogPostMetadata GetPostMetaDataFromFile(string jekyllPostFilename, Post post)
        {
            string content = null;
            try
            {
                content = File.ReadAllText(jekyllPostFilename);
                if (string.IsNullOrEmpty(content))
                    return null;
            }
            catch
            {
                return null;
            }

            var meta = WeblogPostMetadata.GetPostYamlConfigFromMarkdown(content, post);
            if (meta == null)
                return null;

            string dateString = MarkdownUtilities.ExtractYamlValue(meta.YamlFrontMatter,"date");
            DateTime date;
            if (!DateTime.TryParse(dateString, out date))
            {
                dateString = jekyllPostFilename.Substring(0, 10);
                if (!DateTime.TryParse(dateString, out date))
                    date = DateTime.Now.Date;
            }
            post.DateCreated = date;
            meta.PostDate = date;

            content = Markdown.ToPlainText(meta.MarkdownBody);
            post.mt_excerpt = StringUtils.TextAbstract(content,180);

            return meta;
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
                    var k = cat.ToLower().Trim().Replace(" ", "-");
                    sb.Append(k + " ");
                }
                jkMeta.Categories = sb.ToString().TrimEnd();
            }

            if (!string.IsNullOrEmpty(PostMetadata.Keywords))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var key in PostMetadata.Keywords.Split(','))
                {
                    var k = key.ToLower().Trim().Replace(" ", "-");
                    sb.Append(k + " ");
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
        /// Copies images from a Jekyll post into a Markdown Monster Posts folder
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="blogName"></param>
        /// <param name="jekyllBlogPath"></param>
        /// <param name="mmPostPath"></param>
        /// <returns></returns>
        private string CopyImagesToLocal(string markdown, string jekyllBlogPath, string mmPostPath)
        {

            var matches = Image_RegEx.Matches(markdown);
            foreach(Match match in matches)
            {
                var matchedText = match.Value;
                string imageUrl = StringUtils.ExtractString(matchedText, "](", ")");

                if(imageUrl.StartsWith("http://") ||
                   imageUrl.StartsWith("https://"))
                {
                    continue;  // Absolute or site relative paths - leave as is
                }

                string imageFilename = Path.GetFileName(imageUrl);

                var newUrl = imageFilename;


                var oldFile = Path.Combine(jekyllBlogPath, imageFilename.Replace("/","\\"));
                var newFile = Path.Combine(mmPostPath,imageFilename);

                oldFile = FileUtils.NormalizePath(oldFile);
                newFile = FileUtils.NormalizePath(newFile);
                
                if (File.Exists(oldFile))
                {
                    var targetFolder = Path.GetDirectoryName(newFile);
                    File.Copy(oldFile, newFile, true);
                }

                string replaceText = matchedText.Replace("](" + imageUrl + ")", "](" + newUrl + ")");
                markdown = markdown.Replace(matchedText, replaceText);
            }

            return markdown;
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
