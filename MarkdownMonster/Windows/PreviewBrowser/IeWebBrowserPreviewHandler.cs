using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using Westwind.Utilities;

namespace MarkdownMonster.Windows.PreviewBrowser
{
    public class IEWebBrowserPreviewHandler : IPreviewBrowser
    {
        /// <summary>
        /// Instance of the Web Browser control that hosts ACE Editor
        /// </summary>
        public WebBrowser WebBrowser { get; set; }

        public dynamic BrowserPreview { get; set; }


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

        public IEWebBrowserPreviewHandler(WebBrowser browser)
        {
            WebBrowser = browser;
            Model = mmApp.Model;
            Window = Model.Window;

            InitializePreviewBrowser();

            wbHandler = new IEWebBrowserEditorHandler(browser);
        }



        // IMPORTANT: for browser COM CSE errors which can happen with script errors

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
                {
                    // only show preview for Markdown and HTML documents
                    Model.Configuration.EditorExtensionMappings.TryGetValue(ext, out mappedTo);
                    mappedTo = mappedTo ?? string.Empty;
                }

                if (string.IsNullOrEmpty(ext) || mappedTo == "markdown" || mappedTo == "html")
                {
                    dynamic dom = null;
                    if (!showInBrowser)
                    {
                        if (keepScrollPosition)
                        {
                            dom = WebBrowser.Document;
                            doc.LastEditorLineNumber = dom.documentElement.scrollTop;
                        }
                        else
                        {
                            Window.ShowPreviewBrowser(false, false);
                            doc.LastEditorLineNumber = 0;
                        }
                    }

                    if (mappedTo == "html")
                    {
                        if (string.IsNullOrEmpty(renderedHtml))
                            renderedHtml = doc.CurrentText;

                        if (!doc.WriteFile(doc.HtmlRenderFilename,renderedHtml))
                            // need a way to clear browser window
                            return;
                    }
                    else
                    {
                        // Fix up `\` or `~\` Web RootPaths via `webRootPath: <path>` in YAML header
                        // Specify a physical or relative path that `\` or `~\` maps to
                        doc.GetPreviewWebRootPathFromDocument();

                        bool usePragma = !showInBrowser && mmApp.Configuration.PreviewSyncMode != PreviewSyncMode.None;
                        if (string.IsNullOrEmpty(renderedHtml))
                            renderedHtml = doc.RenderHtmlToFile(usePragmaLines: usePragma,
                                renderLinksExternal: mmApp.Configuration.MarkdownOptions.RenderLinksAsExternal);

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
                            dom = WebBrowser.Document;
                            //var content = dom.getElementById("MainContent");


                            if (string.IsNullOrEmpty(renderedHtml))
                                PreviewMarkdown(editor, false, false); // fully reload document
                            else
                            {
                                try
                                {
                                    // explicitly update the document with JavaScript code
                                    // much more efficient and non-jumpy and no wait cursor
                                    var window = dom.parentWindow;

                                    try
                                    {
                                        // scroll preview to selected line
                                        if (mmApp.Configuration.PreviewSyncMode ==
                                            PreviewSyncMode.EditorAndPreview ||
                                            mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorToPreview)
                                        {
                                            int highlightLineNo = editorLineNumber;
                                            if (editorLineNumber < 0)
                                            {
                                                highlightLineNo = editor.GetLineNumber();
                                                editorLineNumber = highlightLineNo;
                                            }
                                            if (renderedHtml.Length < 80000)
                                                highlightLineNo = 0; // no special handling render all code snippets

                                            window.updateDocumentContent(renderedHtml,highlightLineNo);
                                            window.scrollToPragmaLine(editorLineNumber);
                                        }
                                        else
                                            window.updateDocumentContent(renderedHtml);

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
                Debug.WriteLine("PreviewMarkdown failed (Exception captured - continuing)", ex);
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
                            PreviewMarkdown(editor, keepScrollPosition, false, renderedHtml, editorLineNumber);
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

        public void Navigate(string url)
        {
            WebBrowser.Navigate(new Uri(url));
        }

        public void Refresh(bool noCache = false)
        {
            WebBrowser.Refresh(noCache);

            PreviewMarkdownAsync();
        }

        public void ExecuteCommand(string command, params dynamic[] args)
        {
            if (command == "PreviewContextMenu")
            {
                var ctm = WebBrowser.ContextMenu;
                ctm.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                ctm.PlacementTarget = WebBrowser;
                ctm.IsOpen = true;
            }

            if (command == "PrintPreview")
            {
                dynamic dom = WebBrowser.Document;
                dom.execCommand("print", true, null);
            }
        }


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

            dynamic window = null;
            MarkdownDocumentEditor editor = null;
            try
            {
                editor = Window.GetActiveMarkdownEditor();
                dynamic dom = WebBrowser.Document;
                window = dom.parentWindow;
                dom.documentElement.scrollTop = editor.MarkdownDocument.LastEditorLineNumber;

                window.initializeinterop(editor);
                window.previewer.highlightTimeout = Model.Configuration.Editor.PreviewHighlightTimeout;

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
                                window.scrollToPragmaLine(lineno);
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
                Model.Window.Dispatcher.Delay(500,(w)=>
                {
                    dynamic win = (dynamic) w;
                    try
                    {
                        win.initializeinterop(editor);
                        win.previewer.highlightTimeout = Model.Configuration.Editor.PreviewHighlightTimeout;
                    }
                    catch
                    {
                        //mmApp.Log("Preview InitializeInterop failed: " + url, ex);
                    }
                },(object) window);
            }
        }

        public void Dispose()
        {
            WebBrowser.Dispose();
        }

    }


}
