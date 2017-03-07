using System;

namespace WebLogAddin.MetaWebLogApi
{
    public class Page
    {
        public int PageID { get; }
        public int ParentPageID { get; set; }
        public string Slug { get; set; }
        public string Password { get; set; }
        public int PageOrder { get; set; }
        public int AuthorID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Excerpt { get; set; }
        public string mt_text_more { get; set; }
        public bool AllowComments { get; set; }
        public bool AllowPings { get; set; }
        public DateTime DateCreated { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}