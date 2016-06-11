using System;

namespace WebLogAddin.MetaWebLogApi
{
    /// <summary> 
    /// Represents information about a term. 
    /// </summary> 
    public class Term
    {
        public string Taxonomy { get; set; }
        public string[] Terms { get; set; }
    }
}
