using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Markdig.Helpers;
using Markdig.Syntax;
using MarkdownMonster.Windows;
using MarkdownMonster.Windows.DocumentOutlineSidebar;
using Newtonsoft.Json.Linq;
using Westwind.Utilities;

namespace MarkdownMonster
{
    /// <summary>
    /// Contains a few static utilities for Markdown handling
    /// </summary>
    public class MarkdownUtilities
    {


        /// <summary>
        /// Converts an HTML string to Markdown.
        /// </summary>
        /// <remarks>
        /// This routine relies on a browser window
        /// and an HTML file that handles the actual
        /// parsing: Editor\HtmlToMarkdown.htm
        /// </remarks>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HtmlToMarkdown(string html, bool noErrorUI = false)
        {
            if (string.IsNullOrEmpty(html))
                return "";
#if false
            var config = new ReverseMarkdown.Config(githubFlavored: true);
            var converter = new ReverseMarkdown.Converter(config);
            string markdown = converter.Convert(html);
            return markdown ?? html;
#else
            // Old code that uses JavaScript in a WebBrowser Control
            string markdown = null;
            string htmlFile = Path.Combine(App.InitialStartDirectory, "Editor\\htmltomarkdown.htm");

            var form = new BrowserDialog();
            form.ShowInTaskbar = false;
            form.Width = 1;
            form.Height = 1;
            form.Left = -10000;
            form.Show();

            bool exists = File.Exists(htmlFile);
            form.NavigateAndWaitForCompletion(htmlFile);

            WindowUtilities.DoEvents();

            try
            {
                dynamic doc = form.Browser.Document;
                markdown = doc.ParentWindow.htmltomarkdown(html);
            }
            catch (Exception ex)
            {
                mmApp.Log("Failed to convert Html to Markdown", ex);
                if (!noErrorUI)
                    MessageBox.Show("Unable to convert Html to Markdown. Returning Html.", "Html to Markdown Conversion Failed.", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            form.Close();
            form = null;

            return markdown ?? html;
#endif
        }


        #region Front Matter Parsing

        public static readonly Regex YamlExtractionRegex = new Regex(@"\A---[ \t]*\r?\n[\s\S]+?\r?\n(---|\.\.\.)[ \t]*\r?\n", RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// Strips
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public static string StripFrontMatter(string markdown)
        {
            string extractedYaml = null;
            var match = YamlExtractionRegex.Match(markdown);
            if (match.Success)
                extractedYaml = match.Value;

            if (!string.IsNullOrEmpty(extractedYaml))
                markdown = markdown.Replace(extractedYaml, "");

            return markdown;
        }

        /// <summary>
        /// Returns Front Matter Yaml block content as a string
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public static string ExtractFrontMatter(string markdown)
        {
            string extractedYaml = null;
            var match = YamlExtractionRegex.Match(markdown);
            if (match.Success)
                extractedYaml = match.Value;

            return extractedYaml;
        }

        #endregion


        #region Parsing

        static Regex RegexLinkReferences = new Regex(@"\[.*?\]\[(\d{0,10})\]", RegexOptions.Multiline);


        /// <summary>
        /// Adds a Link Reference into the Markdown document based on a selected region in the document
        /// </summary>
        /// <param name="md"></param>
        /// <param name="selectionRange"></param>
        /// <param name="link"></param>
        /// <returns></returns>
        public static LinkReferenceResult AddLinkReference(string md, SelectionRange selectionRange, string link)
        {
            const string STR_NEWID_PLACEHOLDER = "999999";

            if (string.IsNullOrEmpty(md))
                return null;

            var result = new LinkReferenceResult();

            List<LinkReferenceDefinition> links = new List<LinkReferenceDefinition>();
            var syntax = Markdig.Markdown.Parse(md);
            foreach (var item in syntax)
            {

                if (item is LinkReferenceDefinitionGroup)
                {
                    var itemLinks = item as LinkReferenceDefinitionGroup;

                    for (var index = 0; index < itemLinks.Count; index++)
                    {
                        var itemLink = (LinkReferenceDefinition)itemLinks[index];
                        links.Add(itemLink);
                    }
                }
            }

            var lines = StringUtils.GetLines(md);
            var activeLine = lines[selectionRange.StartRow];
            string selText = activeLine.Substring(selectionRange.StartColumn, selectionRange.EndColumn - selectionRange.StartColumn);
            activeLine = activeLine.Remove(selectionRange.StartColumn, selectionRange.EndColumn - selectionRange.StartColumn);
            activeLine = activeLine.Insert(selectionRange.StartColumn, $"[{selText}][{STR_NEWID_PLACEHOLDER}]");
            lines[selectionRange.StartRow] = activeLine;

            md = string.Join("\r\n", lines);

            var sbLinkList = new StringBuilder();

            var matches = RegexLinkReferences.Matches(md);
            for (var index = 0; index < matches.Count; index++)
            {
                Match match = matches[index];
                var value = match.Value;
                var match2 = Regex.Match(value, @"\]\[(\d{0,10})\]");

                if (match2?.Value == null || match.Groups.Count < 2)
                    continue;

                var id = match.Groups[1].Value;

                string newValue = value.Replace(match2.Value, "][" + (index + 1) + "]");
                if (newValue != value)
                    md = md.Replace(value, newValue);

                string newLink = null;
                var existingLink = links.FirstOrDefault(l => l.Label == id);
                if (existingLink == null)
                {
                    newLink = link;
                    result.SelectionLength = newValue.Length;
                }
                else
                    newLink = existingLink.Url;


                sbLinkList.AppendLine($"  [{index + 1}]: {newLink}");
            }

            lines = StringUtils.GetLines(md);

            StringBuilder sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (line.StartsWith("  [") && line.Contains("]: "))
                {
                    continue;
                }

                sb.AppendLine(line);
            }



            result.Markdown = sb.ToString().TrimEnd();

            result.Markdown += "\r\n\r\n" + sbLinkList;

            return result;
        }

        #endregion
    }

    public class LinkReferenceResult
    {
        public string Markdown;
        public int SelectionLength = 0;
    }

    public class MarkdownLintError
    {
        public int LineNumber { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public string RuleName { get; set; }
        public int ErrorRangeStart { get; set; }
        public int ErrorRangeEnd { get; set; }

        public override string ToString()
        {
            return $"Line {LineNumber}: {Description}. {(!string.IsNullOrEmpty(Detail) ? " " + Detail + ". " : "")}({RuleName})";
        }
    }
}
