using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MarkdownMonster;
using Westwind.Utilities;

namespace MarkdownMonster.BrowserComInterop
{

    /// <summary>
    /// JavaScript 
    /// </summary>
    public class PreviewBrowserJavaScriptInterop : BaseBrowserInterop
    {
        private PreviewBrowserDotnetInterop _webViewPreviewDotnetInterop;
        private object WebBrowser;


        public PreviewBrowserJavaScriptInterop(PreviewBrowserDotnetInterop interop)
        {
            _webViewPreviewDotnetInterop = interop;
            WebBrowser = interop.WebBrowser;
            if(WebBrowser is WebBrowser)
                Instance = GetInvocationRoot(WebBrowser as WebBrowser);
        }

        public PreviewBrowserJavaScriptInterop(object webBrowser)
        {
            WebBrowser = webBrowser;
            if(WebBrowser is WebBrowser)
                Instance = GetInvocationRoot(WebBrowser as WebBrowser);
        }


        /// <summary>
        /// Retrieves the JavaScript document window instance object
        /// that's used to invoke methods.
        ///
        /// This method is meant to abstract the base object in such
        /// a way that invoke methods can run
        /// </summary>
        /// <param name="browser">Browser instance</param>
        /// <returns></returns>
        public virtual object GetInvocationRoot(object browser)
        {
            var doc = ReflectionUtils.GetPropertyCom(browser, "Document");
            if ( doc == null)
                return null;

            return ReflectionUtils.GetPropertyCom(doc, "parentWindow");
        }


        public void InitializeInterop(object instance)
        {
            Invoke("initializeinterop", instance);
        }


        public void UpdateDocumentContent(string html, int lineNo)
        {
            if (_webViewPreviewDotnetInterop.WebBrowser == null)
                return;

            //_webViewPreviewDotnetInterop.htmlToUpdate = html;

            Invoke("updateDocumentContent", html, lineNo);
        }


        public void ScrollToPragmaLine(int editorLineNumber = -1,
            string headerId = null,
            bool updateCodeBlocks = true,
            bool noScrollTimeout = false, bool noScrollTopAdjustment = false)
        {
            Invoke("scrollToPragmaLine", editorLineNumber, headerId, noScrollTimeout, noScrollTopAdjustment);
        }
        
        public void ScrollToHtmlBlock(string htmlText)
        {
            Invoke("scrollToHtmlBlock", htmlText);
        }
        

        public void SetHighlightTimeout(int timeout)
        {
            SetEx("previewer.highlightTimeout", timeout);
        }


    }
}
