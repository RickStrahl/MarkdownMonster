using System.Collections.Generic;

namespace MarkdownMonster.RenderExtensions
{
    /// <summary>
    /// Manages any render extensions for the HTML
    /// </summary>
    public class RenderExtensionsManager
    {

        /// <summary>
        /// Global application instance of the Extensions Manager
        /// </summary>
        public static RenderExtensionsManager Current { get; }

        /// <summary>
        /// Active Render Extensions. You can add your own extensions on application startup
        /// </summary>
        public List<IRenderExtension> RenderExtensions = new List<IRenderExtension>();

        static RenderExtensionsManager()
        {
            Current = new RenderExtensionsManager();
            Current.LoadDefaultExtensions();
        }

        public void ProcessAllExtensions(ref string html, string markdown, MarkdownDocument document)
        {
            foreach (var extension in RenderExtensions)
            {
                ProcessExtension(extension,ref  html, markdown, document);
            }
        }

        /// <summary>
        /// Process all BeforeRender Extensions
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="document"></param>
        public void ProcessAllBeforeRenderExtensions(ref string markdown, MarkdownDocument document)
        {
            foreach (var extension in RenderExtensions)
            {
                extension.BeforeRender(ref markdown, document);
            }
        }


        public void ProcessExtension(IRenderExtension extension,
            ref string html, string markdown,
            MarkdownDocument document)
        {          
            // append any custom headers to the top section of the document
            var headers = extension.RenderHeader(html,markdown,document);
            if (headers != null)
                document.AddExtraHeaders(headers);

            // update html content using the ref HTML parameter
            extension.InsertContent(ref html, markdown,document);
        }

        public void LoadDefaultExtensions()
        {
            Current.RenderExtensions.Add(new MermaidRenderExtension());
            Current.RenderExtensions.Add(new MathRenderExtension());
        }
    }
}
