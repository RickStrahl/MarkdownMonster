using Westwind.Utilities;

namespace MarkdownMonster.RenderExtensions
{   

    /// <summary>
    /// Handles Mermaid charts based on one of two sytnax:
    ///
    /// * Converts ```mermaid syntax into div syntax
    /// * Adds the mermaid script from CDN
    /// </summary>
    public class MermaidRenderExtension : IRenderExtension
    {
        /// <summary>
        /// Add script block into the document
        /// </summary>
        /// <param name="args"></param>
        public void AfterMarkdownRendered(ModifyHtmlAndHeadersArguments args)
        {
            if (args.Markdown.Contains(" class=\"mermaid\"") || args.Markdown.Contains("\n```mermaid"))
                args.HeadersToEmbed = MermaidHeaderScript;
        }
        
        /// <summary>
        /// Check for ```markdown blocks and replace them with DIV blocks
        /// </summary>
        /// <param name="args"></param>
        public void BeforeMarkdownRendered(ModifyMarkdownArguments args)
        {
            while (true)
            {
                string extract = StringUtils.ExtractString(args.Markdown, "\n```mermaid", "```", returnDelimiters: true);
                if (string.IsNullOrEmpty(extract))
                    break;

                string newExtract = extract.Replace("```mermaid", "<div class=\"mermaid\">")
                    .Replace("```", "</div>");

                args.Markdown = args.Markdown.Replace(extract, newExtract);
            }
        }

        /// <summary>
        /// Embed the Mermaid script link into the head of the page
        /// </summary>
        /// <param name="args"></param>
        public void AfterDocumentRendered(ModifyHtmlArguments args)
        {

        }



        private const string MermaidHeaderScript =
            @"<script src=""https://cdnjs.cloudflare.com/ajax/libs/mermaid/7.1.2/mermaid.min.js""></script>
<script>
mermaid.initialize({startOnLoad:false});
</script>
<script>
function renderMermaid(){
    mermaid.init(undefined,document.querySelectorAll("".mermaid""));
}
$(function() {
    $(document).on('previewUpdated', function() {        
        renderMermaid();
    });
    renderMermaid();
});
</script>";

    }
}
