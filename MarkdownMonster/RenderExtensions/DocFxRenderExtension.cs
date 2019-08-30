using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Westwind.Utilities;

namespace MarkdownMonster.RenderExtensions
{
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
            ParseDocFxIncludeFiles(args);
            ParseNoteTipWarningImportant(args);
        }


        private static Regex includeFileRegEx = new Regex(@"\[\!include.*?\[.*?\]\(.*?\)\]", RegexOptions.IgnoreCase);


        /// <summary>
        /// Parses DocFx include files in the format of:
        ///
        ///    [!include[title](relativePathToFileToInclude>)]
        ///
        /// Should run **prior** to Markdown parsing of the main document
        /// as it will embed the file content as is.
        /// </summary>
        /// <returns></returns>
        protected void ParseDocFxIncludeFiles(ModifyMarkdownArguments args)
        {
            var matches = includeFileRegEx.Matches(args.Markdown);

            foreach (Match match in matches)
            {
                string value = match.Value;

                //string title = StringUtils.ExtractString(value, "[!include[", "]");
                string file = StringUtils.ExtractString(value, "](", ")]");
                if (string.IsNullOrEmpty(file))
                    continue;

                if (file.StartsWith("~"))
                    file = mmApp.Model.Window.FolderBrowser.FolderPath + file.Substring(1);


                string filePath;
                if (file.Contains(@":\"))
                {
                    filePath = file;
                }
                else
                {
                    filePath = mmApp.Model.ActiveDocument?.Filename;
                    if (string.IsNullOrEmpty(filePath))
                        continue;

                    filePath = Path.GetDirectoryName(filePath);
                    if (filePath == null)
                        continue;
                }


                string includeFile = Path.Combine(filePath, file);
                includeFile = FileUtils.NormalizePath(includeFile);
                if (!File.Exists(includeFile))
                    continue;

                var markdownDocument = new MarkdownDocument();
                markdownDocument.Load(includeFile);
                string includeContent = markdownDocument.RenderHtml();

                args.Markdown = args.Markdown.Replace(value, includeContent);
            }
        }


        private static Regex TipNoteWarningImportantFileRegEx = new Regex(@">\s\[\![A-Z]*][\s\S]*?\n{2}", RegexOptions.Multiline);


        /// <summary>
        /// Handles rendering of Tip/Warning/Important/Caution/Note
        /// blocks.
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
                        if (line.TrimStart().StartsWith("> [!"))
                        {
                            string icon = "fa-info-circle";
                            var word = StringUtils.ExtractString(line, "> [!", "]");
                            switch (word)
                            {
                                case "NOTE":
                                case "TIP":
                                    icon = "fa-info-circle";
                                    break;
                                case "WARNING":
                                case "CAUTION":
                                case "IMPORTANT":
                                    icon = "fa-warning";
                                    break;
                            }

                            icon = $"<i class='fa {icon}'></i>&nbsp;";

                            sb.AppendLine("<div class=\"" + word + "\">");
                            sb.AppendLine($"<h5>{icon }{word}</h5>");
                            sb.AppendLine();
                        }
                        else
                            sb.AppendLine(line.Substring(2));
                    }
                    else
                        sb.AppendLine(line.Substring(2));
                }
                sb.AppendLine();
                sb.AppendLine("</div>");
                sb.AppendLine();

                args.Markdown = args.Markdown.Replace(value, sb.ToString());
            }
        }
    }
}
