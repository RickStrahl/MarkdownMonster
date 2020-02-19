using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Westwind.Utilities;

namespace MarkdownMonster.RenderExtensions
{
    /// <summary>
    /// Renders a handful of DocFx Markdown extensions in the previewer or for output
    /// generation.
    /// </summary>
    public class DocFxRenderExtension : IRenderExtension
    {
        public void AfterDocumentRendered(ModifyHtmlArguments args)
        {

        }

        public void AfterMarkdownRendered(ModifyHtmlAndHeadersArguments args)
        {
        }

        public void BeforeMarkdownRendered(ModifyMarkdownArguments args)
        {
            if(mmApp.Configuration.MarkdownOptions.MarkdownParserName.Contains("DocFx"))
                return;

            ParseDocFxIncludeFiles(args);
            ParseNoteTipWarningImportant(args);
            ParseXrefTags(args);
        }


        private static Regex includeFileRegEx = new Regex(@"\[\![include|INCLUDE|code-|CODE-].*?\[.*?\]\(.*?\)\]", RegexOptions.IgnoreCase);
        private Stack<string> nestedDocs = new Stack<string>();

        /// <summary>
        /// Parses DocFx include files in the format of:
        ///
        ///    [!include[title](relativePathToFileToInclude>)]
        ///
        /// Should run **prior** to Markdown parsing of the main document
        /// as it will embed the file content as is.
        /// </summary>
        /// <returns></returns>
        public void ParseDocFxIncludeFiles(ModifyMarkdownArguments args)
        {
            var matches = includeFileRegEx.Matches(args.Markdown);

            foreach (Match match in matches)
            {
                string value = match.Value;

                //string title = StringUtils.ExtractString(value, "[!include[", "]");
                string file = StringUtils.ExtractString(value, "](", ")]");
                if (string.IsNullOrEmpty(file))
                    continue;

                bool isCode = value.StartsWith("[!code-");
                string syntax = isCode ? StringUtils.ExtractString(value, "[!code-", "[]") : null;

                string filePath;
                bool hasPreviewWebPath = !string.IsNullOrEmpty(args.MarkdownDocument.PreviewWebRootPath);
                // Fix up paths
                 if (hasPreviewWebPath && file.StartsWith("~/"))
                        file = Path.Combine(args.MarkdownDocument.PreviewWebRootPath, file.Substring(2));
                else if (hasPreviewWebPath && file.StartsWith("~") || file.StartsWith("/"))
                        file = Path.Combine(args.MarkdownDocument.PreviewWebRootPath, file.Substring(1));
                if (file.Contains(@":\") || file.Contains(":/"))
                {
                    filePath = file;
                }
                else
                {
                    var lastDoc = nestedDocs.LastOrDefault();
                    if (string.IsNullOrEmpty(lastDoc))
                    {
                        filePath = mmApp.Model.ActiveDocument?.Filename;
                        filePath = Path.GetDirectoryName(filePath);
                    }
                    else
                        filePath = Path.GetDirectoryName(lastDoc);
                    
                    if (string.IsNullOrEmpty(filePath))
                        continue;
                }


                string includeFile = Path.Combine(filePath, file);
                includeFile = FileUtils.NormalizePath(includeFile);
                if (!File.Exists(includeFile))
                {
                    // escape the embedded link ([] so it doesn't expand
                    args.Markdown = args.Markdown.Replace("[]", "&#91;&#93;");
                    continue;
                }

                var includedContent = File.ReadAllText(includeFile);
                if (isCode)
                    includedContent =  $"```{syntax}\n{includedContent.Trim()}\n```";

                nestedDocs.Push(includeFile);

                // We need to process nested content
                var markdownDocument = new MarkdownDocument();
                markdownDocument.PreviewWebRootPath = args.MarkdownDocument.PreviewWebRootPath;
                markdownDocument.CurrentText = includedContent;
                markdownDocument.OnBeforeDocumentRendered(ref includedContent); 
                
                args.Markdown = args.Markdown.Replace(value, includedContent);

                nestedDocs.Pop();
            }
        }



        /*
        Note/Tip/Warning etc. processing

        > [!NOTE]
        > note content
        > [!WARNING]
        > warning content

        */
        private static Regex TipNoteWarningImportantFileRegEx = new Regex(@">\s\[\![A-Z]*][\s\S]*?(\n{2}|\Z)", RegexOptions.Multiline);


        /// <summary>
        /// Handles rendering of Tip/Warning/Important/Caution/Note
        /// blocks.
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Note: Markdown is expected to be in LineFeed only mode for line breaks (StringUtils.NormalizeLinefeeds()).
        /// If you have CR/LF the value needs to be fixed up.  RenderExtensions automatically fix up inbound Markdown
        /// to normalized linefeeds for rendering, but if you test locally make sure to pre-format the args.Markdown
        /// </remarks>
        public void ParseNoteTipWarningImportant(ModifyMarkdownArguments args)
        {
            var matches = TipNoteWarningImportantFileRegEx.Matches(args.Markdown);
            foreach(Match match in matches)
            {
                string value = match.Value.Trim();

                var lines = StringUtils.GetLines(value);

                var sb = new StringBuilder();
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (i == 0)
                    {
                        // note header
                        if (line.TrimStart().StartsWith("> [!"))
                        {
                            var word = StringUtils.ExtractString(line, "> [!", "]");

                            sb.AppendLine("<div class=\"" + word + "\">");
                            sb.AppendLine($"<h5>{word}</h5>");
                            sb.AppendLine();
                        }
                        else
                            // content line - could be empty
                            sb.AppendLine(line.TrimStart(' ', '>'));
                    }
                    else
                        // content line - could be empty
                        sb.AppendLine(line.TrimStart(' ', '>'));
                }
                sb.AppendLine();
                sb.AppendLine("</div>");
                sb.AppendLine();

                args.Markdown = args.Markdown.Replace(value, sb.ToString());
            }
        }

        private static Regex XRefRegEx = new Regex(@"<xref:.*?(/>|>)");

        public void ParseXrefTags(ModifyMarkdownArguments args)
        {
            var matches = XRefRegEx.Matches(args.Markdown);
            foreach (Match match in matches)
            {
                string value = match.Value.Trim();

                var link = StringUtils.ExtractString(value, "<xref:", ">")?.TrimEnd('/');
                if (link == null)
                    return;
                string title = null;


                var filePath = FixUpRootPath(args, link);
                if (File.Exists(filePath))
                {
                    string fileContent = File.ReadAllText(filePath);
                    title = StringUtils.GetLines(fileContent)
                        .FirstOrDefault(l => l.StartsWith("# ") || l.StartsWith("## ") || l.StartsWith("### "));
                }
                else if (File.Exists(Path.ChangeExtension(filePath, "md")))
                {
                    string fileContent = File.ReadAllText(Path.ChangeExtension(filePath,"md"));
                    title = StringUtils.GetLines(fileContent)
                        .FirstOrDefault(l => l.StartsWith("# ") || l.StartsWith("## ") || l.StartsWith("### "));
                }

                if (string.IsNullOrEmpty(title))
                    title = link;
                else
                    title = title.TrimStart('#',' ','\t');

                var html = $"<a href=\"{link}\">{WebUtility.HtmlEncode(title)}</a>";

                args.Markdown = args.Markdown.Replace(value, html);
            }
        }


        public string FixUpRootPath(ModifyMarkdownArguments args,string link)
        {
            if(string.IsNullOrEmpty(link))
                return link;

            var filePath = link;
            if (filePath.StartsWith("~/") || filePath.StartsWith("/"))
            {
                filePath = StringUtils.ReplaceStringInstance(filePath, "~/", "",1,false);
                if (filePath.StartsWith("/"))
                    filePath = filePath.Substring(1);

                filePath = args.MarkdownDocument.PreviewWebRootPath + "\\" + filePath;
            }
            else
            {
                var path = Path.GetDirectoryName(args.MarkdownDocument.Filename);
                if (!string.IsNullOrEmpty(path))
                    filePath = Path.Combine(path, filePath);
                else 
                    filePath = args.MarkdownDocument.PreviewWebRootPath + "\\" + filePath;
            }


            filePath = FileUtils.NormalizePath(filePath);

            return filePath;
        }
    }
}
