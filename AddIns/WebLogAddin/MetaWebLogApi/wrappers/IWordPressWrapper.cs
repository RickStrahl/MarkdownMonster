using System.Collections.Generic;
using System;

namespace WebLogAddin.MetaWebLogApi
{
    public interface IWordPressWrapper : IMetaWeblogWrapper
    {
        bool DeleteCategory(int categoryID);
        bool DeleteComment(string comment_id);
        bool DeletePage(string pageID);
        bool EditComment(string comment_id, CommentStatus status, DateTime date_created_gmt, string content, string author, string author_url, string author_email);
        bool EditPost(string postID, Post content, bool publish);
        bool EditPage(int pageID, Page editedPage, bool publish);
        IEnumerable<Author> GetAuthors();
        Comment GetComment(string comment_id);
        CommentCount GetCommentCount(string post_id);
        IEnumerable<Comment> GetComments(int post_id, string status, int number, int offset);
        IEnumerable<string> GetCommentStatusList(string post_id);
        IEnumerable<Option> GetOptions(string[] options);
        Page GetPage(string pageid);
        IEnumerable<PageMin> GetPageList();
        IEnumerable<Page> GetPages();
        IEnumerable<string> GetPageStatusList();
        IEnumerable<PageTemplate> GetPageTemplates();
        IEnumerable<string> GetPostStatusList();
        IEnumerable<Tag> GetTags();
        int NewCategory(string description, int? parentCategoryID, string name, string slug);
        string NewComment(int postid, int? comment_parent, string content, string author, string author_url, string author_email);
        string NewPage(Page page, bool publish);
        IEnumerable<Option> SetOptions();
        IEnumerable<CategoryMin> SuggestCategories(string startsWith, int max_results);
        File UploadFile(Data data);
    }
}