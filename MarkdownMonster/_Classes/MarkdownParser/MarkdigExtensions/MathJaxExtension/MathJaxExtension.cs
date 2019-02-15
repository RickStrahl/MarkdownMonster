// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using Markdig.Extensions.Mathematics;
using Markdig.Renderers;

namespace Markdig.Extensions.Mathematics
{
    /// <summary>
    /// Extension for adding inline mathematics $...$
    /// </summary>
    /// <seealso cref="Markdig.IMarkdownExtension" />
    public class MathJaxExtension : MathExtension, IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            // Adds the inline parser
            if (!pipeline.InlineParsers.Contains<MathInlineParser>())
            {
                pipeline.InlineParsers.Insert(0, new MathInlineParser());
            }

            // Adds the block parser
            if (!pipeline.BlockParsers.Contains<MathBlockParser>())
            {
                // Insert before EmphasisInlineParser to take precedence
                pipeline.BlockParsers.Insert(0, new MathBlockParser());
            }
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            var htmlRenderer = renderer as HtmlRenderer;
            if (htmlRenderer != null)
            {
                if (!htmlRenderer.ObjectRenderers.Contains<HtmlMathJaxInlineRenderer>())
                {
                    htmlRenderer.ObjectRenderers.Insert(0, new HtmlMathJaxInlineRenderer());
                }
                if (!htmlRenderer.ObjectRenderers.Contains<HtmlMathJaxBlockRenderer>())
                {
                    htmlRenderer.ObjectRenderers.Insert(0, new HtmlMathJaxBlockRenderer());
                }
            }
        }
    }    
}

namespace Markdig
{
    public static class MarkdigMathJaxExtensionMethods
    {
        /// <summary>
        /// Uses the MathJax extension. Handles converting inline $$ and $
        /// blocks into html blocks that render without Markdown expansion
        /// so they can render properly.
        ///
        /// Note: Works in combination with the MathRenderExtension that is
        /// responsible for injecting the MathJax JavaScript from CDN.
        /// 
        /// https://www.mathjax.org
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <returns>The modified pipeline</returns>
        public static MarkdownPipelineBuilder UseMathJax(this MarkdownPipelineBuilder pipeline)
        {
            pipeline.Extensions.AddIfNotAlready<MathJaxExtension>();
            return pipeline;
        }
    }
}



