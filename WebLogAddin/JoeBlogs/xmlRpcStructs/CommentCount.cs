using System;
using CookComputing.XmlRpc;

namespace JoeBlogs
{

	/// <summary>
	/// Shows total number of comments, as well as a break down of comments approved, awaiting moderation, marked as spam.
	/// </summary>
	[XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcCommentCount
	{
		public object approved; //i think this maybe an error on WP part - should be int according to their documentation: http://codex.wordpress.org/XML-RPC#wp.getCommentCount
		public object awaiting_moderation;
		public object spam;
		public object total_comments;
	}
}
