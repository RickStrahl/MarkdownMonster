namespace MarkdownMonster
{
    internal class RenderExtensions
    {
        /// <summary>
        /// Checks for Math extensions and adds the appropriate MathJax script into the page.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="markdown"></param>
        public static void ProcessMath(ref string html, ref string markdown, MarkdownDocument document)
        {
            var script = $@"<script type=""text/x-mathjax-config"">
    // enable inline parsing with single $ instead of /
    MathJax.Hub.Config({{
                    tex2jax: {{inlineMath: [['$','$'], ['\\(','\\)']]}}
                }});
</script>
<script>
if(!window.MathJax) {{    
   var head = document.getElementsByTagName('head')[0];
   var script = document.createElement('script');   
   script.type = 'text/javascript';
   script.async = true;
   script.src=""https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/latest.js?config=TeX-MML-AM_CHTML"";
   head.appendChild(script);
}}    
else
{{    
MathJax.Hub.Queue([""Typeset"",MathJax.Hub,""MainContent""]);
}}
    
</script>";
            html = script + html;
            document.ProcessScripts = true;
        }


        /// <summary>
        /// Adds Mermaid to the active request
        /// </summary>
        /// <param name="html"></param>
        /// <param name="markdown"></param>
        public static void ProcessMermaid(ref string html, ref string markdown, MarkdownDocument document)
        {
            html += @"
<script src=""https://cdnjs.cloudflare.com/ajax/libs/mermaid/7.1.2/mermaid.min.js""></script>
<script>mermaid.initialize({startOnLoad:true});</script>
";
            document.ProcessScripts = true;
        }
    }
}
