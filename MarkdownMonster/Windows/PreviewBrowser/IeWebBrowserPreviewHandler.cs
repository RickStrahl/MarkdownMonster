using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Web;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using Markdig.Helpers;
using MarkdownMonster.BrowserComInterop;
using Westwind.Utilities;

namespace MarkdownMonster.Windows.PreviewBrowser
{
    public class IEWebBrowserPreviewHandler : IPreviewBrowser
    {
        /// <summary>
        /// Instance of the Web Browser control that hosts ACE Editor
        /// </summary>
        public WebBrowser WebBrowser { get; set; }



        IEWebBrowserEditorHandler wbHandler;

        /// <summary>
        /// Reference back to the main Markdown Monster window that
        /// </summary>
        public MainWindow Window { get; set; }

        public AppModel Model { get; set; }

        public bool IsVisible
        {
            get { return this.WebBrowser.Visibility == Visibility.Visible; }
            set { _isVisible = value; }
        }



        private bool _isVisible;

        #region Initialization
        public IEWebBrowserPreviewHandler(WebBrowser browser)
        {
            WebBrowser = browser;
            Model = mmApp.Model;
            Window = Model.Window;

            InitializePreviewBrowser();

            wbHandler = new IEWebBrowserEditorHandler(browser);
        }

        

        // IMPORTANT: for browser COM CSE errors which can happen with script errors

        private void InitializePreviewBrowser()
        {
            // wbhandle has additional browser initialization code
            // using the WebBrowserHostUIHandler
            WebBrowser.LoadCompleted += PreviewBrowserOnLoadCompleted;
        }


        private void PreviewBrowserOnLoadCompleted(object sender, NavigationEventArgs e)
        {
            if (e.Uri == null)
                return;

            string url = e.Uri.ToString();
            if (!url.Contains("_MarkdownMonster_Preview") && !url.Contains("__untitled.htm"))
                return;

            bool shouldScrollToEditor = WebBrowser.Tag != null && WebBrowser.Tag.ToString() == "EDITORSCROLL";
            WebBrowser.Tag = null;

            PreviewBrowserDotnetInterop dnInterop = null;
            MarkdownDocumentEditor editor = null;
            try
            {
                editor = Window.GetActiveMarkdownEditor();
                dnInterop = new PreviewBrowserDotnetInterop(Model, WebBrowser);
                dnInterop.InitializeInterop();

                dnInterop.JsInterop.SetHighlightTimeout(Model.Configuration.Editor.PreviewHighlightTimeout);

                //window.previewer.highlightTimeout = Model.Configuration.Editor.PreviewHighlightTimeout;

                if (shouldScrollToEditor)
                {
                    try
                    {
                        // scroll preview to selected line
                        if (mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorAndPreview ||
                            mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorToPreview)
                        {
                            int lineno = editor.GetLineNumber();
                            if (lineno > -1)
                            {
                                string headerId = string.Empty;
                                var lineText = editor.GetCurrentLine().Trim();
                                if (lineText.StartsWith("#") && lineText.Contains("# ")) // it's header
                                {
                                    lineText = lineText.TrimStart(new[] {' ', '#', '\t'});
                                    headerId = LinkHelper.UrilizeAsGfm(lineText);
                                }

                                if (editor.MarkdownDocument.EditorSyntax == "markdown")
                                    dnInterop.JsInterop.ScrollToPragmaLine(lineno, headerId);
                                else if (editor.MarkdownDocument.EditorSyntax == "html")
                                    dnInterop.JsInterop.ScrollToHtmlBlock(lineText);
                            }
                        }
                    }
                    catch
                    {
                        /* ignore scroll error */
                    }
                }
            }
            catch
            {
                // try again after a short wait
                Model.Window.Dispatcher.Delay(500,(i)=>
                {
                    var introp = i as PreviewBrowserDotnetInterop;
                    try
                    {
                        introp.InitializeInterop();
                        introp.JsInterop.SetHighlightTimeout(Model.Configuration.Editor.PreviewHighlightTimeout);
                    }
                    catch
                    {
                        //mmApp.Log("Preview InitializeInterop failed: " + url, ex);
                    }
                },dnInterop);
            }
        }

