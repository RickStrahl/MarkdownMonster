using System;
using System.Reflection;
using System.Linq;

namespace WebLogAddin.MetaWebLogApi
{
    internal static class Map
    {
        static void SetPrivateFieldValue<T>(string fieldName, object value, T obj)
        {
            var propertyInfo = typeof(T)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (propertyInfo != null)
                propertyInfo.SetValue(obj, value);
        }

        internal static class From
        {
            internal static XmlRpcAuthor Author(Author input)
            {
                var content = new XmlRpcAuthor
                                  {
                                      display_name = input.DisplayName,
                                      user_email = input.EmailAddress,
                                      user_id = input.UserID,
                                      user_login = input.LoginName
                                  };
                return content;
            }
            internal static XmlRpcCategory Category(Category input)
            {
                return new XmlRpcCategory
                {
                    categoryId = Convert.ToString(input.CategoryID),
                    categoryName = input.Name,
                    htmlUrl = input.HtmlUrl,
                    rssUrl = input.RSSUrl,
                    title = input.Name,
                    description = input.Description,
                    parentId = Convert.ToString(input.ParentCategoryID),
                };
            }

            internal static XmlRpcComment Comment(Comment input)
            {
                return new XmlRpcComment
                                {
                                    author = input.AuthorName,
                                    author_email = input.AuthorEmail,
                                    author_url = input.AuthorUrl,
                                    author_ip = input.AuthorIP,
                                    parent = Convert.ToString(input.ParentCommentID),
                                    content = input.Content,
                                    comment_id = Convert.ToString(input.CommentID),
                                    dateCreated = input.DateCreated,
                                    link = input.Link,
                                    post_id = Convert.ToString(input.PostID),
                                    post_title = input.PostTitle
                                };
            }
            internal static XmlRpcCommentCount CommentCount(CommentCount input)
            {
                return new XmlRpcCommentCount
                {
                    approved = input.Approved,
                    awaiting_moderation = input.AwaitingModeration,
                    spam = input.Spam,
                    total_comments = input.TotalComments
                };
            }
            internal static XmlRpcCommentFilter CommentFilter(CommentFilter input)
            {
                return new XmlRpcCommentFilter
                {
                    number = input.Number,
                    offset = input.Offset,
                    post_id = input.PostID,
                    status = input.Status
                };
            }
            internal static XmlRpcCustomField CustomField(CustomField input)
            {
                return new XmlRpcCustomField
                {
                    id = input.ID,
                    key = input.Key,
                    value = input.Value
                };
            }
            internal static XmlRpcData Data(Data input)
            {
                return new XmlRpcData
                {
                    bits = input.Bits,
                    name = input.Name,
                    overwrite = input.Overwrite,
                    type = input.Type
                };
            }
            internal static XmlRpcFile File(File input)
            {
                return new XmlRpcFile
                {
                    file = input.FileContent,
                    type = input.Type,
                    url = input.URL
                };
            }
            internal static XmlRpcMediaObject MediaObject(MediaObject input)
            {
                return new XmlRpcMediaObject
                {
                    bits = input.Bits,
                    name = input.Name,
                    type = input.Type
                };
            }
            internal static XmlRpcMediaObjectInfo MediaObjectInfo(MediaObjectInfo input)
            {
                return new XmlRpcMediaObjectInfo
                {
                    url = input.URL
                };
            }
            internal static XmlRpcOption Option(Option input)
            {
                return new XmlRpcOption
                {
                    option = input.Key,
                    value = input.Value
                };
            }
            internal static XmlRpcPage Page(Page input)
            {
                return new XmlRpcPage
                           {
                               dateCreated = input.DateCreated,
                               description = input.Body,
                               mt_allow_comments = input.AllowComments ? 1 : 0,
                               mt_allow_pings = input.AllowPings ? 1 : 0,
                               mt_excerpt = input.Excerpt,
                               mt_text_more = input.mt_text_more,
                               title = input.Title,
                               wp_author_id = input.AuthorID.ToString(),
                               wp_page_order = input.PageOrder,
                               wp_page_parent_id = input.ParentPageID,
                               wp_password = input.Password,
                               wp_slug = input.Slug
                           };
            }
            internal static XmlRpcPageMin PageMin(PageMin input)
            {
                return new XmlRpcPageMin
                           {
                               dateCreated = input.DateCreated,
                               page_parent_id = input.ParentPageID,
                               page_title = input.Title
                           };
            }
            internal static XmlRpcPageTemplate PageTemplate(PageTemplate input)
            {
                return new XmlRpcPageTemplate
                           {
                               description = input.Description,
                               name = input.Name
                           };
            }
            internal static XmlRpcPostStatusList PostStatusList(PostStatusList input)
            {
                return new XmlRpcPostStatusList
                           {
                               Status = input.Status
                           };
            }
            internal static XmlRpcTagInfo Tag(Tag input)
            {
                return new XmlRpcTagInfo
                           {
                               count = input.Count,
                               html_url = input.HTMLUrl,
                               tag_id = input.ID,
                               name = input.Name,
                               rss_url = input.RSSUrl,
                               slug = input.Slug
                           };
            }
            internal static XmlRpcUserBlog UserBlog(UserBlog input)
            {
                return new XmlRpcUserBlog
                           {
                               blogId = input.BlogID,
                               blogName = input.BlogName,
                               isAdmin = input.IsAdmin,
                               url = input.URL,
                               xmlrpc = input.XMLRpc
                           };
            }
            internal static XmlRpcUserInfo UserInfo(UserInfo input)
            {
                return new XmlRpcUserInfo
                           {
                               blogId = input.BlogID,

                           };
            }
            internal static XmlRpcPost Post(Post input)
            {
                return new XmlRpcPost
                {
                    categories = input.Categories,
                    dateCreated = input.DateCreated,
                    description = input.Body,
                    mt_text_more = input.mt_text_more,                              
                    post_content = input.post_content,
                    mt_keywords = input.Tags == null ? input.mt_keywords : String.Join(",", input.Tags),
                    postid = input.PostID,
                    title = input.Title,
                    permaLink = input.Permalink,
                    post_type = input.PostType,
                    mt_excerpt = input.mt_excerpt,                          
                    custom_fields = input.CustomFields == null ? null : input.CustomFields.Select(cf => new XmlRpcCustomField()
                    {
                        id = cf.ID,
                        key = cf.Key,
                        value = cf.Value
                    }).ToArray(),
                    terms = input.Terms == null ? null : input.Terms.Select(t => new XmlRpcTerm()
                    {
                        taxonomy = t.Taxonomy,
                        terms = t.Terms
                    }).ToArray()
                };
            }
        }




