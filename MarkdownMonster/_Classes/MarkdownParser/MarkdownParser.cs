using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonMark;

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
            return html;
        }
    }

    public class GithubMarkdownParser : MarkdownParser
    {        
    }
}
