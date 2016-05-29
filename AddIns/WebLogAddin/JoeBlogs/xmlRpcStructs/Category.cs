using System;
using CookComputing.XmlRpc;

namespace JoeBlogs
{
    /// <summary> 
    /// This struct represents the information about a category that could be returned by the 
    /// getCategories() method. 
    /// </summary> 
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcCategory
    {
        public string categoryId;
        public string parentId;
        public string description;
        public string categoryName;
        public string title;
        public string htmlUrl;
        public string rssUrl;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcCategoryNew
    {
        public string name;
        public string slug;
        public int parent_id;
        public string description;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcCategoryMin
    {
        public int category_id;
        public string name;
    }
}