        internal static class To
        {
            internal static Author Author(XmlRpcAuthor input)
            {
                return new Author
                {
                    DisplayName = input.display_name,
                    EmailAddress = input.user_email,
                    LoginName = input.user_login,
                    UserID = input.user_id
                };
            }

            internal static Comment Comment(XmlRpcComment input)
            {
                ConstructorInfo ctor = typeof(Comment).GetConstructors
                    (BindingFlags.Instance | BindingFlags.Public)[0];

                var result = (Comment)ctor.Invoke(new object[] { });

                result.AuthorEmail = input.author_email;
                result.AuthorIP = input.author_ip;
                result.AuthorName = input.author;
                result.AuthorUrl = input.author_url;
                result.CommentID = Convert.ToInt16(input.comment_id);
                result.Content = input.content;
                result.DateCreated = input.dateCreated;
                result.Link = input.link;
                result.PostID = Convert.ToInt16(input.post_id);
                result.PostTitle = input.post_title;
                result.UserID = Convert.ToInt16(input.user_id);
                result.Status = EnumsHelper.GetCommentStatus(input.status);

                return result;
            }

            internal static Post Post(XmlRpcPost input)
            {
                return new Post
                {
                    PostID = input.postid,
                    Body = input.description,
                    mt_text_more = input.mt_text_more,
                    post_content = input.post_content,                    
                    Categories = input.categories,
                    DateCreated = input.dateCreated,
                    Tags = input.mt_keywords != null ? input.mt_keywords.Split(',') : null,
                    Title = input.title,
                    Permalink = input.permaLink,
                    mt_excerpt = input.mt_excerpt,
                    mt_keywords = input.mt_keywords,
                    PostType = input.post_type,
                    CustomFields = input.custom_fields == null ? null : input.custom_fields.Select(cf => new CustomField()
                    {
                        ID = cf.id,
                        Key = cf.key,
                        Value = cf.value
                    }).ToArray(),
                    Terms = input.terms == null ? null : input.terms.Select(t => new Term()
                    {
                        Taxonomy = t.taxonomy,
                        Terms = t.terms
                    }).ToArray()
                };
            }

