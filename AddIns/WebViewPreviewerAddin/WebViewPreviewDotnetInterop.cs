using MarkdownMonster;
using MarkdownMonster.BrowserComInterop;


namespace WebViewPreviewerAddin
{

    /// <summary>
    /// Class that is called **from browser JavaScript** to interact
    /// with the Markdown Monster UI/Editor
    /// </summary>
    public class WebViewPreviewDotnetInterop : PreviewBrowserDotnetInterop
    {
        public WebViewPreviewDotnetInterop(AppModel model, object webBrowser) : base(model, webBrowser)
        {
        }

        /// <summary>
        /// Optional reference to the JavaScript interop that allows
        /// calling into JavaScript from .NET code.
        ///
        /// Provided primarily as a helper to make it easier to access
        /// JS code internally as well as for .NET browser initialization
        /// code which needs both directions of Interop.
        /// </summary>
        public new WebViewPreviewJavaScriptInterop JsInterop
        {
            get
            {
                if (_jsInterop != null) return _jsInterop;

                _jsInterop = new WebViewPreviewJavaScriptInterop(this);
                return _jsInterop;
            }
        }
        private WebViewPreviewJavaScriptInterop _jsInterop;



        /// <summary>
        /// Initial call into JavaScript to 
        /// </summary>
        public override void InitializeInterop()
        {
            JsInterop.InitializeInterop();
        }
    }
}

