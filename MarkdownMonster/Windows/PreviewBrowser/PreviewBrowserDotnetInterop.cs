#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Westwind.Utilities;

namespace MarkdownMonster.Windows.PreviewBrowser
{
    public class PreviewBrowserDotnetInterop
    {
        internal AppModel Model { get; }
        public object WebBrowser { get; }

        public PreviewBrowserDotnetInterop(AppModel model, object webBrowser)
        {
            Model = model;
            WebBrowser = webBrowser;
        }


        public void gotoLine(object editorLine, object noRefresh)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Model.ActiveEditor?.GotoLine((int) editorLine, (bool) noRefresh);
            });
        }

        public void GotoBottom(object noRefresh, object noSelection)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Model.ActiveEditor?.GotoBottom((bool) noRefresh, (bool) noSelection);
            });
        }


        public void PreviewContextMenu(string positionAndElementType)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                var pos = JsonSerializationUtils.Deserialize(positionAndElementType, typeof(PositionAndDocumentType));
                mmApp.Model.Window.PreviewBrowser.ExecuteCommand("PreviewContextMenu", pos);
            });
        }


        public bool PreviewLinkNavigation(string url, string src = null)
        {
            return Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                var editor = Model.ActiveEditor;
                return editor.PreviewLinkNavigation(url, src);
            });
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
#endif
