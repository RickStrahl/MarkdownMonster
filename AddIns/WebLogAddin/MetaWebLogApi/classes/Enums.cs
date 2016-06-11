using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebLogAddin.MetaWebLogApi
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