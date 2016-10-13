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
            return new MarkdownParserMarkdig();
        }

    }

    
}