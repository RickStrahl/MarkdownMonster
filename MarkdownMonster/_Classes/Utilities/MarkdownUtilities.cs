using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using MarkdownMonster.Windows;

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

        static readonly Regex YamlExtractionRegex = new Regex(@"^(?:-{3}|\.{3})[\n,\r\n].*?^(?:-{3}|\.{3})[\n,\r\n]", RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.Compiled);

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

    }
}
