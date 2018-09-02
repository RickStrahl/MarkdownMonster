using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using MarkdownMonster.Windows;
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

        #region HtmlSanitation
        static string HtmlSanitizeTagBlackList { get; } = "script|iframe|object|embed|form";

        static Regex _RegExScript = new Regex(
            $@"(<({HtmlSanitizeTagBlackList})\b[^<]*(?:(?!<\/({HtmlSanitizeTagBlackList}))<[^<]*)*<\/({HtmlSanitizeTagBlackList})>)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        static Regex _RegExJavaScriptHref = new Regex(
            @"<.*?(href|src|dynsrc|lowsrc)=.{0,10}(javascript:).*?>",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        static Regex _RegExOnEventAttributes = new Regex(
            @"<.*?\s(on.{4,12}=([""].*?[""]|['].*?['])).*?(>|\/>)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// Sanitizes HTML to some of the most of 
        /// </summary>
        /// <remarks>
        /// This provides rudimentary HTML sanitation catching the most obvious
        /// XSS script attack vectors. For mroe complete HTML Sanitation please look into
        /// a dedicated HTML Sanitizer.
        /// </remarks>
        /// <param name="html">input html</param>
        /// <param name="htmlTagBlacklist">A list of HTML tags that are stripped.</param>
        /// <returns>Sanitized HTML</returns>
        public static string SanitizeHtml(string html, string htmlTagBlacklist = "script|iframe|object|embed|form")
        {
            if (string.IsNullOrEmpty(html))
                return html;

            if (!string.IsNullOrEmpty(htmlTagBlacklist) || htmlTagBlacklist == HtmlSanitizeTagBlackList)
            {
                // Replace Script tags - reused expr is more efficient
                html = _RegExScript.Replace(html, string.Empty);
            }
            else
            {
                html = Regex.Replace(html,
                                      $@"(<({htmlTagBlacklist})\b[^<]*(?:(?!<\/({HtmlSanitizeTagBlackList}))<[^<]*)*<\/({htmlTagBlacklist})>)",
                                      "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }

            // Remove javascript: directives
            var matches = _RegExJavaScriptHref.Matches(html);
            foreach (Match match in matches)
            {
                var txt = StringUtils.ReplaceString(match.Value, "javascript:", "unsupported:", true);
                html = html.Replace(match.Value, txt);
            }

            // Remove onEvent handlers from elements
            matches = _RegExOnEventAttributes.Matches(html);
            foreach (Match match in matches)
            {
                var txt = match.Value;
                if (match.Groups.Count > 1)
                {
                    var onEvent = match.Groups[1].Value;
                    txt = txt.Replace(onEvent, string.Empty);
                    if (!string.IsNullOrEmpty(txt))
                        html = html.Replace(match.Value, txt);
                }
            }

            return html;
        }
        #endregion
    }
}
