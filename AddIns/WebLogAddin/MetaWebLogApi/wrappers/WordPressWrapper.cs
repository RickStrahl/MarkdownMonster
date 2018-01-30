using System;
using System.Collections.Generic;
using System.Linq;
using CookComputing.XmlRpc;
using WebLogAddin.MetaWebLogApi.helpers;
using WebLogAddin.MetaWebLogApi.XmlRpcInterfaces;

namespace WebLogAddin.MetaWebLogApi
{
    /// <summary>
    /// Represents a wrapper for use with Wordpress blogs.
    /// </summary>
    public class WordPressWrapper : MetaWeblogWrapper, IWordPressWrapper
    {
        new protected IWordPressXmlRpc _wrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="WordPressWrapper"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public WordPressWrapper(string url, string username, string password)
            : this(url, username, password, 0)
        {
            _wrapper = (IWordPressXmlRpc)XmlRpcProxyGen.Create(typeof(IWordPressXmlRpc));
            _wrapper.Url = url;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WordPressWrapper"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="blogID">The blog ID.</param>
        public WordPressWrapper(string url, string username, string password, object blogId)
            : base(url, username, password, blogId)
        {
            _wrapper = (IWordPressXmlRpc)XmlRpcProxyGen.Create(typeof(IWordPressXmlRpc));
            _wrapper.Url = url;
        }

        /// <summary>
        /// Retrieve the blogs of the users.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<UserBlog> GetUsersBlogs()
        {
            var xmlRpcResult = _wrapper.GetUserBlogs(Username, Password);

            List<UserBlog> result = new List<UserBlog>();

            foreach (var x in xmlRpcResult)
                result.Add(Map.To.UserBlog(x));

            return result;
        }

        /// <summary>
        /// Get list of all tags. 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tag> GetTags()
        {
            var xmlRpcResult = _wrapper.GetTags(this.BlogID, Username, Password);

            List<Tag> result = new List<Tag>();

            foreach (var x in xmlRpcResult)
                result.Add(Map.To.TagInfo(x));

            return result;
        }

        /// <summary>
        /// Retrieve comment count for a specific post. 
        /// </summary>
        /// <param name="post_id"></param>
        /// <returns></returns>
        public CommentCount GetCommentCount(string post_id)
        {
            var result = _wrapper.GetCommentCount(this.BlogID, Username, Password, post_id);
            return Map.To.CommentCount(result);
        }

        /// <summary>
        /// Retrieve list of post statuses.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetPostStatusList()
        {
            return _wrapper.GetPostStatusList(this.BlogID, Username, Password);
        }

        /// <summary>
        /// Retrieve list of page statuses.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetPageStatusList()
        {
            return _wrapper.GetPageStatusList(this.BlogID, Username, Password);
        }

        /// <summary>
        /// Retrieve page templates.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PageTemplate> GetPageTemplates()
        {
            var result = _wrapper.GetPageTemplates(this.BlogID, Username, Password);
            foreach (var r in result)
                yield return Map.To.PageTemplate(r);
        }

        /// <summary>
        /// Retrieve blog options. If passing in an array, search for options listed within it. 
        /// </summary>
        /// <param name="blogId"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public IEnumerable<Option> GetOptions(string[] options)
        {
            var xmlRpcResult = _wrapper.GetOptions(this.BlogID, Username, Password, options);

            var result = new List<Option>();

            foreach (var x in xmlRpcResult)
                result.Add(Map.To.Option(x));

            return result;
        }

        /// <summary>
        /// Update blog options. Returns array of structs showing updated values. 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Option> SetOptions()
        {
            var xmlRpcResult = _wrapper.SetOptions(this.BlogID, Username, Password);

            var result = new List<Option>();

            foreach (var x in xmlRpcResult)
                result.Add(Map.To.Option(x));

            return result;
        }

        /// <summary>
        /// Remove comment.
        /// </summary>
        /// <param name="comment_id"></param>
        /// <returns></returns>
        public bool DeleteComment(string comment_id)
        {            
            try
            {
                var comment = GetComment(comment_id); ;
                return _wrapper.DeleteComment(this.BlogID, Username, Password, comment_id);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Edit comment.
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public bool EditComment(string comment_id, CommentStatus status, DateTime date_created_gmt, string content, string author, string author_url, string author_email)
        {
            var xmlRpcComment = new XmlRpcComment
            {

                author = author,
                author_email = author_email,
                author_url = author_url,
                dateCreated = date_created_gmt,
                content = content,
                status = EnumsHelper.GetCommentStatusName(status)
            };

            return _wrapper.EditComment(this.BlogID, Username, Password, xmlRpcComment);
        }

        /// <summary>
        /// Create new comment.
        /// </summary>
        /// <param name="postid"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public string NewComment(int postid, int? comment_parent, string content, string author, string author_url, string author_email)
        {
            var xmlRpcComment = new XmlRpcComment
            {
                parent = Convert.ToString(comment_parent),
                content = content,
                author = author,
                author_url = author_url,
                author_email = author_email
            };

            var result = _wrapper.NewComment(this.BlogID, Username, Password, Convert.ToString(postid), xmlRpcComment);
            return Convert.ToString(result);
        }

        /// <summary>
        /// Retrieve all of the comment status.
        /// </summary>
        /// <param name="post_id"></param>
        /// <returns></returns>
        public IEnumerable<string> GetCommentStatusList(string post_id)
        {
            var result = _wrapper.GetCommentStatusList(this.BlogID, Username, Password, post_id);

            return result.Keys.Cast<string>().ToArray();
        }

        /// <summary>
        /// Get the page identified by the page id. 
        /// </summary>
        /// <param name="pageid"></param>
        /// <returns></returns>
        public Page GetPage(string pageid)
        {
            var page = _wrapper.GetPage(this.BlogID, pageid, Username, Password);
            return Map.To.Page(page);
        }

        /// <summary>
        /// Get an array of all the pages on a blog. 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Page> GetPages()
        {
            var xmlRpcResult = _wrapper.GetPages(this.BlogID, Username, Password);

            var result = new List<Page>();

            foreach (var x in xmlRpcResult)
                result.Add(Map.To.Page(x));

            return result;
        }

        /// <summary>
        /// Get an array of all the pages on a blog. Just the minimum details, lighter than GetPages. 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PageMin> GetPageList()
        {
            var result = new List<PageMin>();

            var xmlRpcResult = _wrapper.GetPageList(this.BlogID, Username, Password);

            foreach (var x in xmlRpcResult)
                result.Add(Map.To.PageMin(x));

            return result;
        }

        /// <summary>
        /// Create a new page. Similar to metaWeblog.newPost.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public string NewPage(Page page, bool publish)
        {
            var content = Map.From.Page(page);
            return _wrapper.NewPage(this.BlogID, Username, Password, content, publish);
        }

        /// <summary>
        /// Removes a page from the blog. 
        /// </summary>
        /// <param name="page_id"></param>
        /// <returns></returns>
        public bool DeletePage(string pageID)
        {
            return _wrapper.DeletePage(BlogID, Username, Password, pageID);
        }

        /// <summary>
        /// Make changes to a blog page.
        /// </summary>
        /// <returns></returns>
        public bool EditPage(int pageID, Page editedPage, bool publish)
        {
            var content = Map.From.Page(editedPage);
            return _wrapper.EditPage(BlogID, pageID, Username, Password, content, publish);
        }

        public virtual bool EditPost(string postID, Post content, bool publish)
        {
            return true;
            //return _wrapper.EditPost(postID, Username, Password, Map.From.Post(content), publish);            
        }

        /// <summary>
        /// Get an array of users for the blog.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Author> GetAuthors()
        {
            var result = new List<Author>();
            var xmlRpcResult = _wrapper.GetAuthors(this.BlogID, Username, Password);

            foreach (var x in xmlRpcResult)
                result.Add(Map.To.Author(x));

            return result;
        }

        /// <summary>
        /// Get an array of available categories on a blog.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Category> GetCategories()
        {
            var result = new List<Category>();

            var xmlRpcResult = _wrapper.GetCategories(this.BlogID, Username, Password);

            foreach (var r in xmlRpcResult)
                result.Add(Map.To.Category(r));

            return result;
        }

        /// <summary>
        /// News the category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        public int NewCategory(string description, int? parentCategoryID, string name, string slug)
        {
            var x = new XmlRpcCategoryNew
                {
                    description = description,
                    name = name,
                    parent_id = parentCategoryID.Value,
                    slug = slug
                };
            return _wrapper.NewCategory(this.BlogID, Username, Password, x);
        }

        /// <summary>
        /// Delete a category.
        /// </summary>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public bool DeleteCategory(int categoryID)
        {
            return _wrapper.DeleteCategory(this.BlogID, Username, Password, Convert.ToString(categoryID));
        }

        /// <summary>
        /// Get an array of categories that start with a given string.
        /// </summary>
        /// <param name="startsWith"></param>
        /// <param name="max_results"></param>
        /// <returns></returns>
        public IEnumerable<CategoryMin> SuggestCategories(string startsWith, int max_results)
        {
            var result = new List<CategoryMin>();

            var xmlRpcResult = _wrapper.SuggestCategories(this.BlogID, Username, Password, startsWith, max_results);

            foreach (var r in xmlRpcResult)
                result.Add(Map.To.CategoryMin(r));

            return result;
        }

        /// <summary>
        /// Upload a file.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public File UploadFile(Data data)
        {
            var xmlRpcData = Map.From.Data(data);
            var result = _wrapper.UploadFile(this.BlogID, Username, Password, xmlRpcData);

            return Map.To.File(result);
        }

        /// <summary>
        /// Uploads a file to wordpress
        /// </summary>
        /// <param name="FileToUpload">Full path to file</param>
        /// <param name="WordpressName">Name that the file will get in wordpress</param>
        /// <param name="Owerwrite">If it exists owerwrite</param>
        /// <param name="MimeType">image/jpeg etc...</param>    
        public File UploadFile(string FileToUpload, string WordpressName, bool Owerwrite, string MimeType)
        {
            return(UploadFile(new Data()
            {
                Bits = FileSystemHelper.GetFileBytes(FileToUpload),
                Name = WordpressName,
                Overwrite = Owerwrite,
                Type = MimeType
            }));            
        }

        /// <summary>
        /// Gets a comment, given it's comment ID. Note that this only works for WordPress version 2.6.1 or higher.
        /// </summary>
        /// <param name="comment_id"></param>
        /// <returns></returns>
        public Comment GetComment(string comment_id)
        {
            var result = _wrapper.GetComment(this.BlogID, Username, Password, comment_id);
            return Map.To.Comment(result);
        }

        /// <summary>
        /// Gets a set of comments for a given post. Note that this only works for WordPress version 2.6.1 or higher.
        /// </summary>
        /// <param name="post_id"></param>
        /// <param name="status"></param>
        /// <param name="number"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public IEnumerable<Comment> GetComments(int post_id, string status, int number, int offset)
        {
            //var statusList = GetCommentStatusList(post_id);
            //todo: add some validation to make sure supplied status is available.

            var filter = new XmlRpcCommentFilter
            {
                post_id = post_id,
                number = number,
                offset = offset,
                status = status
            };

            var result = new List<Comment>();

            var xmlRpcResult = _wrapper.GetComments(this.BlogID, Username, Password, filter);

            foreach (var r in xmlRpcResult)
                result.Add(Map.To.Comment(r));

            return result;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public override void Dispose()
        {
            _wrapper = null;
        }

        
    }
}