        public void Dispose()
        {
            WebBrowser.Dispose();
        }

        #endregion

        #region Preview
        /// <summary>
        /// Renders the current document or passed in HTML in the Web Browser preview
        /// or external preview
        /// </summary>
        /// <param name="editor">An instance of the active document editor</param>
        /// <param name="keepScrollPosition">True if scroll position should be maintained if possible</param>
        /// <param name="showInBrowser">If true renders in an external browser.</param>
        /// <param name="renderedHtml">Optional - pass in an HTML string. If null active document is rendered to HTML</param>
        /// <param name="editorLineNumber">Line to scroll the editor to</param>
        [HandleProcessCorruptedStateExceptions]
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public void PreviewMarkdown(MarkdownDocumentEditor editor = null,
            bool keepScrollPosition = false,
            bool showInBrowser = false,
            string renderedHtml = null,
            int editorLineNumber = -1)
        {
            try
            {
                // only render if the preview is actually visible and rendering in Preview Browser
                if (!Model.IsPreviewBrowserVisible && !showInBrowser)
                    return;

                if (editor == null)
                    editor = Window.GetActiveMarkdownEditor();

                if (editor == null)
                    return;

                var doc = editor.MarkdownDocument;
                var ext = Path.GetExtension(doc.Filename).ToLower().Replace(".", "");

                string mappedTo = "markdown";

                if (!string.IsNullOrEmpty(renderedHtml))
                {
                    mappedTo = "html";
                    ext = null;
                }
                else
                    mappedTo = editor.MarkdownDocument.EditorSyntax;

                
                PreviewBrowserDotnetInterop dotnetInterop = null;
                if (string.IsNullOrEmpty(ext) || mappedTo == "markdown" || mappedTo == "html")
                {

                    if (!showInBrowser)
                    {
                        if (keepScrollPosition)
                        {
                            dotnetInterop = new PreviewBrowserDotnetInterop(Model, WebBrowser);
                            dotnetInterop.InitializeInterop();
                        }
                        else
                        {
                            Window.ShowPreviewBrowser(false, false);
                        }
                    }

                    if (mappedTo == "html")
                    {
                        if (string.IsNullOrEmpty(renderedHtml))
                            renderedHtml = doc.CurrentText;

                        if (!doc.WriteFile(doc.HtmlRenderFilename,renderedHtml))
                            // need a way to clear browser window
                            return;

                        renderedHtml = StringUtils.ExtractString(renderedHtml,
                            "<!-- Markdown Monster Content -->",
                            "<!-- End Markdown Monster Content -->");
                    }
                    else
                    {
                        // Fix up `/` or `~/` Web RootPaths via `webRootPath: <path>` in YAML header
                        // Specify a physical or relative path that `\` or `~\` maps to
                        doc.GetPreviewWebRootPath();

                        bool usePragma = !showInBrowser && mmApp.Configuration.PreviewSyncMode != PreviewSyncMode.None;
                        if (string.IsNullOrEmpty(renderedHtml))
                            renderedHtml = doc.RenderHtmlToFile(usePragmaLines: usePragma);

                        if (renderedHtml == null)
                        {

                            Window.ShowStatusError($"Access denied: {Path.GetFileName(doc.Filename)}");
                            // need a way to clear browser window

                            return;
                        }

                        // Handle raw URLs to render
                        if (renderedHtml.StartsWith("http") && StringUtils.CountLines(renderedHtml) == 1)
                        {
                            WebBrowser.Navigate(new Uri(renderedHtml));
                            Window.ShowPreviewBrowser();
                            return;
                        }

                        renderedHtml = StringUtils.ExtractString(renderedHtml,
                            "<!-- Markdown Monster Content -->",
                            "<!-- End Markdown Monster Content -->");
                    }

                    if (showInBrowser)
                    {
                        var url = doc.HtmlRenderFilename;
                        mmFileUtils.ShowExternalBrowser(url);
                        return;
                    }

                    WebBrowser.Cursor = Cursors.None;
                    WebBrowser.ForceCursor = true;

                    // if content contains <script> tags we must do a full page refresh
                    bool forceRefresh = renderedHtml != null && renderedHtml.Contains("<script ");


                    if (keepScrollPosition && !mmApp.Configuration.AlwaysUsePreviewRefresh && !forceRefresh)
                    {
                        string browserUrl = WebBrowser.Source.ToString().ToLower();
                        string documentFile = "file:///" +
                                              doc.HtmlRenderFilename.Replace('\\', '/')
                                                  .ToLower();
                        if (browserUrl == documentFile)
                        {
                            if (string.IsNullOrEmpty(renderedHtml))
                                PreviewMarkdown(editor, false, false); // fully reload document
                            else
                            {
                                try
                                {

                                    try
                                    {
                                        // scroll preview to selected line
                                        if (mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorAndPreview ||
                                            mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorToPreview ||
                                            mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.NavigationOnly)
                                        {
                                            int highlightLineNo = editorLineNumber;
                                                if (editorLineNumber < 0)
                                            {
                                                highlightLineNo = editor.GetLineNumber();
                                                editorLineNumber = highlightLineNo;
                                            }
                                            if (renderedHtml.Length < 100000)
                                                highlightLineNo = 0; // no special handling render all code snippets

                                            var lineText = editor.GetLine(editorLineNumber).Trim();

                                            if (mmApp.Configuration.PreviewSyncMode != PreviewSyncMode.NavigationOnly)
                                              dotnetInterop.JsInterop.UpdateDocumentContent(renderedHtml,highlightLineNo);

                                            // TODO: We need to get Header Ids
                                            var headerId = string.Empty; // headers may not have pragma lines
                                            if (editorLineNumber > -1)
                                            {
                                                if (lineText.StartsWith("#") && lineText.Contains("# ")) // it's header
                                                {
                                                    lineText = lineText.TrimStart(new[] { ' ', '#', '\t' });
                                                    headerId = LinkHelper.UrilizeAsGfm(lineText);
                                                }
                                            }

                                            if (editor.MarkdownDocument.EditorSyntax == "markdown")
                                                dotnetInterop.JsInterop.ScrollToPragmaLine(editorLineNumber, headerId);
                                            else if (editor.MarkdownDocument.EditorSyntax == "html")
                                                dotnetInterop.JsInterop.ScrollToHtmlBlock(lineText);
                                        }
                                        else
                                            dotnetInterop.JsInterop.UpdateDocumentContent(renderedHtml,0);
                                    }
                                    catch
                                    {
                                        /* ignore scroll error */
                                    }
                                }
                                catch
                                {
                                    // Refresh doesn't fire Navigate event again so
                                    // the page is not getting initiallized properly
                                    //PreviewBrowser.Refresh(true);
                                    WebBrowser.Tag = "EDITORSCROLL";
                                    WebBrowser.Navigate(new Uri(doc.HtmlRenderFilename));
                                }
                            }

                            return;
                        }
                    }

                    WebBrowser.Tag = "EDITORSCROLL";
                    WebBrowser.Navigate(new Uri(doc.HtmlRenderFilename));
                    return;
                }

                // not a markdown or HTML document to preview
                Window.ShowPreviewBrowser(true, keepScrollPosition);
            }
            catch (Exception ex)
            {
                //mmApp.Log("PreviewMarkdown failed (Exception captured - continuing)", ex);
                Debug.WriteLine("PreviewMarkdown failed (Exception captured - continuing): " + ex.Message,ex);
            }
        }

