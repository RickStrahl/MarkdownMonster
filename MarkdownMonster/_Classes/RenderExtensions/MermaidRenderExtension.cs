using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return null;
        }

        public void InsertContent(ref string html, string markdown, MarkdownDocument document)
        {
            html += @"
<script src=""https://cdnjs.cloudflare.com/ajax/libs/mermaid/7.1.2/mermaid.min.js""></script>
<script>mermaid.initialize({startOnLoad:true});</script>
";
            document.ProcessScripts = true;
        }

    
        public bool ShouldProcess(string html, string markdown, MarkdownDocument document)
        {
            if (markdown.Contains(" class=\"mermaid\"") || markdown.Contains("```mermaid"))
                return true;

            return false;
        }

        public void BeforeRender(ref string markdown, MarkdownDocument document)
        {
        }

        public bool ShouldProcessBeforeRender(string markdown, MarkdownDocument document) => false;
    }

    /// <summary>
    /// Handle MathText and MathML in the document using $$ for block operations and $ for inline
    /// Math expressions
    /// </summary>
    public class MathRenderExtension : IRenderExtension
    {

        /// <summary>
        /// Embeds the MathJax header and processes dynamically when the document is updated.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="markdown"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public string RenderHeader(string html, string markdown, MarkdownDocument document)
        {
            var script = $@"
<script type=""text/x-mathjax-config"">
    // enable inline parsing with single $ instead of /
    MathJax.Hub.Config({{
                    tex2jax: {{inlineMath: [['$','$'], ['\\(','\\)']]}}
                }});
    $(document).on('previewUpdated',function() {{
            MathJax.Hub.Queue(['Typeset',MathJax.Hub,'MainContent']);   
    }});
</script>
<script async src=""https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/latest.js?config=TeX-MML-AM_CHTML""></script>
";
            return script;
        }


        /// <summary>
        /// No content is added by this extension - it's all handled via script header and javascript events
        /// </summary>
        /// <param name="html"></param>
        /// <param name="markdown"></param>
        /// <param name="document"></param>
        public void InsertContent(ref string html, string markdown, MarkdownDocument document)
        {
            
        }

        public void BeforeRender(ref string markdown, MarkdownDocument document)
        {
        }

        /// <summary>
        /// Look for YAML header and useMath: attribute
        /// </summary>
        /// <param name="html"></param>
        /// <param name="markdown"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public bool ShouldProcess(string html, string markdown, MarkdownDocument document)
        {
            if (markdown.StartsWith("---") && markdown.Contains("useMath:"))
                return true;

            return false;
        }

        public bool ShouldProcessBeforeRender(string markdown, MarkdownDocument document) => false;
    }

}
