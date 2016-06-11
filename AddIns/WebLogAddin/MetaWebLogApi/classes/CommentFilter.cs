
namespace WebLogAddin.MetaWebLogApi
{
 	/// <summary>
	/// Filtering classure for getting comments.
	/// </summary>
	public class CommentFilter
	{
        public string Status { get; set; }
        public int PostID { get; set; }
        public int Number { get; set; }
        public int Offset { get; set; }
	}
}