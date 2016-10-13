#if false
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
using System.Text;
using System.Threading.Tasks;
using CommonMark;

namespace MarkdownMonster
{
    /// <summary>
    /// Wrapper around the CommonMark.NET parser that provides a cached
    /// instance of the Markdown parser. Hooks up custom processing.
    /// </summary>
    public class  MarkdownParserCommonMarkNet : MarkdownParserBase
    {        
        /// <summary>
        /// Parses the actual markdown down to html
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public override string Parse(string markdown)
        {

            if (string.IsNullOrEmpty(markdown))
                return string.Empty;

            var html = CommonMarkConverter.Convert(markdown);
            html = ParseFontAwesomeIcons(html);
            html = ParseStrikeout(html);
            return html;
        }


    }

}
#endif