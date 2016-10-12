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
        /// Post processing routine that post-processes the HTML and 
        /// replaces @icon- with fontawesome icons
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        protected string ParseFontAwesomeIcons(string html)
        {
            if (html == null)
                return null;

            while (true)
            {
                string iconBlock = StringUtils.ExtractString(html, "@icon-", " ", false, false, true);
                if (string.IsNullOrEmpty(iconBlock))
                    break;

                string icon = iconBlock.Replace("@icon-", "").Trim();
                html = html.Replace(iconBlock, "<i class=\"fa fa-" + icon + "\"></i> ");
            }
            return html;
        }


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

                val = "<strikeout>" + val.Substring(2, val.Length - 4) + "</strikeout>";
                html = html.Replace(match.Value, val);
            }

            return html;
        }

    }
}