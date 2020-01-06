using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Westwind.Utilities;

namespace MarkdownMonster
{
    /// <summary>
    /// Wrapper around the Preview Browser's Window instance
    ///
    /// Used primarily to update the window
    /// </summary>
    public class PreviewBrowserInterop : BaseBrowserInterop
    {
        public PreviewBrowserInterop(object windowInstance) : base(windowInstance)
        {
        }


        public static object GetWindow(WebBrowser browser)
        {
            return ReflectionUtils.GetPropertyCom(browser.Document, "parentWindow");
        }

        public void InitializeInterop(MarkdownDocumentEditor editor)
        {
            Invoke("initializeinterop", editor);
        }

        public void SetHighlightTimeout(int timeout)
        {
            SetEx("previewer.highlightTimeout", timeout);
        }

        public void UpdateDocumentContent(string html, int lineNo)
        {
            Invoke("updateDocumentContent", html, lineNo);
        }

        /// <summary>
        /// Scrolls to a pragma line retrieved from the Editor
        /// </summary>
        /// <param name="lineNumber">Line to go to</param>
        /// <param name="headerId">Optionally go to a header id</param>
        /// <param name="noScrollTimeout">Fire scroll events immediately</param>
        /// <param name="noScrollTopAdjustment">Don't scroll just highlight - good for cursor navigations</param>
        public void ScrollToPragmaLine(int lineNumber, string headerId, bool noScrollTimeout = false, bool noScrollTopAdjustment = false)
        {
            Invoke("scrollToPragmaLine", lineNumber,headerId, noScrollTimeout, noScrollTopAdjustment);
        }

        public void ScrollToHtmlBlock(string htmlText)
        {
            Invoke("scrollToHtmlBlock", htmlText);
        }

        public int GetScrollTop()
        {
            return Invoke<int>("getScrollTop",false);
        }

        public void HighlightCode(int lineNo)
        {
            Invoke("highlightCode", lineNo);
        }
    }
}
