namespace MarkdownMonster.RenderExtensions
{
    /// <summary>
    /// Handle MathText and MathML in the document using $$ for block operations and $ for inline
    /// Math expressions
    /// </summary>
    public class MathRenderExtension : IRenderExtension
    {

        public void BeforeMarkdownRendered(ModifyMarkdownArguments args)
        {
        }


        /// <summary>
        /// No content is added by this extension - it's all handled via script header and javascript events
        /// </summary>
        /// <param name="args"></param>
        public void AfterMarkdownRendered(ModifyHtmlAndHeadersArguments args)
        {
            if (args.Html.Contains(" class=\"math\"") || args.Markdown.Contains("useMath: true"))
                args.HeadersToEmbed = MathJaxScript;
        }


        /// <summary>
        /// After HTML has been rendered we need to make sure that
        /// script is rendered into the header.
        /// </summary>
        /// <param name="args"></param>
        public void AfterDocumentRendered(ModifyHtmlArguments args)
        {
            

        }

        public const string MathJaxScript = @"
<script type=""text/x-mathjax-config"">
    // enable inline parsing with single $ instead of /
    MathJax.Hub.Config({
        tex2jax: {
            //inlineMath: [['$','$'],['\\(','\\)']],
            //displayMath: [ ['$$','$$'], ['\\[','\\]'] ],
            processEscapes: true
        },
        //asciimath2jax: {
        //    delimiters: [['`','`']]
        //},
        TeX: {
            extensions: ['autoload-all.js']
        }
    });

    // refresh when the document is refreshed via code
    $(document).on('previewUpdated',function() {
        setTimeout(function() {
                    MathJax.Hub.Queue(['Typeset',MathJax.Hub,'#MainContent']);
        },10);
    });
</script>
<style>
    span.math span.MJXc-display {
        display: inline-block;
    }
</style>
<script src=""https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/latest.js?config=TeX-MML-AM_CHTML"" async></script>
";




    }
}
