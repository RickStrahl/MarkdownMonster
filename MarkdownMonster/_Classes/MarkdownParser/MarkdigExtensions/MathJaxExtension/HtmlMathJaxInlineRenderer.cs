// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Mathematics
{
    /// <summary>
    /// A HTML renderer for a <see cref="MathInline"/>.
    /// </summary>
    /// <seealso cref="Markdig.Renderers.Html.HtmlObjectRenderer{Figure}" />
    public class HtmlMathJaxInlineRenderer : HtmlObjectRenderer<MathInline>
    {
        protected override void Write(HtmlRenderer renderer, MathInline obj)
        {

            bool addBegin = false;
            var firstLine = obj.Content.Text;
            if (!firstLine.TrimStart().StartsWith("\\begin{"))
                addBegin = true;

            renderer.Write("<span").WriteAttributes(obj).Write(">");

            if (addBegin)
                renderer.WriteLine("\\begin{equation}");

            renderer.WriteEscape(ref obj.Content);

            if (addBegin)
                renderer.Write("\n\\end{equation}");

            renderer.Write("</span>");
        }
    }
}
