using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Media;
using System.Windows.Threading;
using FontAwesome.WPF;
using Markdig.Helpers;
using MarkdownMonster;
using MarkdownMonster.Windows.PreviewBrowser;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Westwind.Utilities;

namespace WebViewPreviewerAddin
{
    public class WebViewPreviewHandler : IPreviewBrowser
    {
        private WebView2 WebBrowser { get; set; }

        // <summary>
        /// Reference back to the main Markdown Monster window that 
        /// </summary>
        public MainWindow Window { get; set; }

        /// <summary>
        /// The Application Model for easier access in this control
        /// </summary>
        public AppModel Model { get; set; }


        /// <summary>
        /// Shortcut to visibility setting
        /// </summary>
        public bool IsVisible
        {
            get { return WebBrowser.Visibility == Visibility.Visible; }
            set { _isVisible = value; }
        }
        private bool _isVisible;


        bool IPreviewBrowser.IsVisible => this.IsVisible;

        /// <summary>
        /// The object passed into the JavaScript page to allow for callbacks from
        /// JavaScript into .NET code/MM
        /// </summary>
        public WebViewPreviewDotnetInterop DotnetInterop { get; set; }

        /// <summary>
        /// Object that can be used to access JavaScript operations on the
        /// Preview window. Runs global functions in the document using CallMethod()
        /// </summary>
        public WebViewPreviewJavaScriptInterop JsInterop {get; set; }

        public WebViewPreviewHandler(WebView2 webViewBrowser)
        {
            WebBrowser = webViewBrowser; 
            Model = mmApp.Model;
            Window = Model.Window;

            // externalize so we can use async in the method
            _ = InitializeAsync();
        }

        async Task InitializeAsync()
        {
            // must create a data folder if running out of a secured folder that can't write like Program Files
            var browserFolder = Path.Combine(mmApp.Configuration.CommonFolder, "WebView_Browser");
            var env = await CoreWebView2Environment.CreateAsync(
                userDataFolder: browserFolder
            );
            
            await WebBrowser.EnsureCoreWebView2Async(env);

            WebBrowser.NavigationCompleted += WebBrowser_NavigationCompleted;

            if (Model.Configuration.System.ShowDeveloperToolsOnStartup)
                WebBrowser.CoreWebView2.OpenDevToolsWindow();

            // Set up interop object to pass into JavaScript
            DotnetInterop = new WebViewPreviewDotnetInterop(Model, WebBrowser);
            JsInterop = DotnetInterop.JsInterop;
            WebBrowser.CoreWebView2.AddHostObjectToScript("mm", DotnetInterop);
        }


        private async void WebBrowser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // Call JavaScript method to initialize the document
            // this essentially launches the editor, sets styles etc. 
            await JsInterop.InitializeInterop();
        }
        
        public void Dispose()
        {
            WebBrowser = null; // ensure reference cleared
        }
        
