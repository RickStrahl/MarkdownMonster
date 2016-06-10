using System;
using System.Collections.Generic;
using CookComputing.XmlRpc;
using JoeBlogs.XmlRpcInterfaces;

namespace JoeBlogs
{
    /// <summary>
    ///Implements the MetaWeblog API
    ///http://msdn.microsoft.com/en-us/library/bb259697.aspx
    /// </summary>
    public class MetaWeblogWrapper : BaseWrapper, IMetaWeblogWrapper
    {
        protected IMetaWeblogXmlRpc _wrapper;

        public MetaWeblogWrapper(string url, string username, string password)
            : this(url, username, password, "1")
        {
            _wrapper = (IMetaWeblogXmlRpc)XmlRpcProxyGen.Create(typeof(IMetaWeblogXmlRpc));
            _wrapper.Url = Url;
        }

        public MetaWeblogWrapper(string url, string username, string password, object blogId)
            : base(url, username, password, blogId)
        {
            _wrapper = (IMetaWeblogXmlRpc)XmlRpcProxyGen.Create(typeof(IMetaWeblogXmlRpc));
            _wrapper.Url = Url;
        }


        /// <summary> 
        /// Posts a new entry to a blog. 
        /// </summary> 
        /// <param name="post">The Post.</param>
        /// <param name="publish">If false, this is a draft post.</param>
        /// <returns>The postid of the newly-created post.</returns>
        public virtual int NewPost(Post post, bool publish)
        {
            var content = Map.From.Post(post);
            return Convert.ToInt32(_wrapper.NewPost(this.BlogID, Username, Password, content, publish));
        }
        

        public virtual bool EditPost(Post post, bool publish)
        {            
            var content = Map.From.Post(post);
            return Convert.ToBoolean(_wrapper.EditPost(post.PostID.ToString(), Username, Password, content, publish));            
        }

        public virtual Post GetPost(string postID)
        {
            var post = _wrapper.GetPost(postID, Username, Password);
            return Map.To.Post(post);
        }

        public virtual XmlRpcPost GetPostRaw(int postID)
        {
            return _wrapper.GetPost(postID.ToString(), Username, Password);
        }

        /// <summary> 
        /// Returns the list of categories that have been used in the blog. 
        /// </summary> 
        public virtual IEnumerable<Category> GetCategories()
        {
            var result = _wrapper.GetCategories(this.BlogID, Username, Password);

            foreach (var r in result)
                yield return Map.To.Category(r);
        }

        /// <summary>
        /// Returns the most recent draft and non-draft blog posts sorted in descending order by publish date. 
        /// </summary>
        /// <param name="numberOfPosts">The number of posts to return.</param>
        /// <returns></returns>
        public virtual IEnumerable<Post> GetRecentPosts(int numberOfPosts)
        {
            var result = _wrapper.GetRecentPosts(this.BlogID, Username, Password, numberOfPosts);

            foreach (var r in result)
                yield return Map.To.Post(r);
        }

        /// <summary>
        /// Deletes a post from the blog.
        /// </summary>
        /// <param name="postid">The ID of the post to delete.</param>
        /// <returns>Always returns true.</returns>
        public virtual bool DeletePost(int postid)
        {
            return _wrapper.DeletePost("", Convert.ToString(postid), Username, Password, false);
        }

        /// <summary>
        /// Returns basic user info (name, e-mail, userid, and so on).
        /// </summary>
        public virtual UserInfo GetUserInfo()
        {
            var result = _wrapper.GetUserInfo("", Username, Password);
            return Map.To.UserInfo(result);
        }

        /// <summary>
        /// Gets the blogs for the logged in user.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<UserBlog> GetUserBlogs()
        {
            var result = _wrapper.GetUserBlogs(Username, Password);
            foreach (var r in result)
                yield return Map.To.UserBlog(r);
        }

        /// <summary>
        /// Creates a new media object.
        /// </summary>
        /// <param name="mediaObject">The media object.</param>
        /// <returns></returns>
        public virtual MediaObjectInfo NewMediaObject(MediaObject mediaObject)
        {
            var xmlRpcMediaObject = Map.From.MediaObject(mediaObject);
            var result = _wrapper.NewMediaObject(this.BlogID, Username, Password, xmlRpcMediaObject);

            return Map.To.MediaObjectInfo(result);
        }

        public override void Dispose()
        {
            _wrapper = null;
        }
    }
}