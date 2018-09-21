using System;
using System.Collections.Generic;
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
            string htmlFile = Path.Combine(Environment.CurrentDirectory, "Editor\\htmltomarkdown.htm");

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
                    MessageBox.Show("Unable to convert Html to Markdown. Returning Html.","Html to Markdown Conversion Failed.",MessageBoxButton.OK,MessageBoxImage.Warning);
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

        public static string AddLinkReference(string md, SelectionRange selectionRange, string link)
        {
            const string STR_NEWIDPLACEHOLDER = "999999";

            if (string.IsNullOrEmpty(md))
                return null;

            var lines = new List<string>(StringUtils.GetLines(md));
            var activeLine = lines[selectionRange.StartRow];
            string selText = activeLine.Substring(selectionRange.StartColumn, selectionRange.EndColumn - selectionRange.StartColumn);
            activeLine = activeLine.Remove(selectionRange.StartColumn, selectionRange.EndColumn - selectionRange.StartColumn);

            // find next ref if any
            int nextId = 0;
            int prevId = 0;
            int newId = 0;
            for (int i = selectionRange.StartRow + 1; i == lines.Count; i++)
            {
                var line = lines[i];
                string id = StringUtils.ExtractString(line, "][", "]");
                if (id != null && id.Length < 5 && id[0].IsDigit())
                {
                    nextId = StringUtils.ParseInt(id, 0);                    
                    if (nextId == 0)
                        continue;
                    break;
                }                
            }

            // find previous id
            for (int i = selectionRange.StartRow - 1; i > 0; i--)
            {
                var line = lines[i];
                string id = StringUtils.ExtractString(line, "][", "]");
                if (id != null && id.Length > 0 &&  id.Length < 5 && id[0].IsDigit())
                {
                    prevId = StringUtils.ParseInt(id, 0);
                    if (prevId == 0)
                        continue;
                    break;
                }
            }

            if (nextId != 0)
                newId = nextId;
            else if (prevId != 0)
                newId = prevId + 1;
            else
                newId = 1;

            activeLine = activeLine.Insert(selectionRange.StartColumn, $"[{selText}][{STR_NEWIDPLACEHOLDER}]");
            lines[selectionRange.StartRow] = activeLine;

            
            string newLine = $"  [999999]: {link}";
            lines.Add(newLine);


            var updatedIds = new List<int>();

            List<LinkReferenceDefinition> links = new List<LinkReferenceDefinition>();
            var syntax = Markdig.Markdown.Parse(md);
            foreach (var item in syntax)
            {
                var line = item.Line;
                var content = lines[line];

                if (item is LinkReferenceDefinitionGroup)
                {
                    var itemLinks = item as LinkReferenceDefinitionGroup;

                    int adder = 0;
                    for (var index = 0; index < itemLinks.Count; index++)
                    {
                        var itemLink = (LinkReferenceDefinition) itemLinks[index];
                        if (newId == index  + 1)
                        {
                            links.Add(new LinkReferenceDefinition
                            {
                                Label = newId.ToString(),
                                Url = link
                            });
                            adder = 1;
                        }

                        itemLink.Label = (index + 1 + adder).ToString();
                        if (adder > 0)
                            updatedIds.Add(index + 1);                        

                        links.Add(itemLink);
                    }
                }
            }

            if (links.Count == 0)
            {
                links.Add(new LinkReferenceDefinition
                {
                    Label = newId.ToString(),
                    Url = link
                });
            }

            var sb = new StringBuilder();
            foreach (var linkItem in links)
            {
                sb.AppendLine($"  [{linkItem.Label}]: {linkItem.Url}");
            }
            string linkList = sb.ToString();


            sb.Clear();
            foreach (var line in lines)
            {
                if (line.StartsWith("  [") && line.Contains("]: "))
                {

                    continue;
                }

                sb.AppendLine(line);
            }

            md = sb.ToString();


            for (var index =updatedIds.Count-1; index > -1; index--)
            {
                var updatedId = updatedIds[index];
                md = md.Replace($"][{updatedId}]", $"][{updatedId + 1}]");
            }


            
            md = md.Replace(STR_NEWIDPLACEHOLDER, newId.ToString());
            md += linkList;

            return md;

            
        }

        #endregion
    }
}
