using System;

namespace WebLogAddin.MetaWebLogApi
{
	/// <summary>
	/// Page.
	/// </summary>
	public class PageMin
	{
        public DateTime DateCreated { get; set; }
        public int PageID { get; set; }
        public object ParentPageID { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }
	}
}