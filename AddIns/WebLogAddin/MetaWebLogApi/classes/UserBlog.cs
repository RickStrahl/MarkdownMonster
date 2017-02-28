
namespace WebLogAddin.MetaWebLogApi
{
    /// <summary> 
    /// This class represents information about a user's blog. 
    /// </summary> 
    public class UserBlog
    {
        
        public object BlogId { get; set; }

        public bool IsAdmin { get; set; }
        public string URL { get; set; }
        public string BlogName { get; set; }
        public string XMLRpc { get; set; }
    }
}