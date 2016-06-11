
namespace WebLogAddin.MetaWebLogApi
{
    public class CategoryMin
    {
        private int _categoryID;
        public int CategoryID { get { return _categoryID; } }

        public string Name { get; set; }
    }

    public class Category
    {
        private long _categoryID;
        public long CategoryID { get { return _categoryID; } }
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