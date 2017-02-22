using MarkdownMonster.AddIns;

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
        /// <param name="renderLinksAsExternal">If true renders links with target="top"</param>
        /// <param name="forceLoad">Forces the parser to be reloaded - otherwise previously loaded instance is used</param>
        /// <param name="usePragmaLines">If true adds pragma line ids into the document that the editor can sync to</param>
        /// <returns>Mardown Parser Interface</returns>
        public static IMarkdownParser GetParser(bool renderLinksAsExternal = false, bool usePragmaLines = false, bool forceLoad = false)
        {
            var parser = AddinManager.Current.RaiseOnCreateMarkdownParser() as IMarkdownParser;

            if (parser == null)
                return new MarkdownParserMarkdig(usePragmaLines: usePragmaLines, force: forceLoad);

            return parser;
        }

    }

    
}