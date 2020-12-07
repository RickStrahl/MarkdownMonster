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
    public class PreviewBrowserDotnetInterop : BaseBrowserInterop
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


        public PreviewBrowserDotnetInterop(AppModel model, object webBrowser, object baseInstance) : base(baseInstance)
        {
            Model = model;
            WebBrowser = webBrowser;
        }

        public static object GetWebBrowserWindow(WebBrowser browser)
        {
            if ( browser.Document == null)
                return null;

            return ReflectionUtils.GetPropertyCom(browser.Document, "parentWindow");
        }

        /// <summary>
        /// Initial call into JavaScript to 
        /// </summary>
        public void InitializeInterop()
        {
            Invoke("initializeinterop", this);
        }



        public void gotoLine(object editorLine, object noRefresh)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Model.ActiveEditor?.GotoLine((int)editorLine, (bool)noRefresh);
            });
        }

        public void GotoBottom(object noRefresh, object noSelection)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Model.ActiveEditor?.GotoBottom((bool)noRefresh, (bool)noSelection);
            });
        }


        public void PreviewContextMenu(string positionAndElementType)
        {
            var pos = JsonSerializationUtils.Deserialize(positionAndElementType, typeof(PositionAndDocumentType));
            mmApp.Model.Window.PreviewBrowser.ExecuteCommand("PreviewContextMenu", pos);
        }


        public bool PreviewLinkNavigation(string url, string src = null)
        {
            var editor = Model.ActiveEditor;
            return editor.PreviewLinkNavigation(url, src);
        }


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
