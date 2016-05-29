using System;
using CookComputing.XmlRpc;

namespace JoeBlogs
{
    /// <summary>
    /// Represents a tag.
    /// </summary>
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcTagInfo
    {
        /// <summary>
        /// The id.
        /// </summary>
        public string tag_id;

        /// <summary>
        /// The name. This is also usually the textual representation of the tag.
        /// </summary>
        public string name;

        /// <summary>
        /// The number of posts tagged with this tag.
        /// </summary>
        public string count;

        /// <summary>
        /// The slug of this tag - usually a lowercase version of the tag, with spaces replaced by hyphens.
        /// </summary>
        public string slug;

        /// <summary>
        /// The url for the search result page for the tag.
        /// </summary>
        public string html_url;

        /// <summary>
        /// The url to the rss feed for the tag.
        /// </summary>
        public string rss_url;

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            return this.name;
        }
    }
}
