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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ControlzEx.Standard;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Syntax;
using Westwind.Utilities;
using Microsoft.DocAsCode.MarkdigEngine.Extensions;

namespace MarkdownMonster
{

    /// <summary>
    /// Wrapper around the CommonMark.NET parser that provides a cached
    /// instance of the Markdown parser. Hooks up custom processing.
    /// </summary>
    public class MarkdownParserDocFxMarkdig : MarkdownParserBase
    {
        protected MarkdownPipeline Pipeline;
        protected bool UsePragmaLines;

        

        public MarkdownParserDocFxMarkdig(bool usePragmaLines = false)
        {
            UsePragmaLines = usePragmaLines;
            //var builder = CreatePipelineBuilder();
            //Pipeline = builder.Build();            
        }

        /// <summary>
        /// Parses the actual markdown down to html
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>        
        public override string Parse(string markdown)
        {
            var options = mmApp.Configuration.MarkdownOptions;

            var builder = new MarkdownPipelineBuilder();

            var errors = Array.Empty<string>();
            var tokens = new Dictionary<string, string>();
            var files = new Dictionary<string, string>();

            var actualErrors = new List<string>();
            var actualDependencies = new HashSet<string>();

            var context = new MarkdownContext(
                getToken: key => tokens.TryGetValue(key, out var value) ? value : null,
                logInfo: (a, b, c, d) => { },
                logSuggestion: Log("suggestion"),
                logWarning: Log("warning"),
                logError: Log("error"),
                readFile: ReadFile);


            // Fake root path to what MM sees as the root path (.markdown, project file, project open etc.)
            string filePath = mmApp.Model.ActiveDocument?.Filename;
            if (string.IsNullOrEmpty(filePath) || filePath.Equals("untitled", StringComparison.OrdinalIgnoreCase))
            {
                filePath = mmApp.Model.ActiveDocument?.PreviewWebRootPath;
                if (string.IsNullOrEmpty(filePath))
                    filePath = "preview.md";
                else
                    filePath += "\\preview";
            }

            files.Add(filePath, markdown);


            builder = builder.UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
                .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
                .UseMediaLinks()
                .UsePipeTables()
                .UseAutoLinks()
                .UseHeadingIdRewriter()
                .UseIncludeFile(context)
                .UseCodeSnippet(context)
                .UseDFMCodeInfoPrefix()
                .UseQuoteSectionNote(context)
                .UseXref()
                .UseEmojiAndSmiley(false)
                .UseTabGroup(context)
                .UseMonikerRange(context)
                .UseInteractiveCode()
                .UseRow(context)
                .UseNestedColumn(context)
                .UseTripleColon(context)
                .UseNoloc();



            builder = RemoveUnusedExtensions(builder);
            builder = builder
                .UseYamlFrontMatter()
                .UseLineNumber();

            if (options.NoHtml)
                builder = builder.DisableHtml();

            if (UsePragmaLines)
                builder = builder.UsePragmaLines();

            var pipeline = builder.Build();

            string html;

            using (InclusionContext.PushFile(filePath))
            {
                html = Markdown.ToHtml(markdown, pipeline);
            }

            html = ParseFontAwesomeIcons(html);


            if (mmApp.Configuration.MarkdownOptions.RenderLinksAsExternal)
                html = ParseExternalLinks(html);

            if (!mmApp.Configuration.MarkdownOptions.AllowRenderScriptTags)
                html = HtmlUtils.SanitizeHtml(html);


            return html;


            MarkdownContext.LogActionDelegate Log(string level)
            {
                return (code, message, origin, line) => actualErrors.Add(code);
            }

            
            // Handler to fix up file paths for nested/included documents
            (string content, object file) ReadFile(string path,  MarkdownObject origin)
            {

                string key;
                
                var rootPath = mmApp.Model.ActiveDocument?.PreviewWebRootPath;
                if (rootPath == null)
                    rootPath = Path.GetDirectoryName(files.FirstOrDefault().Key);


                string parentDocPath = null; //relativeTo as string;   NOT PROVIDEd BY DOCFX ANYMORE???
                //if (!string.IsNullOrEmpty(parentDocPath))
                //    parentDocPath = Path.GetDirectoryName(parentDocPath);


                //fully qualified path
                if (path.Contains(":/") || path.Contains(":\\"))
                    key = path;
                else if (!string.IsNullOrEmpty(rootPath) && path.StartsWith("~/"))
                {
                    path = path.Substring(2);
                    key = Path.Combine(rootPath, path).Replace('\\', '/');
                }
                // Site relative  path
                else if (!string.IsNullOrEmpty(rootPath) && path.StartsWith("/"))
                {
                    path = path.Substring(1);
                    key = Path.Combine(rootPath, path).Replace('\\', '/');
                }
                // Site relative path
                else if (!string.IsNullOrEmpty(parentDocPath))
                    key = Path.GetFullPath(Path.Combine(parentDocPath, path));
                else
                    key = path;

                actualDependencies.Add(key);

                files.TryGetValue(key, out var value);
                if (value == null)
                {
                    try
                    {
                        value = File.ReadAllText(key)?.Trim();
                    }
                    catch
                    {
                    }
                }

                if (value == null)
                    return (null, null);

                return (value, key as object);
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
            return builder;
        }

        private static MarkdownPipelineBuilder RemoveUnusedExtensions(MarkdownPipelineBuilder pipeline)
        {
            pipeline.Extensions.RemoveAll(extension => extension is CustomContainerExtension);
            return pipeline;
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
