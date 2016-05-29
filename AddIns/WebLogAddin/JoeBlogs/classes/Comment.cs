using System;

namespace JoeBlogs
{
    /// <summary>
    /// Represents a comment.
    /// This object can only be created as a result of a GetComment type request.
    /// </summary>
    public class Comment
    {
        public Comment() { }

        /// <summary>
        /// Gets or sets the date created.
        /// </summary>
        /// <value>
        /// The date created.
        /// </value>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        /// <value>
        /// The user ID.
        /// </value>
        public int UserID { get; set; }

        /// <summary>
        /// Gets or sets the comment ID.
        /// </summary>
        /// <value>
        /// The comment ID.
        /// </value>
        public int CommentID { get; set; }

        /// <summary>
        /// Gets or sets the parent comment ID.
        /// </summary>
        /// <value>
        /// The parent comment ID.
        /// </value>
        public int? ParentCommentID { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public CommentStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the content, which is the comment body.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the link.
        /// </summary>
        /// <value>
        /// The link.
        /// </value>
        public string Link { get; set; }

        /// <summary>
        /// Gets the post ID.
        /// </summary>
        public int PostID { get; set; }

        /// <summary>
        /// Gets or sets the post title.
        /// </summary>
        /// <value>
        /// The post title.
        /// </value>
        public string PostTitle { get; set; }

        /// <summary>
        /// Gets or sets the author IP.
        /// </summary>
        /// <value>
        /// The author IP.
        /// </value>
        public string AuthorIP { get; set; }

        /// <summary>
        /// Gets or sets the name of the author of the comment.
        /// </summary>
        /// <value>
        /// The name of the author.
        /// </value>
        public string AuthorName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the author of the comment
        /// </summary>
        /// <value>
        /// The author URL.
        /// </value>
        public string AuthorUrl { get; set; }

        /// <summary>
        /// Gets or sets the email address of the author of the comment.
        /// </summary>
        /// <value>
        /// The author email address
        /// </value>
        public string AuthorEmail { get; set; }
    }
}