using System;

namespace MarkdownMonster.AddIns
{
    public class AddInMenuItem
    {
        public string Caption { get; set; }

        public Action<AppModel,MarkdownMonsterAddin> Execute { get; set; }

        public Action<AppModel,MarkdownMonsterAddin>  CanExecute { get; set;  }

        /// <summary>
        /// Editor command that can be excuted (bold,italic,image etc.) 
        /// plus your own (or other) extended commands
        /// </summary>
        public string EditorCommand { get; set; }
    }
}