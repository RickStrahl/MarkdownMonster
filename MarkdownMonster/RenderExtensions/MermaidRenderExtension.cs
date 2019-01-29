using Westwind.Utilities;

namespace MarkdownMonster.RenderExtensions
{

    /// <summary>
    /// Handles Mermaid charts based on one of two sytnax:
    ///
    /// <div class="mermaid">
    /// </div>
    ///
    /// or
    ///
    /// ```mermaid
    /// ```
    /// </summary>
    public class MermaidRenderExtension : IRenderExtension
    {
        public string RenderHeader(string html, string markdown, MarkdownDocument document)
        {
            if (markdown.Contains(" class=\"mermaid\"") || markdown.Contains("\n```mermaid"))
                return MermaidHeaderScript;

            return null;
        }

        /// <summary>
        /// Replace ```mermaid block with HTML equivalent
        /// </summary>
        /// <param name="html"></param>
        /// <param name="markdown"></param>
        /// <param name="document"></param>
        public void InsertContent(ref string html, string markdown, MarkdownDocument document)
        {

        }


        /// <summary>
        /// Check for ```markdown blocks and replace them with div blocks
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="document"></param>
        public void BeforeRender(ref string markdown, MarkdownDocument document)
        {           

            while (true)
            {
                string extract = StringUtils.ExtractString(markdown, "\n```mermaid", "```", returnDelimiters: true);
                if (string.IsNullOrEmpty(extract))
                    break;

                string newExtract = extract.Replace("```mermaid", "<div class=\"mermaid\">")
                    .Replace("```", "</div>");

                markdown = markdown.Replace(extract, newExtract);
            }


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
