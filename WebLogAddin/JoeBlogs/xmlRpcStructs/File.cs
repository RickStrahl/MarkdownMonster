using System;
using CookComputing.XmlRpc;

namespace JoeBlogs
{
	/// <summary> 
	/// This struct represents information about a user. 
	/// </summary> 
	[XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcFile
	{
		public string file;
		public string url;
		public string type;
	}
}