using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Markdig.Syntax;
using MarkdownMonster.Annotations;
using Westwind.Utilities;

namespace MarkdownMonster.Windows.DocumentOutlineSidebar
{
    public class DocumentOutlineModel : INotifyPropertyChanged
    {
        public DocumentOutlineModel()
        {            
            AppModel = mmApp.Model;
            Window = AppModel.Window;
            Commands = AppModel.Commands;            
        }

        public AppModel AppModel{ get; set; }

        public AppCommands Commands { get; set; }

        public MainWindow Window { get; set; }

        private ObservableCollection<HeaderItem> _documentOutline;


        public ObservableCollection<HeaderItem> DocumentOutline
        {
            get { return _DocumentOutline; }
            set
            {
                if (value == _DocumentOutline) return;
                _DocumentOutline = value;
                OnPropertyChanged(nameof(DocumentOutline));
            }
        }
        private ObservableCollection<HeaderItem> _DocumentOutline;



        public ObservableCollection<HeaderItem> CreateDocumentOutline(string md)
        {
            if (string.IsNullOrEmpty(md))
                return null;

            var syntax = Markdig.Markdown.Parse(md);
            var lines = StringUtils.GetLines(md);
            bool inFrontMatter = false;

            var list = new ObservableCollection<HeaderItem>();

            foreach (var item in syntax)
            {
                var line = item.Line;
                var content = lines[line].TrimStart(' ', '#'); ;

                if (line == 0 && content == "---")
                {
                    inFrontMatter = true;
                    continue;
                }
                if (inFrontMatter && content == "---")
                {
                    inFrontMatter = false;
                    continue;
                }

                if (item is HeadingBlock)
                {
                    
                    var heading = item as HeadingBlock;
                    var indent = "".PadRight(heading.Level * 2);

                    // underlined format
                    if (line > 0 && (content.StartsWith("---") || content.StartsWith("===")))
                    {
                        line--;
                        content = lines[line].TrimStart(' ', '#');                        
                    }

                    var headerItem = new HeaderItem()
                    {
                        Text = $"{indent}{content}",
                        Level = heading.Level,
                        Line = line
                    };
                    list.Add(headerItem);
                }
            }

            return list;
        }

        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class HeaderItem
    {
        public string Text { get; set; }
        public int Line { get; set; }
        public int Level { get; set; }
        public List<HeaderItem> Children { get; set; }
    }
}
