
using CookComputing.XmlRpc;
namespace WebLogAddin.MetaWebLogApi
{
    public class Author
    {
        public string UserID { get; set; }
        public string LoginName { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}