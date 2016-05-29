using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JoeBlogs
{
    public enum PostStatus
    {
        Draft,
        Pending,
        Private,
        Publish
    }

    public enum CommentStatus
    {
        Hold,
        Approve,
        Spam
    }
}