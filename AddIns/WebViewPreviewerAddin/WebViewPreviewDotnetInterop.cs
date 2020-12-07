
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using MarkdownMonster.BrowserComInterop;
using MarkdownMonster.Windows.PreviewBrowser;
using Microsoft.Web.WebView2.Wpf;
using WebViewPreviewerAddin;
using Westwind.Utilities;

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
        public new  WebViewPreviewJavaScriptInterop JsInterop
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

        //public void gotoLine(object editorLine, object noRefresh)
        //{
        //    Dispatcher.CurrentDispatcher.Invoke(()=>
        //    {
        //        Model.ActiveEditor?.GotoLine((int) editorLine,(bool) noRefresh);
        //    });
        //}

        //public void GotoBottom(object noRefresh, object noSelection)
        //{
        //    Dispatcher.CurrentDispatcher.Invoke(()=>
        //    {
        //        Model.ActiveEditor?.GotoBottom((bool) noRefresh,(bool) noSelection);
        //    });
        //}


        //public void  PreviewContextMenu(string positionAndElementType)
        //{
        //    var pos = JsonSerializationUtils.Deserialize(positionAndElementType, typeof(PositionAndDocumentType));
        //    mmApp.Model.Window.PreviewBrowser.ExecuteCommand("PreviewContextMenu", pos);
        //}

      
        //public bool PreviewLinkNavigation(string url, string src = null)
        //{
        //    var editor = Model.ActiveEditor;
        //    return editor.PreviewLinkNavigation(url, src);
        //}


        //public bool IsPreviewToEditorSync()
        //{
        //    var result = Model.Window.Dispatcher.Invoke(()=>
        //    {
        //        if (Model.ActiveEditor != null )
        //            return Model.ActiveEditor.IsPreviewToEditorSync();

        //        return false;
        //    });

        //    return result;
        //}

    }
}

