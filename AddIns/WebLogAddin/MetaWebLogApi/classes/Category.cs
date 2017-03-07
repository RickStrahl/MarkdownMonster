
namespace WebLogAddin.MetaWebLogApi
{
    public class CategoryMin
    {
        public int CategoryID { get; }

        public string Name { get; set; }
    }

    public class Category
    {
        public long CategoryID { get; }
        public long ParentCategoryID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string HtmlUrl { get; set; }
        public string RSSUrl { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}