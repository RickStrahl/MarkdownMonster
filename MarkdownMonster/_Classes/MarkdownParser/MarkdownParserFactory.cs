using CommonMark;
using Microsoft.DocAsCode.MarkdownLite;

namespace MarkdownMonster
{
    /// <summary>
    /// Retrieves an instance of a markdown parser
    /// </summary>
    public static class MarkdownParserFactory
    {

        /// <summary>
        /// Retrieves a cached instance of the markdown parser
        /// </summary>
        /// <param name="markdownStyle"></param>
        /// <param name="RenderLinksAsExternal"></param>
        /// <returns></returns>
        public static IMarkdownParser GetParser(MarkdownStyles markdownStyle, bool RenderLinksAsExternal = false)
        {
            if (mmApp.Configuration.MarkdownParser == MarkdownParsers.MarkdownLite)
                return new MarkdownParserMarkdownLite();
            if (mmApp.Configuration.MarkdownParser == MarkdownParsers.Markdig)
                return new MarkdownParserMarkdig();

            if (RenderLinksAsExternal)
            {
                CommonMarkSettings.Default.OutputDelegate =
                    (doc, output, settings) => new ExternalLinkFormatter(output, settings).WriteDocument(doc);
            }
            return new MarkdownParserCommonMarkNet();
        }

    }

    public enum MarkdownParsers
    {
        CommonMarkNet,
        MarkdownLite,
        Markdig
    }
}