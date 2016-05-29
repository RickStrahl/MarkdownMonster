using System;
using CookComputing.XmlRpc;

namespace JoeBlogs
{
	/// <summary>
	/// Page.
	/// </summary>
	[XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcPageTemplate
	{
		public string name;
		public string description;

		public override string ToString()
		{
			return name;
		}
	}
}
