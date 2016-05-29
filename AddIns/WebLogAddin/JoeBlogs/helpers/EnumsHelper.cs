using System;

namespace JoeBlogs
{
    static public class EnumsHelper
    {
        public static string GetCommentStatusName(CommentStatus status)
        {
            return Enum.GetName(typeof(CommentStatus), status);
        }
        public static CommentStatus GetCommentStatus(string commentStatus)
        {
            try
            {
                return (CommentStatus)Enum.Parse(typeof(CommentStatus), commentStatus, true);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(string.Format("Status value of '{0}' doesn't exist in the CommentStatus enum", commentStatus));
            }
        }
    }
}