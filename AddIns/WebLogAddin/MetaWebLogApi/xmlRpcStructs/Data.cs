using System;
using CookComputing.XmlRpc;

namespace WebLogAddin.MetaWebLogApi
{
	/// <summary> 
	/// This struct represents information about a user. 
	/// </summary> 
	[XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcData
	{
		public string name;
		public string type;
		public byte[] bits;
		public bool overwrite;
	}
}