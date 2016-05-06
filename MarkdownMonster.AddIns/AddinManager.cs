using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownMonster.AddIns
{
    public class AddinManager
    {
        public static AddinManager LoadedAddIns { get; set; }

        public List<MarkdownMonsterAddin> AddIns;


        static AddinManager()
        {
            LoadedAddIns = new AddinManager();
        }

        public AddinManager()
        {
            AddIns = new List<MarkdownMonsterAddin>();
        }




    }
}
