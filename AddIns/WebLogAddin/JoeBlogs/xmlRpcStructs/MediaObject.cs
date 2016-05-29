using System;
using CookComputing.XmlRpc;

namespace JoeBlogs
{
	/// <summary>
	/// Represents a Media Object - this is usually an image, video, document etc..
	/// </summary>
	[XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcMediaObject
	{
		/// <summary>
		/// The name of the Media Object.
		/// </summary>
		public string name;

		/// <summary>
		/// The type of the Media Object.
		/// </summary>
		public string type;

		/// <summary>
		/// The byte array of the Media Object itself.
		/// 
		/// </summary>
		public byte[] bits;
	}
}
