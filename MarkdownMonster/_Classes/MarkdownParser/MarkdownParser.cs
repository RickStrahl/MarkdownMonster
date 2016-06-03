#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 04/28/2016
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonMark;
using Westwind.Utilities;

namespace MarkdownMonster
{

    /// <summary>
    /// Wrapper around the CommonMark.NET parser that provides a cached
    /// instance of the Markdown parser. Hooks up custom processing.
    /// </summary>
    public class MarkdownParser
    {
        /// <summary>
        /// Retrieves a cached instance of the markdown parser
        /// </summary>
        /// <param name="markdownStyle"></param>
        /// <param name="RenderLinksAsExternal"></param>
        /// <returns></returns>
        public static MarkdownParser GetParser(MarkdownStyles markdownStyle, bool RenderLinksAsExternal = false)
        {
            if (RenderLinksAsExternal)
            {
                CommonMarkSettings.Default.OutputDelegate =
                    (doc, output, settings) => new ExternalLinkFormatter(output, settings).WriteDocument(doc);
            }

            if (markdownStyle == MarkdownStyles.GitHub)
                return new GithubMarkdownParser();
            
            return new MarkdownParser();
        }

        /// <summary>
        /// Parses the actual markdown down to html
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public string Parse(string markdown)
        {
            var html = CommonMark.CommonMarkConverter.Convert(markdown);
            html = ParseFontAwesomeIcons( html);
            return html;
        }

        /// <summary>
        /// Post processing routine that post-processes the HTML and 
        /// replaces @icon- with fontawesome icons
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        protected string ParseFontAwesomeIcons(string html)
        {
            while (true)
            {
                string iconBlock = StringUtils.ExtractString(html, "@icon-", " ", false, false, true);
                if (string.IsNullOrEmpty(iconBlock))
                    break;

                string icon = iconBlock.Replace("@icon-", "").Trim();
                html = html.Replace(iconBlock, "<i class=\"fa fa-" + icon + "\"></i> ");                
            }
            return html;
        }
    }

    public class GithubMarkdownParser : MarkdownParser
    {        
    }
}