        private DateTime invoked = DateTime.MinValue;

        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false, string renderedHtml = null, int editorLineNumber = -1)
        {
            if (!mmApp.Configuration.IsPreviewVisible)
                return;

            var current = DateTime.UtcNow;

            // prevent multiple stacked refreshes
            if (invoked == DateTime.MinValue) // || current.Subtract(invoked).TotalMilliseconds > 4000)
            {
                invoked = current;
                Application.Current.Dispatcher.InvokeAsync(
                    () => {
                        try
                        {
                            PreviewMarkdown(editor, keepScrollPosition, false, renderedHtml, editorLineNumber == 0 ? 0 : editorLineNumber -1);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Preview Markdown Async Exception: " + ex.Message);
                        }
                        finally
                        {
                            invoked = DateTime.MinValue;
                        }
                    }, DispatcherPriority.ApplicationIdle);
            }
        }

        public async Task ScrollToEditorLineAsync(int editorLineNumber = -1, bool updateCodeBlocks = false, bool noScrollTimeout = false, bool noScrollTopAdjustment = false)
        {
            await Application.Current.Dispatcher.InvokeAsync(() => ScrollToEditorLine(editorLineNumber, updateCodeBlocks, noScrollTimeout, noScrollTopAdjustment));
        }

        public void ScrollToEditorLine(int editorLineNumber = -1, bool updateCodeBlocks = false, bool noScrollTimeout = false, bool noScrollTopAdjustment = false)

