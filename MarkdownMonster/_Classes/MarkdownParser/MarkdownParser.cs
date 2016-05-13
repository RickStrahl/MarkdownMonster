using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonMark;
using Westwind.Utilities;

namespace MarkdownMonster
{
    public class MarkdownParser
    {
        public static MarkdownParser GetParser(MarkdownStyles markdownStyle, bool RenderLinksAsExternal = false)
        {
            if (RenderLinksAsExternal)
            {
                CommonMarkSettings.Default.OutputDelegate =
                    (doc, output, settings) => new ExternalLinkFormatter(output, settings).WriteDocument(doc);
            }

            if (markdownStyle == MarkdownStyles.GitHub)
                return new GithubMarkdownParser();
            
            return new MarkdownParser();
        }

        public string Parse(string markdown)
        {
            var html = CommonMark.CommonMarkConverter.Convert(markdown);
            html = ParseFontAwesomeIcons( html);
            return html;
        }

        public string ParseFontAwesomeIcons(string html)
        {
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
    }

    public class GithubMarkdownParser : MarkdownParser
    {        
    }
}
