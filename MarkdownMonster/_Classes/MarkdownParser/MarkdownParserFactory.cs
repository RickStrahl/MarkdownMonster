using System.Collections.Generic;
using System.Linq;
using MarkdownMonster.AddIns;

namespace MarkdownMonster
{
    /// <summary>
    /// Retrieves an instance of a markdown parser
    /// </summary>
    public static class MarkdownParserFactory
    {

        /// <summary>
        /// Use a cached instance of the Markdown Parser to keep alive
        /// </summary>
        public static IMarkdownParser CurrentParser;

        /// <summary>
        /// Retrieves a cached instance of the markdown parser
        /// </summary>        
        /// <param name="renderLinksAsExternal">If true renders links with target="top"</param>
        /// <param name="forceLoad">Forces the parser to be reloaded - otherwise previously loaded instance is used</param>
        /// <param name="usePragmaLines">If true adds pragma line ids into the document that the editor can sync to</param>
        /// <param name="addinId">optional addin id that checks for a registered Markdown parser</param>
        /// <returns>Mardown Parser Interface</returns>
        public static IMarkdownParser GetParser(bool renderLinksAsExternal = false, bool usePragmaLines = false,
                                                bool forceLoad = false, string addinId = null)
        {
            if (!forceLoad && CurrentParser != null)
                return CurrentParser;

            IMarkdownParser parser = null;

            if (!string.IsNullOrEmpty(addinId) && addinId != "MarkDig")
            {
                var addin = AddinManager.Current.AddIns.FirstOrDefault(a => a.Name == addinId || a.Id == addinId);
                if (addin != null)
                    parser = addin.GetMarkdownParser();
            }
            
            CurrentParser = parser ?? (parser = new MarkdownParserMarkdig(usePragmaLines: usePragmaLines, force: forceLoad));

            return CurrentParser;
        }


        /// <summary>
        /// Gets a list of all registered markdown parsers that live in an addin
        /// </summary>
        /// <returns></returns>
        public static List<string> GetParserNames()
        {

            var parserStrings = new List<string>()
            {
                {"MarkDig"}
            };
            foreach (var addin in AddinManager.Current.AddIns)
            {
                var parser = addin.GetMarkdownParser();
                if (parser != null)
                    parserStrings.Add(addin.Name ?? addin.Id);
            }

            return parserStrings;
        }
        
    }

    
}