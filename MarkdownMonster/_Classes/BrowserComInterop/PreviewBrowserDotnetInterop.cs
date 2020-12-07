using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MarkdownMonster.Windows.PreviewBrowser;
using Westwind.Utilities;

namespace MarkdownMonster.BrowserComInterop
{
    /// <summary>
    /// This object is passed into the Preview web browser
    /// and used to make callbacks from JavaScript into .NET
    /// </summary>
    public class PreviewBrowserDotnetInterop
    {
        internal AppModel Model { get; }

        public object WebBrowser { get; }

        /// <summary>
        /// Optional reference to the JavaScript interop that allows
        /// calling into JavaScript from .NET code.
        ///
        /// Provided primarily as a helper to make it easier to access
        /// JS code internally as well as for .NET browser initialization
        /// code which needs both directions of Interop.
        /// </summary>
        public PreviewBrowserJavaScriptInterop JsInterop
        {
            get
            {
                if (_jsInterop != null) return _jsInterop;

                _jsInterop = new PreviewBrowserJavaScriptInterop(this);
                return _jsInterop;
            }
        }
        private PreviewBrowserJavaScriptInterop _jsInterop;


        public PreviewBrowserDotnetInterop(AppModel model, object webBrowser)
        {
            Model = model;
            WebBrowser = webBrowser;

            // pass this object into the Preview Browser
            // or otherwise initialize via `initializeInterop in JS)
            InitializeInterop();
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
        

        /// <summary>
        /// Initial call into JavaScript to 
        /// </summary>
        public void InitializeInterop()
        {
            JsInterop.InitializeInterop(this);
        }


        /// <summary>
        /// Navigates the Editor to a specified line
        /// </summary>
        /// <param name="editorLine"></param>
        /// <param name="noRefresh"></param>
        public void gotoLine(object editorLine, object noRefresh)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Model.ActiveEditor?.GotoLine((int)editorLine, (bool)noRefresh);
            });
        }

        /// <summary>
        /// Goes to the bottom of the editor
        /// </summary>
        /// <param name="noRefresh"></param>
        /// <param name="noSelection"></param>
        public void GotoBottom(object noRefresh, object noSelection)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Model.ActiveEditor?.GotoBottom((bool)noRefresh, (bool)noSelection);
            });
        }


        /// <summary>
        /// Shows the WPF Preview menu
        /// </summary>
        /// <param name="positionAndElementType"></param>
        public void PreviewContextMenu(string positionAndElementType)
        {
            var pos = JsonSerializationUtils.Deserialize(positionAndElementType, typeof(PositionAndDocumentType));
            mmApp.Model.Window.PreviewBrowser.ExecuteCommand("PreviewContextMenu", pos);
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
            var editor = Model.ActiveEditor;
            return editor.PreviewLinkNavigation(url, src);
        }


        /// <summary>
        /// Checks to see if the editor and preview are synced and if scrolling
        /// the preview needs to scroll the editor.
        /// </summary>
        /// <returns></returns>
        public bool IsPreviewToEditorSync()
        {
            var result = Model.Window.Dispatcher.Invoke(() =>
            {
                if (Model.ActiveEditor != null)
                    return Model.ActiveEditor.IsPreviewToEditorSync();

                return false;
            });

            return result;
        }

    }
}
