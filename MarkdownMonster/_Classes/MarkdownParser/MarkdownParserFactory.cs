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
        /// <param name="renderLinksAsExternal"></param>
        /// <returns></returns>
        public static IMarkdownParser GetParser(bool renderLinksAsExternal = false, bool usePragmaLines = false, bool forceLoad = false)
        {            
            return new MarkdownParserMarkdig(usePragmaLines: usePragmaLines, force: forceLoad);
        }

    }

    
}