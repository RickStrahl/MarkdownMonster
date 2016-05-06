using System;
using System.Collections.Generic;
using System.Linq;

namespace JoeBlogs
{
    /// <summary>
    /// Represents a wrapper.
    /// </summary>
    public abstract class BaseWrapper : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseWrapper"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public BaseWrapper(string url, string username, string password)
            : this(url, username, password, 0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseWrapper"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="blogID">The blog ID.</param>
        public BaseWrapper(string url, string username, string password, int blogID)
        {
            Url = url;
            Username = username;
            Password = password;

            BlogID = blogID;

            if (BlogID == 0)
            {
                try { BlogID = GetUserBlogs().First().BlogID; }
                catch { BlogID = 1; }
            }
        }

        /// <summary>
        /// Gets the blogs for the user.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<UserBlog> GetUserBlogs();

        /// <summary>
        /// Gets or sets the blog ID.
        /// </summary>
        /// <value>The blog ID.</value>
        protected int BlogID { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        protected static string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        protected static string Password { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>The URL.</value>
        protected static string Url { get; set; }

        public abstract void Dispose();
    }
}