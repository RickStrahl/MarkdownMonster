namespace WebLogAddin.MetaWebLogApi
{
	/// <summary>
	/// Represents a Media Object - this is usually an image, video, document etc..
	/// </summary>
	public class MediaObject
	{
		/// <summary>
		/// The name of the Media Object.
		/// </summary>
        public string Name { get; set; }

		/// <summary>
		/// The type of the Media Object.
		/// </summary>
        public string Type { get; set; }

		/// <summary>
		/// The byte array of the Media Object itself.
		/// 
		/// </summary>
        public byte[] Bits { get; set; }        
	}
}