        {
            var interop = new PreviewBrowserDotnetInterop(Model, WebBrowser);
            interop.InitializeInterop();

            // let's make sure the doc is loaded
            if(interop.JsInterop?.Instance == null)
                return;

            var editor = Window.GetActiveMarkdownEditor();
            if (editor == null)
                return;

            if (editorLineNumber < 0)
                editorLineNumber = editor.GetLineNumber();
            

            string lineText = null;
            
            // TODO: We need to get Header Ids
            var headerId = string.Empty; // headers may not have pragma lines
            if (editorLineNumber > -1)
            {
                lineText = editor.GetLine(editorLineNumber).Trim();

                if (lineText.StartsWith("#") && lineText.Contains("# ")) // it's header
                {
                    lineText = lineText.TrimStart(new[] {' ', '#', '\t'});
                    headerId = LinkHelper.UrilizeAsGfm(lineText);
                }
            }

            if (editor.MarkdownDocument.EditorSyntax == "markdown")
                interop.JsInterop.ScrollToPragmaLine(editorLineNumber, headerId, noScrollTimeout, noScrollTopAdjustment);
            else if (editor.MarkdownDocument.EditorSyntax == "html")
                interop.JsInterop.ScrollToHtmlBlock(lineText ?? editor.GetLine(editorLineNumber) );
        }

        #endregion

        #region Navigation
        public void Navigate(string url)
        {
            WebBrowser.Navigate(new Uri(url));
        }

        public void Refresh(bool noCache = false)
        {
            WebBrowser.Refresh(noCache);

            PreviewMarkdownAsync();
        }

        public void ExecuteCommand(string command, params object[] args)
        {
            if (command ==  "PreviewContextMenu")
            {
                object parms = null;
                if (args != null && args.Length > 0)
                    parms = args[0];

                var menu = new PreviewBrowserContextMenu();
                menu.ShowContextMenu(new PositionAndDocumentType(parms),
                    Window.Model,
                    WebBrowser);
            }

            if (command == "PrintPreview")
            {
                object dom = WebBrowser.Document;
                ReflectionUtils.CallMethodCom(dom,"execCommand","print", true, null);
            }
        }

        public void ShowDeveloperTools()
        {
            MessageBox.Show(mmApp.Model.Window,
                "This browser doesn't support Developer tools.", "Developer Tools",
                MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
        }
        #endregion
    }



}
