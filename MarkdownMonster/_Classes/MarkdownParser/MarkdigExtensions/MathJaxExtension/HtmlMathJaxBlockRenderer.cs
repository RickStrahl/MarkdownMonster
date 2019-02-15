// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using System.Linq;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Mathematics
{
    /// <summary>
    /// A HTML renderer for a <see cref="MathBlock"/>.
    /// </summary>
    /// <seealso cref="Markdig.Renderers.Html.HtmlObjectRenderer{T}" />
    public class HtmlMathJaxBlockRenderer : HtmlObjectRenderer<MathBlock>
    {
        protected override void Write(HtmlRenderer renderer, MathBlock obj)
        {
            bool addBegin = false;
            var firstLine = obj.Lines.ToString();
            if (!firstLine.TrimStart().StartsWith("\\begin"))
                addBegin = true;

            renderer.EnsureLine();
            renderer.Write("<div").WriteAttributes(obj).Write(">");

            if (addBegin)
                renderer.WriteLine("\\begin{equation}");
            renderer.WriteLeafRawLines(obj, true, true);
            if(addBegin)
                renderer.WriteLine("\n\\end{equation}");

            renderer.WriteLine("</div>");
        }
    }
}
