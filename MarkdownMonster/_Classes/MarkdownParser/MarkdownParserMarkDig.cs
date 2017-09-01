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
using System.IO;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Renderers;
using Westwind.Utilities;
using MarkdownMonster.ParserExtensions;

namespace MarkdownMonster
{

    /// <summary>
    /// Wrapper around the CommonMark.NET parser that provides a cached
    /// instance of the Markdown parser. Hooks up custom processing.
    /// </summary>
    public class  MarkdownParserMarkdig : MarkdownParserBase
    {
        public static MarkdownPipeline Pipeline;

        private readonly bool _usePragmaLines;

        public MarkdownParserMarkdig(bool usePragmaLines = false, bool force = false)
        {
            _usePragmaLines = usePragmaLines;
            if (force || Pipeline == null)
            {
                var builder = CreatePipelineBuilder();
                Pipeline = builder.Build();
            }
        }

        /// <summary>
        /// Parses the actual markdown down to html
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>        
        public override string Parse(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return string.Empty;

            var htmlWriter = new StringWriter();
            var renderer = CreateRenderer(htmlWriter);

			Markdown.Convert(markdown, renderer, Pipeline);
            var html = htmlWriter.ToString();
            
            html = ParseFontAwesomeIcons(html);

            if (mmApp.Configuration.MarkdownOptions.RenderLinksAsExternal)
                html = ParseExternalLinks(html);

            if (!mmApp.Configuration.MarkdownOptions.AllowRenderScriptTags)
                html = ParseScript(html);  
                      
            return html;
        }

        protected virtual MarkdownPipelineBuilder CreatePipelineBuilder()
        {
            var builder = new MarkdownPipelineBuilder()
                .UseEmphasisExtras()
                .UsePipeTables()
                .UseGridTables()
                .UseFooters()
                .UseFootnotes()
                .UseCitations();


            var options = mmApp.Configuration.MarkdownOptions;
            if (options.AutoLinks)
                builder = builder.UseAutoLinks();
            if (options.AutoHeaderIdentifiers)
                builder = builder.UseAutoIdentifiers();
            if (options.Abbreviations)
                builder = builder.UseAbbreviations();

			if (options.StripYamlFrontMatter)
				builder = builder.UsePandocYamlFrontMatter();
            if (options.EmojiAndSmiley)
                builder = builder.UseEmojiAndSmiley();
            if (options.MediaLinks)
                builder = builder.UseMediaLinks();
            if (options.ListExtras)
                builder = builder.UseListExtras();
            if (options.Figures)
                builder = builder.UseFigures();
            if (options.GithubTaskLists)
                builder = builder.UseTaskLists();
            if (options.SmartyPants)
                builder = builder.UseSmartyPants();            

            if (_usePragmaLines)
                builder = builder.UsePragmaLines();

            return builder;
        }

        protected virtual IMarkdownRenderer CreateRenderer(TextWriter writer)
        {
            return new HtmlRenderer(writer);
        }
    }
}