            internal static Post Post(XmlRpcRecentPost input)
            {
                return new Post
                {
                    PostID = input.postid,
                    Body = input.description,
                    Categories = input.categories,
                    DateCreated = input.dateCreated,
                    Tags = input.mt_keywords?.Split(','),
                    Title = input.title,
                    Permalink = input.permaLink,
                    PostType = input.post_type,
                    mt_keywords = input.mt_keywords,
                    mt_excerpt = input.mt_excerpt,
                    CustomFields = input.custom_fields == null ? null : input.custom_fields.Select(cf => new CustomField()
                    {
                        ID = cf.id,
                        Key = cf.key,
                        Value = cf.value
                    }).ToArray(),
                    Terms = input.terms == null ? null : input.terms.Select(t => new Term()
                    {
                        Taxonomy = t.taxonomy,
                        Terms = t.terms
                    }).ToArray()
                };
                
            }

            internal static Page Page(XmlRpcPage input)
            {
                var result = new Page
                                 {
                                     AllowComments = (input.mt_allow_comments == 1),
                                     AllowPings = (input.mt_allow_comments == 1),
                                     AuthorID = Convert.ToInt32(input.wp_author_id),
                                     Body = input.description,
                                     DateCreated = input.dateCreated,
                                     Excerpt = input.mt_excerpt,
                                     mt_text_more = input.mt_text_more,
                                     PageOrder = input.wp_page_order,
                                     ParentPageID = input.wp_page_parent_id,
                                     Password = input.wp_password,
                                     Slug = input.wp_slug,
                                     Title = input.title
                                 };

                SetPrivateFieldValue("_pageID", input.page_id, result);

                return result;
            }

            internal static Category Category(XmlRpcCategory input)
            {
                var result = new Category
                                 {
                                     ParentCategoryID = Convert.ToInt32(input.parentId),
                                     Name = input.categoryName,
                                     Description = input.description,
                                     HtmlUrl = input.htmlUrl,
                                     RSSUrl = input.rssUrl,
                                 };

                SetPrivateFieldValue("_categoryID", Convert.ToInt32(input.categoryId), result);

                return result;
            }

            internal static CategoryMin CategoryMin(XmlRpcCategoryMin input)
            {
                var result = new CategoryMin
                                 {
                                     Name = input.name,
                                 };

                SetPrivateFieldValue("_categoryID", Convert.ToInt16(input.category_id), result);

                return result;
            }

            internal static UserBlog UserBlog(XmlRpcUserBlog input)
            {
                var result = new UserBlog
                {
                    BlogName = input.blogName,
                    IsAdmin = input.isAdmin,
                    URL = input.url
                };

                SetPrivateFieldValue("_userBlogID", input.blogId, result);

                return result;
            }

            internal static File File(XmlRpcFile input)
            {
                return new File
                {
                    FileContent = input.file,
                    Type = input.type,
                    URL = input.url
                };
            }

            internal static MediaObjectInfo MediaObjectInfo(XmlRpcMediaObjectInfo input)
            {
                return new MediaObjectInfo
                {
                    URL = input.url
                };
            }

            internal static CommentCount CommentCount(XmlRpcCommentCount input)
            {
                return new CommentCount
                {
                    Approved = input.approved,
                    AwaitingModeration = input.awaiting_moderation,
                    Spam = input.spam,
                    TotalComments = input.total_comments
                };
            }

            internal static Option Option(XmlRpcOption input)
            {
                return new Option
                {
                    Key = input.option,
                    Value = input.value
                };
            }

            internal static PageMin PageMin(XmlRpcPageMin input)
            {
                return new PageMin
                {
                    DateCreated = input.dateCreated,
                    PageID = Convert.ToInt16(input.page_id),
                    ParentPageID = input.page_parent_id,
                    Title = input.page_title
                };
            }

            internal static Tag TagInfo(XmlRpcTagInfo input)
            {
                return new Tag
                {
                    ID = input.tag_id,
                    Count = input.count,
                    HTMLUrl = input.html_url,
                    Name = input.name,
                    RSSUrl = input.rss_url,
                    Slug = input.slug
                };
            }

            internal static PageTemplate PageTemplate(XmlRpcPageTemplate input)
            {
                return new PageTemplate
                {
                    Description = input.description,
                    Name = input.name
                };
            }

            internal static UserInfo UserInfo(XmlRpcUserInfo input)
            {
                return new UserInfo
                {
                    BlogID = input.blogId,
                    BlogName = input.blogName,
                    Email = input.email,
                    FirstName = input.firstname,
                    LastName = input.lastname,
                    Nickname = input.nickname,
                    URL = input.url
                };
            }
        }
    }
}
