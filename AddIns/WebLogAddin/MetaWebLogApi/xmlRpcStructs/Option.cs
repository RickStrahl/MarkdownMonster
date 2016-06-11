using System;
using CookComputing.XmlRpc;

namespace WebLogAddin.MetaWebLogApi
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
