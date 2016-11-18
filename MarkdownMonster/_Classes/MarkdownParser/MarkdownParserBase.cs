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
        /// Strips Front Matter headers at the beginning of the document
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        protected string StripFrontMatter(string html)
        {
            string fm = null;

            if (html.StartsWith("---\n") || html.StartsWith("---\r"))
                fm = mmFileUtils.ExtractString(html, "---", "---", returnDelimiters: true);

            if (fm == null || !fm.Contains("title: "))
                return html;
            
            return html.Replace(fm,"").TrimStart();
        }
        
        /// <summary>
        /// Parses out script tags that might not be encoded yet
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        protected string ParseScript(string html)
        {
            html = html.Replace("<script", "&lt;script");
            html = html.Replace("</script", "&lt;/script");
            html = html.Replace("javascript:", "javaScript:");
            return html;
        }


        public static Regex fontAwesomeIconRegEx = new Regex(@"@icon-.*?[\s|\.|\,|\<]");

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

    }
}