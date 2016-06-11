using System;
using CookComputing.XmlRpc;

namespace WebLogAddin.MetaWebLogApi
{
    /// <summary> 
    /// This struct represents information about a user. 
    /// </summary> 
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct XmlRpcAuthor
    {
        public string user_id;
        public string user_email;
        public string user_login;
        public string display_name;

        public override string ToString()
        {
            return display_name;
        }
    }
}