using MarkdownMonster._Classes;

namespace MarkdownMonster.Services
{
    public class WebServerRequestHandler
    {
        public static WebServerResult OnMarkdownMonsterOperationHandler(WebServerOperation operation)
        {

            var result = mmApp.Model.Window.Dispatcher.Invoke(() =>
            {
                // Opens Markdown Monster using MM's command line options
                if (operation.Operation == "open" && !string.IsNullOrEmpty(operation.Data))
                {
                    // Open Markdown Monster documents
                    //App.CommandArgs = new[] {operation.Data};
                    mmApp.Model.Window.Dispatcher.InvokeAsync(() =>
                    {
                            var opener = new CommandLineOpener(mmApp.Model.Window);
                        opener.OpenFilesFromCommandLine( new[] {operation.Data});
                    });
                }
                // Opens the specified document from disk
                else if (operation.Operation == "openDocument")
                {
                    var tab = mmApp.Model.Window.OpenFile(operation.Data);
                    if (tab != null)
                        mmApp.Model.Window.ActivateTab(tab);
                }
                else if (operation.Operation == "openNew")
                {
                    var tab = mmApp.Model.Window.OpenTab("untitled");
                    if (tab != null)
                    {
                        mmApp.Model.Window.ActivateTab(tab);
                        var editor = tab.Tag as MarkdownDocumentEditor;
                        if (editor != null)
                        {
                            mmApp.Model.Window.Dispatcher.InvokeAsync(() =>
                            {
                                editor.SetMarkdown(operation.Data);
                                editor.PreviewMarkdownCallback(); // refresh the preview
                            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        }
                    }
                }
                // Get the active document
                else if (operation.Operation == "getDocument")
                {
                    var md = mmApp.Model?.ActiveEditor?.GetMarkdown();
                    var r =  new WebServerResult((object) md);
                    return r;
                }

                // return a no data result
                return new WebServerResult();
            });

            return result;
        }
    }
}
