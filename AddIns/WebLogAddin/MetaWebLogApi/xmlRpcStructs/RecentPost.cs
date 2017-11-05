using System;
using CookComputing.XmlRpc;

namespace WebLogAddin.MetaWebLogApi
{
    /// <summary> 
    /// This struct represents the information about a post that could be returned by the 
    /// EditPost() and GetPost() methods. 
    /// </summary> 
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcRecentPost
    {
        public DateTime dateCreated;
        public string userid;
        public string description;
        public string title;
        public object postid;
        public string link;
        public string permaLink;
        public string[] categories;

        public string mt_excerpt;
        public string mt_text_more;
        public string wp_more_text;
        public int mt_allow_comments;
        public int mt_allow_pings;
        public string mt_keywords;
        public string wp_slug;
        public string wp_password;
        public string wp_author_id;
        public string wp_author_display_name;
        public string post_status;
        public string post_type;

        public XmlRpcCustomField[] custom_fields;
        public XmlRpcTerm[] terms;

        public override string ToString()
        {
            return description;
        }
    }
}
