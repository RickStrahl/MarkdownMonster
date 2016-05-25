using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static string HtmlToMarkdown(string html)
        {
            string markdown = null;
            string htmlFile = Path.Combine(Environment.CurrentDirectory, "Editor\\htmltomarkdown.htm");

            var form = new BrowserDialog();
            form.ShowInTaskbar = false;
            form.Width = 1;
            form.Height = 1;
            form.Left = -10000;
            form.Show();

            bool exists = File.Exists(htmlFile);
            form.Navigate(htmlFile);

            WindowUtilities.DoEvents();

            for (int i = 0; i < 200; i++)
            {
                dynamic doc = form.Browser.Document;

                if (!form.IsLoaded)
                {
                    Task.Delay(10);
                    WindowUtilities.DoEvents();
                }
                else
                {
                    markdown = doc.ParentWindow.htmltomarkdown(html);
                    break;
                }
            }

            form.Close();
            form = null;

            return markdown ?? html;
        }
    }
}