        private DateTime invoked = DateTime.MinValue;
        public void PreviewMarkdown(MarkdownDocumentEditor editor = null,
            bool keepScrollPosition = false, bool showInBrowser = false,
            string renderedHtml = null, int editorLineNumber = -1)
        {
            if(DotnetInterop == null)
                return;

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
                    // TODO: Get DOM and 
                    //dynamic dom = null;
                    //if (!showInBrowser)
                    //{
                    //    if (keepScrollPosition)
                    //    {
                    //        dom = WebBrowser..Document;
                    //        editor.MarkdownDocument.LastEditorLineNumber = dom.documentElement.scrollTop;
                    //    }
                    //    else
                    //    {
                    //        Window.ShowPreviewBrowser(false, false);
                    //        editor.MarkdownDocument.LastEditorLineNumber = 0;
                    //    }
                    //}

                    if (mappedTo == "html")
                    {
                        if (string.IsNullOrEmpty(renderedHtml))
                            renderedHtml = editor.MarkdownDocument.CurrentText;

                        if (!editor.MarkdownDocument.WriteFile(editor.MarkdownDocument.HtmlRenderFilename, renderedHtml))
                            // need a way to clear browser window
                            return;
                    }
                    else
                    {
                        bool usePragma = !showInBrowser && mmApp.Configuration.PreviewSyncMode != PreviewSyncMode.None;
                        if (string.IsNullOrEmpty(renderedHtml))
                            renderedHtml = editor.MarkdownDocument.RenderHtmlToFile(usePragmaLines: usePragma);

                        if (renderedHtml == null)
                        {
                            Window.SetStatusIcon(FontAwesomeIcon.Warning, Colors.Red, false);
                            Window.ShowStatus($"Access denied: {Path.GetFileName(editor.MarkdownDocument.Filename)}",
                                5000);
                            // need a way to clear browser window

                            return;
                        }

                        renderedHtml = StringUtils.ExtractString(renderedHtml,
                            "<!-- Markdown Monster Content -->",
                            "<!-- End Markdown Monster Content -->");
                    }

                    if (showInBrowser)
                    {
                        var url = editor.MarkdownDocument.HtmlRenderFilename;
                        mmFileUtils.ShowExternalBrowser(url);
                        return;
                    }

                    WebBrowser.Cursor = System.Windows.Input.Cursors.None;
                    WebBrowser.ForceCursor = true;

                    // if content contains <script> tags we must do a full page refresh
                    bool forceRefresh = renderedHtml != null && renderedHtml.Contains("<script ");


                    if (keepScrollPosition && !mmApp.Configuration.AlwaysUsePreviewRefresh && !forceRefresh)
                    {
                        string browserUrl = WebBrowser.Source?.ToString().ToLower();
                        string documentFile = "file:///" +
                                              editor.MarkdownDocument.HtmlRenderFilename.Replace('\\', '/')
                                                  .ToLower();
                        if (browserUrl == documentFile)
                        {
                            if (string.IsNullOrEmpty(renderedHtml))
                                PreviewMarkdown(editor, false, false); // fully reload document
                            else
                            {
                                try
                                {

                                    JsInterop = DotnetInterop.JsInterop;
                                    
                                    int lineno = editor.GetLineNumber();
                                    _ = JsInterop.UpdateDocumentContent(renderedHtml, lineno);

                                    try
                                    {
                                        // scroll preview to selected line
                                        if (mmApp.Configuration.PreviewSyncMode ==
                                            PreviewSyncMode.EditorAndPreview ||
                                            mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorToPreview)
                                        {
                                            if (lineno > -1)
                                                _= JsInterop.ScrollToPragmaLine(lineno);
                                        }
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


                                    WebBrowser.Source = new Uri(editor.MarkdownDocument.HtmlRenderFilename);
                                    mmApp.Log("Document Update Crash", null, false, LogLevels.Information);
                                }
                            }

                            return;
                        }
                    }

                    WebBrowser.Tag = "EDITORSCROLL";
                    var uri = new Uri(editor.MarkdownDocument.HtmlRenderFilename);

                    if (WebBrowser.Source != null && WebBrowser.Source.Equals(uri))
                        WebBrowser.CoreWebView2.Reload();
                    else
                        WebBrowser.Source = new Uri(editor.MarkdownDocument.HtmlRenderFilename);

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


        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false,
            string renderedHtml = null, int editorLineNumber = -1)
        {
            if (!mmApp.Configuration.IsPreviewVisible)
                return;

            var current = DateTime.UtcNow;

            // prevent multiple stacked refreshes
            if (invoked == DateTime.MinValue) // || current.Subtract(invoked).TotalMilliseconds > 4000)
            {
                invoked = current;
                Model.Window.Dispatcher.InvokeAsync(
                    () => {
                        try
                        {
                            PreviewMarkdown(editor, keepScrollPosition, renderedHtml: renderedHtml);
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

        public void ScrollToEditorLine(int editorLineNumber = -1, 
            bool updateCodeBlocks = false, 
            bool noScrollTimeout = false,
            bool noScrollTopAdjustment = false)
        {

            var editor = Window.GetActiveMarkdownEditor();
            if (editor == null || editor.MarkdownDocument == null)
                return;

            if (editorLineNumber < 0)
                editorLineNumber = editor.GetLineNumber();
            

            string lineText = null;
            
            // TODO: We need to get Header Ids
            var headerId = string.Empty; // headers may not have pragma lines
            if (editorLineNumber > -1)
            {
                lineText = editor.GetLine(editorLineNumber)?.Trim();

                if (lineText != null && lineText.StartsWith("#") && lineText.Contains("# ")) // it's header
                {
                    lineText = lineText.TrimStart(new[] {' ', '#', '\t'});
                    headerId = LinkHelper.UrilizeAsGfm(lineText);
                }
            }

            if (editor.MarkdownDocument.EditorSyntax == "markdown")
            {
                _ = JsInterop.ScrollToPragmaLine(editorLineNumber, headerId, noScrollTimeout, noScrollTopAdjustment);
            }
            else if (editor.MarkdownDocument.EditorSyntax == "html")
                _ = JsInterop.CallMethod("scrollToHtmlBlock", lineText ?? editor.GetLine(editorLineNumber));
            else
                _ = JsInterop.ScrollToPragmaLine(editorLineNumber, headerId);
        }

        public async Task ScrollToEditorLineAsync(int editorLineNumber = -1,
            bool updateCodeBlocks = false,
            bool noScrollTimeout = false,
            bool noScrollTopAdjustment = false)
        {
            await mmApp.Model?.Window?.Dispatcher?.InvokeAsync(() =>
                                ScrollToEditorLine(editorLineNumber,
                                updateCodeBlocks,
                                noScrollTimeout, noScrollTopAdjustment));
        }

        public void Navigate(string url)
        {
            WebBrowser.Source = null;   //  force Url Change
            WebBrowser.Source = new Uri(url);
        }

        public void Refresh(bool noCache)
        {
            WebBrowser.CoreWebView2.Reload();
        }

        
        public void ExecuteCommand(string command, params dynamic[] args)
        {
            if (command ==  "PreviewContextMenu")
            {
                object parms = null;
                if (args != null && args.Length > 0)
                    parms = args[0];

                if (parms is string)
                {
                    
                }

                var menu = new PreviewBrowserContextMenu();
                menu.ShowContextMenu(new PositionAndDocumentType(parms),
                    Window.Model,
                    WebBrowser);
            }

            if (command == "PrintPreview")
            {
                //object dom = WebBrowser..Document;
                //ReflectionUtils.CallMethodCom(dom,"execCommand","print", true, null);
            }
        }

        public void ShowDeveloperTools()
        {
            WebBrowser.CoreWebView2.OpenDevToolsWindow();
        }

    }
}
