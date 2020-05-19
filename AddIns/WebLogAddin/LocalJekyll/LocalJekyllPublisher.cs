using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using WeblogAddin;
using Westwind.Utilities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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


        public  void PublishPost(bool pushToGit = false)
        {
            var blogPath = WeblogInfo.ApiUrl;
            var postTitle = FileUtils.SafeFilename(WeblogInfo.Name).Replace(" ","-").Replace("--","-");
            var blogFileName = $"{PublishDate:yyyy-MM-dd}-{postTitle}";
            

            // normalize the path
            if (!blogPath.EndsWith("\\"))
                blogPath += "\\";

            var blogRoot = Path.GetFullPath(Path.Combine(blogPath, "..\\"));
            if (!Directory.Exists(blogRoot))
            {
                Console.WriteLine($"ERROR: Blog root directory does not exist at {blogRoot}");
                return;
            }

            var postText = PostMetadata.MarkdownBody;
            if (string.IsNullOrWhiteSpace(postText))
                return;

            // fix up image paths to point at /assets/blog-post-name/ and copy files there
            // updates the markdown text with the new image URLs
            postText = CopyImages(postText, blogFileName, Path.GetDirectoryName(DocumentFilename), blogPath);

            WritePostFile(postText, blogFileName, blogPath);
        }

        public void PushPostToGit()
        {

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

        public void WritePostFile(string postText, string blogName, string blogRoot)
        {
            var jkMeta = new JekyllMetaData
            {
                Title = PostMetadata.Title,
                PostTime = PostMetadata.PostDate
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

            var serializer = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            
            string yaml = serializer.Serialize(jkMeta);

            var folder = Path.Combine(blogRoot,"_posts");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            
            var file = Path.Combine(folder, blogName + ".markdown");

            System.IO.File.WriteAllText(file, "---\n" + yaml + "---\n"+ postText);
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
                    //if (Directory.Exists(targetFolder))
                    //    Directory.Delete(targetFolder, true);

                    if (!Directory.Exists(targetFolder))
                        Directory.CreateDirectory(targetFolder);

                    System.IO.File.Copy(oldFile, newFile, true);
                }

                string replaceText = matchedText.Replace("](" + imageUrl + ")", "](" + newUrl + ")");
                markdown = markdown.Replace(matchedText, replaceText);
            }

            return markdown;
        }

        //public void CopyImages(string blogName, string sourcePath, string blogPath)
        //{
        //    blogPath = Path.Combine(blogPath, "assets") + blogName.Replace(".md", "") + @"\";


            
        //    if (!Directory.Exists(blogPath))
        //        Directory.CreateDirectory(blogPath);

        //    var images = new List<string>();
        //    images.AddRange(Directory.GetFiles(sourcePath, "*.png"));
        //    images.AddRange(Directory.GetFiles(sourcePath, "*.jpg"));
        //    images.AddRange(Directory.GetFiles(sourcePath, "*.gif"));
        //    foreach (var item in images)
        //    {
        //        var fileName = item;
        //        if (fileName.Contains("\\"))
        //        {
        //            var parts = fileName.Split('\\');
        //            fileName = parts[parts.Length];
        //        }

        //        System.IO.File.Copy(item, blogPath + fileName, true);
        //    }
        //}

    }

    public class JekyllMetaData
    {
        public string Layout { get; set; } = "post";
        public string Title { get; set; }

        public DateTime PostTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Categories separated by spaces
        /// </summary>
        public string Categories {get; set; }  
    }
}
