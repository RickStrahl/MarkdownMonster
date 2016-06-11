using System;
using CookComputing.XmlRpc;

namespace WebLogAddin.MetaWebLogApi
{
    /// <summary>
    /// Page.
    /// </summary>
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcPage
    {
        public int page_id;
        public string wp_slug;
        public string wp_password;
        public int wp_page_parent_id;
        public int wp_page_order;
        public string wp_author_id;
        public string title;
        public string description;
        public string mt_excerpt;
        public string mt_text_more;
        public int mt_allow_comments;
        public int mt_allow_pings;
        public DateTime dateCreated;

        public override string ToString()
        {
            return title;
        }
    }
}
