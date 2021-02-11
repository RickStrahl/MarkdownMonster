using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using HtmlAgilityPack;
using MarkdownMonster.Windows;
using Westwind.Utilities;
using WebBrowser = System.Windows.Controls.WebBrowser;

namespace MarkdownMonster.Utilities
{
    public class SearchEngine
    {
        /// <summary>
        /// Opens a browser with the search term provided in the 
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="searchEngineType">duckduckgo,google,bing (any other domain name that can be postfixed with .com)</param>
        public void OpenSearchEngine(string searchTerm, SearchEngineTypes searchEngineType = SearchEngineTypes.DuckDuckGo)
        {
            var url = "https://{0}.com?q={1}";
            url = string.Format(url, searchEngineType, searchTerm);

            ShellUtils.OpenUrl(url);
        }

        //private BrowserDialog window;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="searchEngineType"></param>
        /// <returns></returns>
        public async Task<List<SearchItem>> GetSearchLinks(string searchTerm,
                        SearchEngineTypes searchEngineType = SearchEngineTypes.DuckDuckGo)
        {
            var list = new List<SearchItem>();
            var url = "https://{0}.com?q={1}";
            url = string.Format(url, searchEngineType, searchTerm);

            var window = new BrowserDialog();
            window.Width = 0;
            window.Height = 0;
            window.Top = -50000; // offsreen but visible
            window.Visibility = Visibility.Visible;

            if (!window.NavigateAndWaitForCompletion(url))
                return null;



            await window.Dispatcher.DelayAsync(1000, (p) =>
            {
                dynamic document = window.Browser.Document;
                var html = document.Body.InnerHtml as string;
                window?.Close();

                if (string.IsNullOrEmpty((html)))
                    return;

                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var links = doc.DocumentNode.SelectNodes("//*/h2/a[1]");

                if (links == null)
                    return;

                foreach (var link in links)
                {
                    var searchItem = new SearchItem();
                    url = link.Attributes["href"]?.Value;
                    if (url.Contains("?uddg="))
                        url = StringUtils.ExtractString(url, "?uddg=", "x",
                                                        allowMissingEndDelimiter: true);
                    searchItem.Url = StringUtils.UrlDecode(url);
                    searchItem.Title = link.InnerText;
                    list.Add(searchItem);
                }
            }, DispatcherPriority.ApplicationIdle);

            
            return list;
        }

        private void Browser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
         


            var browser = sender as WebBrowser;
            return;


            //var html = await HttpUtils.HttpRequestStringAsync(new HttpRequestSettings()
            //{
            //    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.146 Safari/537.36",
            //    Url = url
            //});

            
        }
    }

    public enum SearchEngineTypes
    {
        DuckDuckGo,
        Bing,
        Google
    }

    public class SearchItem
    {
        public string Title { get; set; }
        public string Domain { get; set; }
        public string Url { get; set; }
    }
}
