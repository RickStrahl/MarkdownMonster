using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

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


        /// <summary>
        /// Process all BeforeRenderMarkdown Extensions
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="document"></param>
        public void ProcessAllBeforeMarkdownRenderedHooks(ModifyMarkdownArguments args)
        {
            foreach (var extension in RenderExtensions)
            {
                try
                {
                    extension.BeforeMarkdownRendered(args);
                }
                catch (Exception ex)
                {
                    mmApp.Log($"BeforeMarkdownRendered RenderExtension failed: {extension.GetType().Name}", ex);
                }
            }
        }


        /// <summary>
        /// Processed after Markdown has been rendered into HTML, but not been
        /// merged into the template.
        ///
        /// You can modify the HTML and also add headers to be rendered into the HEAD
        /// of the template here.
        /// </summary>
        /// <param name="args"></param>
        public void ProcessAllAfterMarkdownRenderedHooks(ModifyHtmlAndHeadersArguments args)
        {
            foreach (var extension in RenderExtensions)
            {
                args.HeadersToEmbed = null;

                // update html content using the ref HTML parameter
                try
                {
                    extension.AfterMarkdownRendered(args);
                }
                catch (Exception ex)
                {
                    mmApp.Log($"AfterMarkdownRendered RenderExtension failed: {extension.GetType().Name}", ex);
                }

                if (args.HeadersToEmbed != null)
                {
                    args.MarkdownDocument.AddExtraHeaders(args.HeadersToEmbed);
                }
            }
        }

        public void ProcessAllAfterDocumentRenderedHooks(ModifyHtmlArguments args)
        {
            foreach (var extension in RenderExtensions)
            {
                try
                {
                    extension.AfterDocumentRendered(args);
                }
                catch (Exception ex)
                {
                    mmApp.Log($"AfterDocumentRendered RenderExtension failed: {extension.GetType().Name}", ex);
                }
            }
        }


        public void LoadDefaultExtensions()
        {
            if(mmApp.Configuration.MarkdownOptions.MermaidDiagrams)
                Current.RenderExtensions.Add(new MermaidRenderExtension());

            if (mmApp.Configuration.MarkdownOptions.UseMathematics)
                Current.RenderExtensions.Add(new MathRenderExtension());

            if (mmApp.Configuration.MarkdownOptions.ParseDocFx)
                Current.RenderExtensions.Add(new DocFxRenderExtension());
        }

    }
}
