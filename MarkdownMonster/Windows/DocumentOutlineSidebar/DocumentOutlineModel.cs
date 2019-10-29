using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
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

            // create a full pipeline to strip out front matter/yaml
            var parser = new MarkdownParserMarkdig();
            var builder = parser.CreatePipelineBuilder();
            var syntax = Markdig.Markdown.Parse(md,builder.Build());

            var list = new ObservableCollection<HeaderItem>();

            foreach (var item in syntax)
            {
                var line = item.Line;

                if (item is HeadingBlock)
                {
                    var heading = item as HeadingBlock;
                    var inline = heading?.Inline?.FirstChild;
                    if (inline == null)
                        continue;

                    StringBuilder sb = new StringBuilder();
                    foreach (var inl in heading.Inline)
                    {
                        if (inl is HtmlEntityInline)
                        {
                            var htmlEntity = inl as HtmlEntityInline;
                            sb.Append(WebUtility.HtmlDecode(htmlEntity.Transcoded.ToString()));
                        }
                        else if (inl is CodeInline)
                        {
                            var codeInline = inl as CodeInline;
                            sb.Append(WebUtility.HtmlDecode(codeInline.Content.ToString()));
                        }
                        else
                            sb.Append(inl.ToString());
                    }
                    var content = sb.ToString();

                    if (content.Contains("\n"))
                        continue;

                    if (heading.Level > AppModel.Configuration.MaxDocumentOutlineLevel)
                        continue;

                    var headerItem = new HeaderItem()
                    {
                        Text = content,
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
        /// Search the outline for a given header text and return the line number
        /// or -1 on error
        /// </summary>
        /// <param name="md">Markdown document</param>
        /// <param name="headerLink">anchor to search for - should be generated using Github Style header encoding</param>
        /// <returns>line number or -1</returns>
        public int FindHeaderHeadline(string md, string headerLink)
        {
            if (string.IsNullOrEmpty(md))
                return -1;

            var syntax = Markdig.Markdown.Parse(md);
            var lines = StringUtils.GetLines(md);
            bool inFrontMatter = false;

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

                    var link = LinkHelper.UrilizeAsGfm(content.TrimEnd());
                    if (link == headerLink)
                        return line;
                }
            }

            return -1;
        }

        /// <summary>
        /// Creates a Markdown Outline for the active document
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public string CreateMarkdownOutline(MarkdownDocument document, int startLine = -1)
        {

            if (string.IsNullOrEmpty(document.CurrentText))
                return string.Empty;

            bool oldAutoHeaderIdentifiers = mmApp.Configuration.MarkdownOptions.AutoHeaderIdentifiers;
            mmApp.Configuration.MarkdownOptions.AutoHeaderIdentifiers = true;

            string origDocument = null;
            var sb = new StringBuilder();

            if (startLine > 0 )
            {
                origDocument = document.CurrentText;
                
                var lines = StringUtils.GetLines(origDocument).Skip(startLine-1);
                foreach (var line in lines)
                    sb.AppendLine(line);
                document.CurrentText = sb.ToString();
            }
            
            string html = document.RenderHtml();

            if (origDocument != null)   
                document.CurrentText = origDocument;

            mmApp.Configuration.MarkdownOptions.AutoHeaderIdentifiers = oldAutoHeaderIdentifiers;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            sb.Clear();
            
            var xpath = "//*[self::h1 or self::h2 or self::h3 or self::h4 or self::h5 or self::h6]";
            var nodes = doc.DocumentNode.SelectNodes(xpath);

            // nothing to do
            if (nodes == null)
                return null;


            var headers = new List<HeaderItem>();
            foreach (var node in nodes)
            {
                var id = node.Id;
                var text = node.InnerText.Trim();
                var textIndent = node.Name.Replace("h", "");
                if (!int.TryParse(textIndent, out int level) || level > AppModel.Configuration.MaxDocumentOutlineLevel)
                    continue;

                headers.Add(new HeaderItem {LinkId = id, Level = level, Text = text});
            }

            // if level starts > 1 adjust - so that min indent level is always 0
            int startOffset = headers.Min(h=> h.Level) - 1;
            if (startOffset < 0)
                startOffset = 0;

            int lastLevel = 0;  // check that we don't skip multiple levels - won't work in Markdown

            for (var index = 0; index < headers.Count; index++)
            {
                var header = headers[index];
                string leadin = null;
                int level = header.Level - startOffset;

                if (level > lastLevel + 1)
                {
                    var origLevel = level;
                    level = lastLevel + 1;

                    // read forward and adjust all items at the same level to this level
                    var index2 = index;
                    while (index2 < headers.Count && headers[index2].Level == origLevel)
                    {
                        headers[index2].Level = level;
                        index2++;
                    }
                }

                lastLevel = level;

                if (level > 0)
                    leadin = StringUtils.Replicate("\t", level - 1);

                sb.AppendLine($"{leadin}* [{header.Text}](#{header.LinkId})");
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
