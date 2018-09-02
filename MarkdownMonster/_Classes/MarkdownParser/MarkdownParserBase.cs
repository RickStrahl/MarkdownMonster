using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Westwind.Utilities;

namespace MarkdownMonster
{
    /// <summary>
    /// Base class that includes various fix up methods for custom parsing
    /// that can be called by the specific implementations.
    /// </summary>
    public abstract class MarkdownParserBase : IMarkdownParser
    {
        protected static Regex strikeOutRegex = 
            new Regex("~~.*?~~", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// Parses markdown
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public abstract string Parse(string markdown);
        
        /// <summary>
        /// Parses strikeout text ~~text~~. Single line (to linebreak) allowed only.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        protected string ParseStrikeout(string html)
        {
            if (html == null)
                return null;

            var matches = strikeOutRegex.Matches(html);
            foreach (Match match in matches)
            {
                string val = match.Value;

                if (match.Value.Contains('\n'))
                    continue;

                val = "<del>" + val.Substring(2, val.Length - 4) + "</del>";
                html = html.Replace(match.Value, val);
            }

            return html;
        }

        /// <summary>
        /// Strips 
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public string StripFrontMatter(string markdown)
        {
            string extractedYaml = null;
            var match = MarkdownUtilities.YamlExtractionRegex.Match(markdown);
            if (match.Success)
                extractedYaml = match.Value;

            if (!string.IsNullOrEmpty(extractedYaml))
                markdown = markdown.Replace(extractedYaml, "");

            return markdown;
        }        




        protected static Regex fontAwesomeIconRegEx = new Regex(@"@icon-.*?[\s|\.|\,|\<]");

        /// <summary>
        /// Post processing routine that post-processes the HTML and 
        /// replaces @icon- with fontawesome icons
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        protected string ParseFontAwesomeIcons(string html)
        {
            var matches = fontAwesomeIconRegEx.Matches(html);
            foreach (Match match in matches)
            {
                string iconblock = match.Value.Substring(0, match.Value.Length - 1);
                string icon = iconblock.Replace("@icon-", "");
                html = html.Replace(iconblock, "<i class=\"fa fa-" + icon + "\"></i> ");
            }

            return html;
        }


        private static Regex includeFileRegEx = new Regex(@"\[\!include.*?\[.*?\]\(.*?\)\]",RegexOptions.IgnoreCase);

        /// <summary>
        /// Parses DocFx include files in the format of:
        ///
        ///    [!include[title](relativePathToFileToInclude>)]
        ///
        /// Should run **prior** to Markdown parsing of the main document
        /// as it will embed the file content as is.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        protected string ParseDocFxIncludeFiles(string html)
        {
            var matches = includeFileRegEx.Matches(html);
            foreach (Match match in matches)
            {
                string value = match.Value;

                //string title = StringUtils.ExtractString(value, "[!include[", "]");
                string file = StringUtils.ExtractString(value, "](", ")]");
                if (string.IsNullOrEmpty(file))
                    continue;

                if (file.StartsWith("~"))                                    
                    file = mmApp.Model.Window.FolderBrowser.FolderPath + file.Substring(1);
                

                string filePath;
                if (file.Contains(@":\"))
                {
                    filePath = file;
                }
                else
                {
                    filePath = mmApp.Model.ActiveDocument?.Filename;
                    if (string.IsNullOrEmpty(filePath))
                        continue;

                    filePath = Path.GetDirectoryName(filePath);
                    if (filePath == null)
                        continue;
                }


                string includeFile = Path.Combine(filePath, file);
                includeFile = FileUtils.NormalizePath(includeFile);
                if (!File.Exists(includeFile))
                    continue;

                var markdownDocument = new MarkdownDocument();
                markdownDocument.Load(includeFile);
                string includeContent = markdownDocument.RenderHtml();

                html = html.Replace(value, includeContent);
            }

            return html;
        }


        /// <summary>
        /// Replaces all links with target="top" links
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        protected string ParseExternalLinks(string html)
        {
            return html.Replace("<a href=", "<a target=\"top\" href=");
        }

    }
}
