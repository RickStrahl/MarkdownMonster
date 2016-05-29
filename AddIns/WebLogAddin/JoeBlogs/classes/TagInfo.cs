
namespace JoeBlogs
{
    /// <summary>
    /// Represents a tag.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// The id.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The name. This is also usually the textual representation of the tag.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The number of posts tagged with this tag.
        /// </summary>
        public string Count { get; set; }

        /// <summary>
        /// The slug of this tag - usually a lowercase version of the tag, with spaces replaced by hyphens.
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// The url for the search result page for the tag.
        /// </summary>
        public string HTMLUrl { get; set; }

        /// <summary>
        /// The url to the rss feed for the tag.
        /// </summary>
        public string RSSUrl { get; set; }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}