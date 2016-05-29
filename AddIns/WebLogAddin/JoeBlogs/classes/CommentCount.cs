
namespace JoeBlogs
{
    public class CommentCount
    {
        /// <summary>
        /// The number of comments marked as approved.
        /// </summary>
        public object Approved;

        /// <summary>
        /// The number of comments awaiting moderation
        /// </summary>
        public object AwaitingModeration;

        /// <summary>
        /// The number of comments marked as spam
        /// </summary>
        public object Spam;

        /// <summary>
        /// The total number of comments.
        /// </summary>
        public object TotalComments;
    }
}
