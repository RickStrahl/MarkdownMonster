
namespace WebLogAddin.MetaWebLogApi
{
	/// <summary> 
	/// This class represents information about a user. 
	/// </summary> 
	public class UserInfo
	{
        public string URL { get; set; }
        public int BlogID { get; set; }
        public string BlogName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
	}
}