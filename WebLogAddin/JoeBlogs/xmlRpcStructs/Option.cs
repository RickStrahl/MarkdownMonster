using System;
using CookComputing.XmlRpc;

namespace JoeBlogs
{
	/// <summary>
	/// Page.
	/// </summary>
	[XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcOption
	{
		public string option;
		public string value;
	}
}
