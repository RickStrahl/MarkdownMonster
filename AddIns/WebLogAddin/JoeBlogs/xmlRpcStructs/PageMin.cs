using System;
using CookComputing.XmlRpc;

namespace JoeBlogs
{
	/// <summary>
	/// Page.
	/// </summary>
	[XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcPageMin
	{
		public DateTime dateCreated;
		public string page_id;
		public string page_title;
		public object page_parent_id;
	}
}