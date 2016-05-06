using CookComputing.XmlRpc;

namespace JoeBlogs.XmlRpcInterfaces
{
    /// <summary>
    /// WordPress XML RPC
    /// http://codex.wordpress.org/XML-RPC_wp
    /// </summary>
    public interface IWordPressXmlRpc : IMetaWeblogXmlRpc
    {
        /// <summary>
        /// Gets the user blogs.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        [XmlRpcMethod("wp.getUsersBlogs")]
        new XmlRpcUserBlog[] GetUserBlogs(string username, string password);

        /// <summary>
        /// Gets the tags.
        /// </summary>
        /// <param name="blogId">The blog id.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        [XmlRpcMethod("wp.getTags")]
        XmlRpcTagInfo[] GetTags(int blogId, string username, string password);

        /// <summary>
        /// Gets the comment count.
        /// </summary>
        /// <param name="blogId">The blog id.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="post_id">The post_id.</param>
        /// <returns></returns>
        [XmlRpcMethod("wp.getCommentCount")]
        XmlRpcCommentCount GetCommentCount(int blogId, string username, string password, string post_id);

        [XmlRpcMethod("wp.getPostStatusList")]
        string[] GetPostStatusList(int blogId, string username, string password);

        [XmlRpcMethod("wp.getPageStatusList")]
        string[] GetPageStatusList(int blogId, string username, string password);

        [XmlRpcMethod("wp.getPageTemplates")]
        XmlRpcPageTemplate[] GetPageTemplates(int blogId, string username, string password);

        [XmlRpcMethod("wp.getOptions")]
        XmlRpcOption[] GetOptions(int blogId, string username, string password, string[] options);

        [XmlRpcMethod("wp.setOptions")]
        XmlRpcOption[] SetOptions(int blogId, string username, string password);

        [XmlRpcMethod("wp.deleteComment")]
        bool DeleteComment(int blogId, string username, string password, string comment_id);

        [XmlRpcMethod("wp.editComment")]
        bool EditComment(int blogId, string username, string password, XmlRpcComment comment);

        [XmlRpcMethod("wp.newComment")]
        int NewComment(int blogId, string username, string password, string post_id, XmlRpcComment coment);

        [XmlRpcMethod("wp.getCommentStatusList")]
        XmlRpcStruct GetCommentStatusList(int blogId, string username, string password, string post_id);

        [XmlRpcMethod("wp.getPage")]
        XmlRpcPage GetPage(int blogId, string username, string password, string pageId);

        [XmlRpcMethod("wp.getPages")]
        XmlRpcPage[] GetPages(int blogId, string username, string password);

        [XmlRpcMethod("wp.getPageList")]
        XmlRpcPageMin[] GetPageList(int blogId, string username, string password);

        [XmlRpcMethod("wp.newPage")]
        string NewPage(int blogId, string username, string password, XmlRpcPage content, bool publish);

        [XmlRpcMethod("wp.deletePage")]
        bool DeletePage(int blogId, string username, string password, string page_id);

        [XmlRpcMethod("wp.editPage")]
        bool EditPage(int blogId, int page_id, string username, string password, XmlRpcPage content, bool publish);

        [XmlRpcMethod("wp.editPost")]
        bool EditPost(string postid, string username, string password, XmlRpcPost content, bool publish);

        [XmlRpcMethod("wp.getAuthors")]
        XmlRpcAuthor[] GetAuthors(int blogId, string username, string password);

        [XmlRpcMethod("wp.getCategories")]
        XmlRpcCategory[] GetCategories(int blogId, string username, string password);

        [XmlRpcMethod("wp.newCategory")]
        int NewCategory(int blogId, string username, string password, XmlRpcCategoryNew category);

        [XmlRpcMethod("wp.deleteCategory")]
        bool DeleteCategory(int blogId, string username, string password, string category_id);

        [XmlRpcMethod("wp.suggestCategories")]
        XmlRpcCategoryMin[] SuggestCategories(int blogId, string username, string password, string category, int max_results);

        [XmlRpcMethod("wp.uploadFile")]
        XmlRpcFile UploadFile(int blogId, string username, string password, XmlRpcData data);

        [XmlRpcMethod("wp.getComment")]
        XmlRpcComment GetComment(int blogId, string username, string password, string commentId);

        [XmlRpcMethod("wp.getComments")]
        XmlRpcComment[] GetComments(int blogId, string username, string password, XmlRpcCommentFilter filter);
    }
}
