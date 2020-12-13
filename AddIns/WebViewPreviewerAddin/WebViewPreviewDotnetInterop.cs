using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using MarkdownMonster;
using MarkdownMonster.BrowserComInterop;
using MarkdownMonster.Windows.PreviewBrowser;
using Westwind.Utilities;


namespace WebViewPreviewerAddin
{

    /// <summary>
    /// Class that is called **from browser JavaScript** to interact
    /// with the Markdown Monster UI/Editor
    /// </summary>
    [ComVisible(true)]
    public class WebViewPreviewDotnetInterop
        //: PreviewBrowserDotnetInterop
    {
        internal AppModel Model { get; }

        internal object WebBrowser { get; }

        public WebViewPreviewDotnetInterop(AppModel model, object webBrowser) // : base(model, webBrowser)
        {
            Model = model;
            WebBrowser = webBrowser;
        }

        /// <summary>
        /// Optional reference to the JavaScript interop that allows
        /// calling into JavaScript from .NET code.
        ///
        /// Provided primarily as a helper to make it easier to access
        /// JS code internally as well as for .NET browser initialization
        /// code which needs both directions of Interop.
        /// </summary>
        public  WebViewPreviewJavaScriptInterop JsInterop
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
        /// Initial call *into* JavaScript to 
        /// </summary>
        public void InitializeInterop()
        {
            // async - fire and forget we don't have to wait
            _ = JsInterop.InitializeInterop();
        }

        /// <summary>
        /// Intial Call into JavaScript
        /// </summary>
        /// <returns></returns>
        public async Task InitializeInteropAsync()
        {
            await JsInterop.InitializeInterop();
        }


        /// <summary>
        /// Navigates the Editor to a specified line
        /// </summary>
        /// <param name="editorLine"></param>
        /// <param name="noRefresh"></param>
        public async Task GotoLine(object editorLine, object noRefresh)
        {
            await Model.Window.Dispatcher.InvokeAsync(() =>
            {
                Model.ActiveEditor?.GotoLine(Convert.ToInt32(editorLine), (bool)noRefresh);
            });
        }

        /// <summary>
        /// Goes to the bottom of the editor
        /// </summary>
        /// <param name="noRefresh"></param>
        /// <param name="noSelection"></param>
        public async Task GotoBottom(object noRefresh, object noSelection)
        {
            await Model.Window.Dispatcher.InvokeAsync(() =>
            {
                Model.ActiveEditor?.GotoBottom((bool)noRefresh, (bool)noSelection);
            });
        }


        /// <summary>
        /// Shows the WPF Preview menu
        /// </summary>
        /// <param name="positionAndElementType"></param>
        public async Task PreviewContextMenu(string positionAndElementType)
        {
            await Model.Window.Dispatcher.InvokeAsync(() =>
            {
                var pos = JsonSerializationUtils.Deserialize(positionAndElementType, typeof(PositionAndDocumentType));
                mmApp.Model.Window.PreviewBrowser.ExecuteCommand("PreviewContextMenu", pos);
            });
        }


        /// <summary>
        /// Fired when a link is clicked in the preview editor. Opens a new
        /// external browser instance with the URL opened or opens certain
        /// supported files (like other markdown files) in the editor.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        public bool PreviewLinkNavigation(string url, string src = null)
        {
            bool handled =  Model.Window.Dispatcher.Invoke(() =>
            {
                var editor = Model.ActiveEditor;
                return editor.PreviewLinkNavigation(url, src);
            });
            return handled;
        }


        /// <summary>
        /// Checks to see if the editor and preview are synced and if scrolling
        /// the preview needs to scroll the editor.
        /// </summary>
        /// <remarks>Note this method is called SYNCHRONOUS</remarks>
        /// <returns></returns>
        public bool IsPreviewToEditorSync()
        {
            bool result =  Model.Window.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (Model.ActiveEditor != null)
                        return Model.ActiveEditor.IsPreviewToEditorSync();

                    return false;
                }
                catch
                {
                    return false;
                }
            });
            return result;
        }
    }
}

