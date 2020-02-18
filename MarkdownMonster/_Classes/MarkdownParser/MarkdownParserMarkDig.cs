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
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Westwind.Utilities;

namespace MarkdownMonster
{

    /// <summary>
    /// Wrapper around the CommonMark.NET parser that provides a cached
    /// instance of the Markdown parser. Hooks up custom processing.
    /// </summary>
    public class MarkdownParserMarkdig : MarkdownParserBase
    {
        protected MarkdownPipeline Pipeline;
        protected bool UsePragmaLines;

        public MarkdownParserMarkdig(bool usePragmaLines = false)
        {
            UsePragmaLines = usePragmaLines;
            var builder = CreatePipelineBuilder();
            Pipeline = builder.Build();            
        }

        /// <summary>
        /// Parses the actual markdown down to html
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>        
        public override string Parse(string markdown)
        {
            try
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
                    html = HtmlUtils.SanitizeHtml(html);

                return html;
            }
            catch (Exception ex)
            {
                if (markdown.Length > 10000)
                    markdown = markdown.Substring(0, 10000);

                mmApp.Log("Unable to render Markdown Document\n" + markdown, ex, logLevel: LogLevels.Warning);
                var html = $@"
<h1><i class='fa fa-warning text-error'></i> Unable to render Markdown Document</h1>

<p>
   An error occurred trying to parse the Markdown document to HTML.
</p>

<h4>Error</h4>
<b>{ex.Message}</b>

<p>
    <a id='hrefShow' href='#0' style='font-size: 0.8em; font-weight: normal'>more info...</a>
</p>

<div id='detail' style='display:none'>
<h3>Error Detail</h3>

<pre><code class='hljs language-text'>{System.Net.WebUtility.HtmlEncode(StringUtils.NormalizeIndentation(ex.StackTrace))}</code></pre>
</div>

<script>
$('#hrefShow').click(function () {{ $('#detail').show(); }});
</script>
";
                return html;
            }
        }

        /// <summary>
        /// Builds the Markdig processing pipeline and returns a builder.
        /// Use this method to override any custom pipeline addins you want to
        /// add or append. 
        /// 
        /// Note you can also add addins using options.MarkdigExtensions which
        /// use MarkDigs extension syntax using commas instead of +.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected virtual MarkdownPipelineBuilder BuildPipeline(MarkdownOptionsConfiguration options, MarkdownPipelineBuilder builder)
        {

            builder = builder.UseEmphasisExtras();

            if(options.UseMathematics)
                builder = builder.UseMathematics();                

            if (options.AutoLinks)
                builder = builder.UseAutoLinks();
            if (options.AutoHeaderIdentifiers)
                builder = builder.UseAutoIdentifiers(Markdig.Extensions.AutoIdentifiers.AutoIdentifierOptions.GitHub);
            if (options.Abbreviations)
                builder = builder.UseAbbreviations();

            if (options.UseTables)            
                builder = builder
                    .UsePipeTables()
                    .UseGridTables();
            
            if (options.StripYamlFrontMatter)
                builder = builder.UseYamlFrontMatter();
            if (options.EmojiAndSmiley)
                builder = builder.UseEmojiAndSmiley(true);
            if (options.MediaLinks)
                builder = builder.UseMediaLinks();
            if (options.ListExtras)
                builder = builder
                            .UseListExtras()
                            .UseDefinitionLists();

            if (options.Figures)
                builder = builder.UseFigures();
            if (options.GithubTaskLists)
                builder = builder.UseTaskLists();
            if (options.SmartyPants)
                builder = builder.UseSmartyPants();


            
            if (options.CustomContainers)
                builder = builder.UseCustomContainers();

            if (options.GenericAttributes)
                builder = builder.UseGenericAttributes();
            
            if (options.FootersAndFootnotes)
                builder = builder                    
                    .UseFooters()
                    .UseFootnotes();
            if(options.NoHtml)
                builder = builder.DisableHtml();

                
            if (UsePragmaLines)
                builder = builder.UsePragmaLines();
            
            try
            {
                if (!string.IsNullOrWhiteSpace(options.MarkdigExtensions))
                {
                    builder = builder.Configure(options.MarkdigExtensions.Replace(",", "+"));
                }
            }
            catch (ArgumentException ex)
            {
                // One or more of the extension options is invalid. 
                mmApp.Log("Failed to load Markdig extensions: " + options.MarkdigExtensions + "\r\n" + ex.Message, ex);
                mmApp.Model.Window.ShowStatusError("Failed to load Markdig extensions: " + ex.Message);
                // reset to default
                options.MarkdigExtensions = string.Empty;
                builder = builder.Configure(options.MarkdigExtensions.Replace(",", "+"));                
            }

            return builder;
        }


        /// <summary>
        /// Create the entire Markdig pipeline and return the completed
        /// ready to process builder.
        /// </summary>
        /// <returns></returns>
        public  virtual MarkdownPipelineBuilder CreatePipelineBuilder()
        {
            var options = mmApp.Configuration.MarkdownOptions;
            var builder = new MarkdownPipelineBuilder();

            try
            {
                builder = BuildPipeline(options, builder);
            }
            catch (ArgumentException ex)
            {
                mmApp.Log($"Failed to build pipeline: {ex.Message}", ex);
            }

            return builder;
        }

        protected virtual IMarkdownRenderer CreateRenderer(TextWriter writer)
        {
            return new HtmlRenderer(writer);
        }
    }

}
