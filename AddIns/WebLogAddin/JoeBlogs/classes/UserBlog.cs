
namespace JoeBlogs
{
    /// <summary> 
    /// This class represents information about a user's blog. 
    /// </summary> 
    public class UserBlog
    {
        private int _userBlogID;

        public int BlogID { get { return _userBlogID; } }

        public bool IsAdmin { get; set; }
        public string URL { get; set; }
        public string BlogName { get; set; }
        public string XMLRpc { get; set; }
    }
}