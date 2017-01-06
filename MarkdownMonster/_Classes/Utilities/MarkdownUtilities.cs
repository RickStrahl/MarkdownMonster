using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MarkdownMonster.Windows;

namespace MarkdownMonster
{
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
                    MessageBox.Show("Unable to convert Html to Markdown. Returning Html.","Html to Markdown Conversion Failed.",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }

            form.Close();
            form = null;

            return markdown ?? html;
#endif
        }
    }
}
