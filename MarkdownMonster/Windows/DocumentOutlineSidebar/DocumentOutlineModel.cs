using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using Markdig.Helpers;
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
        

    

        public ObservableCollection<HeaderItem> DocumentOutline
        {
            get { return _DocumentOutline; }
            set
            {
                if (value == _DocumentOutline) return;

                if (_DocumentOutline == null)
                {
                    _DocumentOutline = new ObservableCollection<HeaderItem>();
                    OnPropertyChanged(nameof(DocumentOutline));
                }
                else if (value == null)
                {                    
                    OnPropertyChanged(nameof(DocumentOutline));
                }

                _DocumentOutline.Clear();
                if (value != null)
                {
                    foreach (var item in value)
                        _DocumentOutline.Add(item);
                }                
                
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
                if (inFrontMatter)
                    continue;
                
                if (item is HeadingBlock)
                {                    
                    var heading = item as HeadingBlock;

                    if (heading.Level > AppModel.Configuration.MaxDocumentOutlineLevel)
                        continue;

                    // underlined format
                    if (line > 0 && (content.StartsWith("---") || content.StartsWith("===")))
                    {
                        line--;
                        content = lines[line].TrimStart(' ', '#');                        
                    }
                    
                    var headerItem = new HeaderItem()
                    {
                        Text = $"{content}",
                        Level = heading.Level,
                        Line = line,
                        LinkId = LinkHelper.UrilizeAsGfm(content.TrimEnd())
                    };

                    list.Add(headerItem);
                }
            }


            
            
            return list;
        }




        /// <summary>
        /// Creates a Markdown Outline for the active document
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public string CreateMarkdownOutline(MarkdownDocument document)
        {
            bool oldAutoHeaderIdentifiers = mmApp.Configuration.MarkdownOptions.AutoHeaderIdentifiers;
            mmApp.Configuration.MarkdownOptions.AutoHeaderIdentifiers = true;

            string html = document.RenderHtml();

            mmApp.Configuration.MarkdownOptions.AutoHeaderIdentifiers = oldAutoHeaderIdentifiers;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            StringBuilder sb = new StringBuilder();

            var xpath = "//*[self::h1 or self::h2 or self::h3 or self::h4]";
            int lastLevel = 0;
            foreach (var node in doc.DocumentNode.SelectNodes(xpath))
            {
                var id = node.Id;
                var text = node.InnerText.Trim();
                var textIndent = node.Name.Replace("h", "");
                if (!int.TryParse(textIndent, out int level) || level > AppModel.Configuration.MaxDocumentOutlineLevel)                   
                    continue;
                

                string leadin = null;
                if (level > lastLevel)
                    lastLevel++;
                else if (level < lastLevel)
                    lastLevel--;

                leadin = StringUtils.Replicate("\t",lastLevel - 1);

                sb.AppendLine($"{leadin}* [{text}](#{id})");
            }

            return sb.ToString();
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

        public string LinkId { get; set; }

        public Thickness Margin
        {
            get { return new Thickness((Level -1) * 20, 0, 0, 0); }
        }

        public List<HeaderItem> Children { get; set; }
    }
}
