
using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MarkdownMonster;
using MarkdownMonster.Windows.PreviewBrowser;
using Microsoft.Web.WebView2.Wpf;
using WebViewPreviewerAddin;
using Westwind.Utilities;

namespace ChromiumPreviewerAddin
{
    public class PreviewDotnetInterop
    {
        internal WebView2 WebBrowser;
        internal AppModel Model;
        internal WebViewPreviewerAddin.WebViewPreviewerAddin Addin;
        internal PreviewJavaScriptInterop PreviewJavaScriptInterop;

        public string htmlToUpdate { get; set; }

        public string Parm2 { get; set; }
        
        public PreviewDotnetInterop(AppModel model, WebViewPreviewerAddin.WebViewPreviewerAddin addin, WebView2 browser)
        {
            WebBrowser = browser;
            Model = model;
            Addin = addin;
            PreviewJavaScriptInterop = new PreviewJavaScriptInterop(this);
        }

        
        public void gotoLine(object editorLine, object noRefresh)
        {
            Dispatcher.CurrentDispatcher.Invoke(()=>
            {
                Model.ActiveEditor?.GotoLine((int) editorLine,(bool) noRefresh);
            });
        }

        public void GotoBottom(object noRefresh, object noSelection)
        {
            Dispatcher.CurrentDispatcher.Invoke(()=>
            {
                Model.ActiveEditor?.GotoBottom((bool) noRefresh,(bool) noSelection);
            });
        }


        public void  PreviewContextMenu(string positionAndElementType)
        {
            var pos = JsonSerializationUtils.Deserialize(positionAndElementType, typeof(PositionAndDocumentType));
            mmApp.Model.Window.PreviewBrowser.ExecuteCommand("PreviewContextMenu", pos);
        }

        
        public bool IsPreviewToEditorSync()
        {
            var result = Model.Window.Dispatcher.Invoke(()=>
            {
                if (Model.ActiveEditor != null )
                    return Model.ActiveEditor.IsPreviewToEditorSync();

                return false;
            });

            return result;
        }

    }
}

