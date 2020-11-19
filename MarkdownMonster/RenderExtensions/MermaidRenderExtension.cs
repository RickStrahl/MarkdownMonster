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

        private static string MermaidHeaderScript = $"\n<script src=\"{mmApp.Configuration.MarkdownOptions.MermaidDiagramsUrl}\"></script>\n" +
@"<script>
var _isIE = navigator.userAgent.indexOf(""MSIE"") > -1 || navigator.userAgent.indexOf(""Trident"") > -1;
if(_isIE) return;
mermaid.initialize({startOnLoad:false});
</script>
<script>
var _isIE = navigator.userAgent.indexOf(""MSIE"") > -1 || navigator.userAgent.indexOf(""Trident"") > -1;
function renderMermaid() {
    if (!_isIE) { 
        mermaid.init(undefined,document.querySelectorAll("".mermaid""));
    }
}   
$(function() {
    fixMermaidInInternetExplorer();    

    $(document).on('previewUpdated', function() {        
        if (_isIE) fixMermaidInInternetExplorer();  // removes .mermaid tags
        renderMermaid();
    });

    if (_isIE) return;
    renderMermaid();

    // Mermaid code no longer renders in IE - let's replace it with a link to view
    // in the browser
    function fixMermaidInInternetExplorer() {      
      var count = 0;

      var $div = $(""div.mermaid"");
      $div.each(function(i) {
        var $el = $(this);
        var id = $el.id;

        if (!id) {
          count++;
          id = ""mermaid"" + count;
          $el.attr(""id"", id);
        }

        if (!_isIE)
          return;

        var html = ""<div style='border: 1px solid #ccc; padding: 5px;'>"" +
          ""<i class='fa fa-line-chart'></i>&nbsp;"" + 
          ""<a href='"" + location.href + ""#"" + id + ""'>"" +
          ""Mermaid Diagram: Show in external browser</a>"" +
          ""</div>"";

        $el.before(html);
        $el.remove();
      });
    }
});

</script>";

    }
}
