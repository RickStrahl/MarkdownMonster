#region 
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 04/28/2016
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Markdig;
using MarkdownMonster.AddIns;
using MarkdownMonster.Annotations;
using MarkdownMonster.Utilities;
using MarkdownMonster.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using NHunspell;
using Westwind.Utilities;


namespace MarkdownMonster
{

    /// <summary>
    /// Wrapper around the Editor WebBrowser Control and the embedded
    /// Ace Editor instance that is contained within it. This class 
    /// manages creation of the WebBrowser instance and handles configuration
    /// and event firing. It also provides event interfaces for AceEditor
    /// callbacks and methods to affect the behavior of the AceEditor instance
    /// using the low level AceEditor property.
    /// </summary>
    [ComVisible(true)]
    public class MarkdownDocumentEditor : INotifyPropertyChanged

    {
        /// <summary>
        /// Instance of the Web Browser control that hosts ACE Editor
        /// </summary>
        public WebBrowser WebBrowser { get; set; }


        /// <summary>
        /// Reference back to the main Markdown Monster window that 
        /// </summary>
        public MainWindow Window { get; set;  }


        /// <summary>
        /// References the loaded MarkdownDocument instance. Note this 
        /// value can be null before the document has been loaded.
        /// </summary>
        public MarkdownDocument MarkdownDocument { get; set; }

        public dynamic AceEditor { get; set; }
        

        public string EditorSyntax
        {
            get => _editorSyntax;
            set
            {
                if (value == _editorSyntax) return;
                _editorSyntax = value;
                OnPropertyChanged();
            }
        }
        private string _editorSyntax;


        public int InitialLineNumber { get; set; }

        #region Behavior Properties and Storage
        /// <summary>
        /// Determines whether the editor displays as a read-only document
        /// </summary>
        public bool IsReadOnly { get; set; }

        
        /// <summary>
        /// Determines if the the document is treated as a preview
        /// </summary>
        public bool IsPreview
        {
            get => _isPreview;
            set
            {
                if (value == _isPreview) return;
                _isPreview = value;
                OnPropertyChanged();
            }
        }
        private bool _isPreview;

        /// <summary>
        /// Determines whether the editor is initially focused
        /// </summary>
        public bool NoInitialFocus { get; set; }

        /// <summary>
        /// Determines if the editor requires an HTML preview window
        /// </summary>
        public bool HasHtmlPreview { get; set; }

  


        /// <summary>
        /// Optional identifier that lets you specify what type of
        /// document we're dealing with.
        /// 
        /// Can be used by Addins to create customer editors or handle
        /// displaying the document a different way.
        /// </summary>
        public string Identifier { get; set; } = "MarkdownDocument";

        /// <summary>
        /// Optional storage object that allows you to store additional data
        /// for the document. Useful for plug-ins that may want to keep things
        /// with the document for rendering or other purposes.
        /// </summary>
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();


     

        #endregion


        public EditorAndPreviewPane EditorPreviewPane { get; private set; }

        #region Loading And Initialization
        public MarkdownDocumentEditor()
        {

            Window = mmApp.Model.Window;

            EditorPreviewPane = new EditorAndPreviewPane();

            WebBrowser = EditorPreviewPane.EditorWebBrowser;            
            WebBrowser.Visibility = Visibility.Hidden;
            WebBrowser.Navigating += WebBrowser_NavigatingAndDroppingFiles;

                    
            // Remove preview browser from old parent if there is one
            ((Grid) Window.PreviewBrowserContainer.Parent)?.Children.Remove(Window.PreviewBrowserContainer);

            // add it to the current editor
            Window.PreviewBrowserContainer.SetValue(Grid.ColumnProperty, 2);

            Window.Model.WindowLayout.IsPreviewVisible = mmApp.Configuration.IsPreviewVisible;

            // add the previewer
            EditorPreviewPane.ContentGrid.Children.Add(Window.PreviewBrowserContainer);
        }
        
        /// <summary>
        /// Loads a new document into the active editor using 
        /// MarkdownDocument instance.
        /// </summary>
        /// <param name="mdDoc"></param>
        public void LoadDocument(MarkdownDocument mdDoc = null, bool forceReload = false)
        {            
            if (mdDoc != null)
                MarkdownDocument = mdDoc;

            if (AceEditor == null)
            {
                WebBrowser.LoadCompleted += OnDocumentCompleted;
                WebBrowser.Navigate(new Uri(Path.Combine(Environment.CurrentDirectory, "Editor\\editor.htm")));
                //WebBrowser.Navigate("http://localhost:8080/editor.htm");
            }
            else if(forceReload)
                WebBrowser.Navigate(new Uri(Path.Combine(Environment.CurrentDirectory, "Editor\\editor.htm")));

            EditorSyntax = mmFileUtils.GetEditorSyntaxFromFileType(MarkdownDocument.Filename);
        }


        private void OnDocumentCompleted(object sender, NavigationEventArgs e)
        {
            if (AceEditor == null)
            {
                // Get the JavaScript Ace Editor Instance
                dynamic doc = WebBrowser.Document;
                var window = doc.parentWindow;

                try
                {
                    AceEditor = window.initializeinterop(this);                 
                }
                catch (Exception ex)
                {
                    mmApp.Log($"Editor InitializeInterop failed: {e.Uri}", ex);
                    //throw;
                }

                if (EditorSyntax != "markdown")
					AceEditor?.setlanguage(EditorSyntax);

                RestyleEditor(true);

                SetShowLineNumbers(mmApp.Configuration.Editor.ShowLineNumbers);
                SetShowInvisibles(mmApp.Configuration.Editor.ShowInvisibles);
                SetReadOnly(IsReadOnly);

                if (!NoInitialFocus)
                    SetEditorFocus();
                
                if (InitialLineNumber > 0)
                    GotoLine(InitialLineNumber);                

                WebBrowser.Visibility = Visibility.Visible;
            }
            SetMarkdown();            
        }

        #endregion

        #region Markdown Access and Manipulation

        /// <summary>
        /// Sets the markdown text into the editor control
        /// </summary>
        /// <param name="markdown"></param>
        public void SetMarkdown(string markdown = null, object position = null, bool updateDirtyFlag = false)
        {
            if (MarkdownDocument != null)
            {
                if (string.IsNullOrEmpty(markdown))
                    markdown = MarkdownDocument.CurrentText;
                else if (markdown != MarkdownDocument.CurrentText)
                     SetDirty(true);                
            }
            if (AceEditor != null)
            {
                if (position == null)
                    position = -2; // keep position
                AceEditor.setvalue(markdown ?? string.Empty, position);
            }

            if (updateDirtyFlag)
                IsDirty();
        }

        /// <summary>
        /// Reads the markdown text from the editor control
        /// </summary>
        /// <returns></returns>
        public string GetMarkdown()
        {
            if (AceEditor == null)
                return "";

            MarkdownDocument.CurrentText =  AceEditor.getvalue(false);
            return MarkdownDocument.CurrentText;
        }

        /// <summary>
        /// Renders Markdown as HTML
        /// </summary>
        /// <param name="markdown">Markdown text to turn into HTML</param>
        /// <param name="renderLinksExternal">If true creates all links with target='top'</param>
        /// <returns></returns>
        public string RenderMarkdown(string markdown, bool renderLinksExternal = false, bool usePragmaLines = false)
        {
            return MarkdownDocument.RenderHtml(markdown, renderLinksExternal, usePragmaLines);
        }

        /// <summary>
        /// Saves the active document to file using the filename
        /// defined on the MarkdownDocument.
        /// 
        /// If there's no active filename a file save dialog
        /// If there's no active filename a file save dialog
        /// is popped up. 
        /// </summary>
        /// <param name="isEncrypted">Determines if the file is using local encryption</param>        
        public bool SaveDocument(bool isEncrypted = false)
        {            
            if (MarkdownDocument == null || AceEditor == null || 
               !AddinManager.Current.RaiseOnBeforeSaveDocument(MarkdownDocument))
                return false;
            
            GetMarkdown();
            
            if (isEncrypted && MarkdownDocument.Password == null)
            {
                var pwdDialog = new FilePasswordDialog(MarkdownDocument, true)
                {
                    Owner = Window
                };
                bool? pwdResult = pwdDialog.ShowDialog();
                if (pwdResult == false)
                {
                    Window.ShowStatus("Encrypted document not opened, due to missing password.",
                        mmApp.Configuration.StatusMessageTimeout);
                    return false;
                }
            }


            if (!MarkdownDocument.Save())
                return false;

            AddinManager.Current.RaiseOnAfterSaveDocument(MarkdownDocument);

            AceEditor.isDirty = false;

            // reload settings if we were editing the app config file.
            var justfile = Path.GetFileName(MarkdownDocument.Filename).ToLower();
            if (justfile == "markdownmonster.json")
            {
                mmApp.Configuration.Read();

                mmApp.SetTheme(mmApp.Configuration.ApplicationTheme,Window);
                mmApp.SetThemeWindowOverride(Window);
                
                foreach (TabItem tab in Window.TabControl.Items)
                {
                    var editor = tab.Tag as MarkdownDocumentEditor;
                    if (editor == null) continue;
                    editor.RestyleEditor();
                    editor.AceEditor?.setShowLineNumbers(mmApp.Configuration.Editor.ShowLineNumbers);
                }
            }

            return true;
        }

        public struct MarkupMarkdownResult
        {
            public string Html;
            public int CursorMovement;
        }

        /// <summary>
        /// Wraps a string with beginning and ending delimiters.
        /// Fixes up accidental leading and trailing spaces.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="delim1"></param>
        /// <param name="delim2"></param>
        /// <param name="stripSpaces"></param>
        /// <returns></returns>
        public string wrapValue(string input, string delim1, string delim2, bool stripSpaces = true)
        {
            if (!stripSpaces)
                return delim1 + input + delim2;

            if (input.StartsWith(" "))
                input = " " + delim1 + input.TrimStart();
            else
                input = delim1 + input;

            if (input.EndsWith(" "))
                input = input.TrimEnd() + delim2 + " ";
            else
                input += delim2;

            return input;
        }
        #endregion

       


        #region Editor Selection Replacements and Insertions
        /// <summary>
        /// Takes action on the selected string in the editor using
        /// predefined commands.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="input"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public MarkupMarkdownResult MarkupMarkdown(string action, string input, string style = null)
        {
            var result = new MarkupMarkdownResult();

            if (string.IsNullOrEmpty(action))
            {
                result.Html = input;
                return result;
            }

            action = action.ToLower();

            // check for insert actions that don't require a pre selection
            //if (string.IsNullOrEmpty(input) && !StringUtils.Inlist(action,
            //    new string[] {
            //        "image", "href", "code", "emoji",
            //        "h1", "h2", "h3", "h4", "h5",

            //    }))
            //    return null;

            string html = input;
            int cursorMovement = 0;

            if (action == "bold")
            {
                html = wrapValue(input, "**", "**", stripSpaces: true);
                cursorMovement = -2;
            }
            else if (action == "italic")
            {
                html = wrapValue(input, "*", "*", stripSpaces: true);
                cursorMovement = -1;
            }
            else if (action == "small")
            {
                // :-( no markdown spec for this - use HTML
                html = wrapValue(input, "<small>", "</small>", stripSpaces: true);
                cursorMovement = -7;
            }
            else if (action == "underline")
            {
                // :-( no markdown spec for this - use HTML
                html = wrapValue(input, "<u>", "</u>", stripSpaces: true);
                cursorMovement = -4;
            }
            else if (action == "strikethrough")
            {
                html = wrapValue(input, "~~", "~~", stripSpaces: true);
                cursorMovement = -2;
            }
            else if (action == "inlinecode")
            {
                html = wrapValue(input, "`", "`", stripSpaces: true);
                cursorMovement = -1;
            }
            else if (action == "h1")
                html = "# " + input;
            else if (action == "h2")
                html = "## " + input;
            else if (action == "h3")
                html = "### " + input;
            else if (action == "h4")
                html = "#### " + input;
            else if (action == "h5")
                html = "##### " + input;
            else if (action == "h6")
                html = "###### " + input;

            else if (action == "quote")
            {
                StringBuilder sb = new StringBuilder();
                var lines = StringUtils.GetLines(input);
                foreach (var line in lines)
                {
                    sb.AppendLine("> " + line);
                }
                html = sb.ToString();

                if (string.IsNullOrEmpty(input))
                    html = html.TrimEnd() + " ";  // strip off LF
            }
            else if (action == "list")
            {
                StringBuilder sb = new StringBuilder();
                var lines = StringUtils.GetLines(input);
                foreach (var line in lines)
                {
                    sb.AppendLine("* " + line);
                }
                html = sb.ToString();

                if (string.IsNullOrEmpty(input))
                    html = html.TrimEnd() + " ";  // strip off LF
            }
            else if (action == "numberlist")
            {
                StringBuilder sb = new StringBuilder();
                var lines = StringUtils.GetLines(input);
                int ct = 0;
                foreach (var line in lines)
                {
                    ct++;
                    sb.AppendLine($"{ct}. " + line);
                }
                html = sb.ToString();

                if (string.IsNullOrEmpty(input))
                    html = html.TrimEnd() + " ";  // strip off LF
            }
            else if (action == "table")
            {
                var form = new TableEditor();
                form.Owner = Window;
                form.ShowDialog();

                if (!form.Cancelled)
                    html = form.TableHtml;
            }

            else if (action == "emoji")
            {
                var form = new EmojiWindow();
                form.Owner = Window;
                form.ShowDialog();

                if (!form.Cancelled)
                    html = form.EmojiString;
            }
            else if (action == "href")
            {
                var form = new PasteHref()
                {
                    Owner = Window,
                    LinkText = input,
                    MarkdownFile = MarkdownDocument.Filename
                };

                // check for links in input or on clipboard
                string link = input;
                if (string.IsNullOrEmpty(link))
                    link = Clipboard.GetText();

                if (!(input.StartsWith("http:") || input.StartsWith("https:") || input.StartsWith("mailto:") || input.StartsWith("ftp:")))
                    link = string.Empty;
                form.Link = link;

                bool? res = form.ShowDialog();
                if (res != null && res.Value)
                {
                    if (form.IsExternal)
                        html = $"<a href=\"{form.Link}\" target=\"_blank\">{form.LinkText}</a>";
                    else
                        html = $"[{form.LinkText}]({form.Link})";
                }
            }
            else if (action == "image")
            {
                var form = new PasteImageWindow(Window)
                {
                    ImageText = input,
                    MarkdownFile = MarkdownDocument.Filename
                };


                // check for links in input or on clipboard
                string link = input;
                if (string.IsNullOrEmpty(link))
                    link = Clipboard.GetText();

                if (!(input.StartsWith("http:") || input.StartsWith("https:") || input.StartsWith("mailto:") || input.StartsWith("ftp:")))
                    link = string.Empty;

                if (input.Contains(".png") || input.Contains(".jpg") || input.Contains(".gif"))
                    link = input;

                form.Image = link;

                bool? res = form.ShowDialog();
                if (res != null && res.Value && form.Image != null)
                {
                    var image = form.Image;
                    if (!image.StartsWith("data:image/"))
                        html = $"![{form.ImageText}]({image.Replace(" ", "%20")})";
                    else
                    {
                        var id = "image_ref_" + DataUtils.GenerateUniqueId();

                        dynamic pos = AceEditor.getCursorPosition(false);
                        dynamic scroll = AceEditor.getscrolltop(false);

                        // the ID tag
                        html = $"\r\n\r\n[{id}]: {image}\r\n";

                        // set selction position to bottom of document
                        AceEditor.gotoBottom(false);
                        SetSelection(html);

                        // reset the selection point
                        AceEditor.setcursorposition(pos); //pos.column,pos.row);

                        if (scroll != null)
                            AceEditor.setscrolltop(scroll);

                        WindowUtilities.DoEvents();
                        html = $"![{form.ImageText}][{id}]";

                        // Force a refresh of the window
                        Window.PreviewBrowser.Refresh(true);
                    }
                }
            }
            else if (action == "code")
            {
                var form = new PasteCode();
                form.Owner = Window;
                if (!string.IsNullOrEmpty(input))
                    form.Code = input;
                else
                {
                    // use clipboard text if we think it contains code
                    string clipText = Clipboard.GetText(TextDataFormat.Text);
                    if (!string.IsNullOrEmpty(clipText) &&
                        clipText.Contains("{") ||
                        clipText.Contains("/>") ||
                        clipText.Contains("="))
                        form.Code = clipText;
                }

                form.CodeLanguage = mmApp.Configuration.DefaultCodeSyntax;
                bool? res = form.ShowDialog();

                if (res != null && res.Value && !string.IsNullOrEmpty(form.Code))
                {
                    html = "```" + form.CodeLanguage + "\r\n" +
                           form.Code.Trim() + "\r\n" +
                           "```\r\n";
                }
            }
            else
            {
                // allow addins to handle custom actions
                string addinAction = AddinManager.Current.RaiseOnEditorCommand(action, input);
                if (!string.IsNullOrEmpty(addinAction))
                    html = addinAction;
            }


            if (string.IsNullOrEmpty(input))
                result.CursorMovement = cursorMovement;
            result.Html = html;

            return result;
        }

        /// <summary>
        /// Takes a command  like bold,italic,href etc., reads the
        /// text from editor selection, transforms it and pastes
        /// it back into the document.
        /// </summary>
        /// <param name="action"></param>
        public void ProcessEditorUpdateCommand(string action)
        {
            if (AceEditor == null)
                return;

            string html = AceEditor.getselection(false);

            var result = MarkupMarkdown(action, html);


            if (!string.IsNullOrEmpty(result.Html) && html != result.Html)
                SetSelectionAndFocus(result.Html);
            else
                SetEditorFocus();

            if (result.CursorMovement != 0)
                MoveCursorPosition(result.CursorMovement);
        }

        /// <summary>
        /// Fired from Editor Menu when items are selected
        /// </summary>
        /// <param name="action"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public string EditorSelectionOperation(string action, string text)
        {
            if (action == "image")
            {
                string label = StringUtils.ExtractString(text, "![", "]");
                string link = StringUtils.ExtractString(text, "](", ")");

                var form = new PasteImageWindow(Window)
                {
                    Image = link,
                    ImageText = label,
                    MarkdownFile = MarkdownDocument.Filename
                };
                form.SetImagePreview();

                bool? res = form.ShowDialog();
                string html = null;

                if (res != null && res.Value)
                {
                    var image = form.Image;
                    if (!image.StartsWith("data:image/"))
                        html = $"![{form.ImageText}]({image.Replace(" ", "%20")})";
                    else
                    {
                        var id = "image_ref_" + DataUtils.GenerateUniqueId();

                        dynamic pos = AceEditor.getCursorPosition(false);
                        dynamic scroll = AceEditor.getscrolltop(false);

                        // the ID tag
                        html = $"\r\n\r\n[{id}]: {image}\r\n";

                        // set selction position to bottom of document
                        AceEditor.gotoBottom(false);
                        SetSelection(html);

                        // reset the selection point
                        AceEditor.setcursorposition(pos); //pos.column,pos.row);

                        if (scroll != null)
                            AceEditor.setscrolltop(scroll);

                        WindowUtilities.DoEvents();
                        html = $"![{form.ImageText}][{id}]";
                    }

                    if (!string.IsNullOrEmpty(html))
                    {
                        SetSelection(html);
                        PreviewMarkdownCallback();

                        // Force the browser to refresh so the image gets updated.
                        Window.PreviewBrowser.Refresh(true);
                    }
                    
                }
            }
            else if (action == "hyperlink")
            {
                string label = StringUtils.ExtractString(text, "[", "]");
                string link = StringUtils.ExtractString(text, "](", ")");

                var form = new PasteHref()
                {
                    Owner = Window,
                    Link = link,
                    LinkText = label,
                    MarkdownFile = MarkdownDocument.Filename
                };

                bool? res = form.ShowDialog();
                if (res != null && res.Value)
                {
                    string html = $"[{form.LinkText}]({form.Link})";
                    if (!string.IsNullOrEmpty(html))
                    {
                        SetSelection(html);
                        PreviewMarkdownCallback();
                    }
                }

            }
            else if (action == "table")
            {
                var form = new TableEditor(text);
                var res = form.ShowDialog();

                if (res != null && res.Value)
                {
                    if (!string.IsNullOrEmpty(form.TableHtml))
                    {
                        SetSelection(form.TableHtml.TrimEnd() + "\r\n");
                        PreviewMarkdownCallback();
                    }
                }
            }
            else if (action == "code")
            {
                MessageBox.Show("Link to Edit", text);
            }
            return null;
        }

        public void ExecEditorCommand(string action, object parm = null)
        {
            if (action == null)
                return;

            AceEditor?.execcommand(action, parm);
        }

        #endregion


        #region Get and Set Document Properties

        public bool IsPreviewToEditorSync()
        {
            var mode = mmApp.Configuration.PreviewSyncMode;
            if (mode == PreviewSyncMode.PreviewToEditor || mode == PreviewSyncMode.EditorAndPreview)
                return true;

            return false;
        }


        /// <summary>
        /// Sets the Syntax language to highlight for in the editor
        /// </summary>
        /// <param name="syntax"></param>
        public void SetEditorSyntax(string syntax = "markdown")
        {
            AceEditor?.setlanguage(syntax);
        }


        /// <summary>
        /// Returns the font size of the editor. Note font-size automatically
        /// affects all open editor instances .
        /// </summary>
        /// <returns></returns>        
        public int GetFontSize()
        {
            if (AceEditor == null)
                return 0;

            object fontsize = AceEditor.getfontsize(false);
            if (fontsize == null || !(fontsize is double || fontsize is int) )
                return 0;

            return Convert.ToInt32(fontsize);

            //// If we have a fontsize, force the zoom level to 100% 
            //// The font-size has been adjusted to reflect the zoom percentage
            //var wb = (dynamic)WebBrowser.GetType().GetField("_axIWebBrowser2",
            //        BindingFlags.Instance | BindingFlags.NonPublic)
            //    .GetValue(WebBrowser);
            //int zoomLevel = 100; // Between 10 and 1000
            //wb.ExecWB(63, 2, zoomLevel, ref zoomLevel);   // OLECMDID_OPTICAL_ZOOM (63) - don't prompt (2)                     
        }
		

        /// <summary>
        /// Restyles the current editor with configuration settings
        /// from the mmApp.Configuration object (or Model.Configuration
        /// from an addin).
        /// </summary>
        /// <param name="forceSync">Forces higher priority on this operation - use when editor initializes at first</param>
        public void RestyleEditor(bool forceSync = false)
        {
            if (AceEditor == null)
                return;

            Window.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        // determine if we want to rescale the editor fontsize
                        // based on DPI Screen Size
                        decimal dpiRatio = 1;
                        try
                        {
                            dpiRatio = WindowUtilities.GetDpiRatio(Window);
                        }
                        catch
                        {
                        }

                        var fontSize = mmApp.Configuration.Editor.FontSize *  ((decimal) mmApp.Configuration.Editor.ZoomLevel / 100) * dpiRatio;
                        
                        var config = mmApp.Configuration;

                        var style = new
                        {
                            Theme = config.EditorTheme,
                            Font = config.Editor.Font,
                            FontSize = (int)fontSize,
                            RightToLeft = config.Editor.RightToLeft,
                            LineHeight = config.Editor.LineHeight,
                            WrapText = config.Editor.WrapText,
                            ShowLineNumbers = config.Editor.ShowLineNumbers,
                            ShowInvisibles = config.Editor.ShowInvisibles,
                            HighlightActiveLine = config.Editor.HighlightActiveLine,
                            KeyboardHandler = config.Editor.KeyboardHandler,
                            EnableBulletAutoCompletion = config.Editor.EnableBulletAutoCompletion  
                        };

                        var jsonStyle = JsonConvert.SerializeObject(style);
                        AceEditor.setEditorStyle(jsonStyle);

                        if (EditorSyntax == "markdown" || EditorSyntax == "text")
                            AceEditor.enablespellchecking(!mmApp.Configuration.Editor.EnableSpellcheck,
                                mmApp.Configuration.Editor.Dictionary);
                        else
                            // always disable for non-markdown text
                            AceEditor.enablespellchecking(true, mmApp.Configuration.Editor.Dictionary);
                    }
                    catch
                    {
                    }
                },
                forceSync
                    ? System.Windows.Threading.DispatcherPriority.Send
                    : System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

     

        /// <summary>
        /// Sets line number gutter on and off. Separated out from Restyle Editor to 
        /// allow line number config to be set separately from main editor settings
        /// for specialty file editing.
        /// </summary>
        /// <param name="show"></param>
        public void SetShowLineNumbers(bool? show = null)
        {
            if (show == null)
                show = mmApp.Configuration.Editor.ShowLineNumbers;

            AceEditor?.setShowLineNumbers(show.Value);
        }

        /// <summary>
        /// Enables or disables the display of invisible characters.
        /// </summary>
        /// <param name="show"></param>
        public void SetShowInvisibles(bool? show = null)
        {
            if (show == null)
                show = mmApp.Configuration.Editor.ShowInvisibles;

            AceEditor?.setShowInvisibles(show.Value);
        }

        /// <summary>
        /// Makes the document readonly or read-write
        /// 
        /// Fires event when ReadOnly document is double clicked:
        /// OnNotifyAddin("ReadOnlyEditorDoubleClick",editor) 
        /// </summary>
        /// <param name="show"></param>
        public void SetReadOnly(bool show = true)
        {
            AceEditor?.setReadOnly(show);
        }

        /// <summary>
        /// Enables or disables Wordwrap
        /// </summary>
        /// <param name="enable"></param>
        public void SetWordWrap(bool enable)
        {
            AceEditor?.setWordWrap(enable);
        }
        #endregion

        #region Properties Collection Helpers


        /// <summary>
        /// Returns a value from the Properties Collection as a sepcific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        public T GetProperty<T>(string key)
        {
            if (Properties.TryGetValue(key, out object obj))
                return (T)obj;

            return default(T);
        }
        /// <summary>
        /// Returns a value from the Properties Collection as a sepcific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetProperty<T>(string key, T defaultValue)
        {
            if (Properties.TryGetValue(key, out object obj))
                return (T)obj;

            return defaultValue;
        }


        /// <summary>
        /// Returns a Property from the Properties collection as a string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetPropertyString(string key, string defaultValue = null)
        {
            if (Properties.TryGetValue(key, out object obj))
                return obj as string;

            return defaultValue;
        }
        #endregion

        #region Selection and Line Operations

        /// <summary>
        /// Replaces the editor's content without completely 
        /// reloading the document. 
        /// 
        /// Leaves scroll position intact.
        /// </summary>
        /// <param name="text"></param>
        public void ReplaceContent(string text)
        {
            AceEditor?.replacecontent(text);
        }


        /// <summary>
        /// Gets the current selection of the editor
        /// </summary>
        /// <returns></returns>
        public string GetSelection()
        {
            return AceEditor?.getselection(false);
        }

        /// <summary>
        /// Pastes text into the editor at the current 
        /// insertion/selection point. Replaces any 
        /// selected text.
        /// </summary>
        /// <param name="text"></param>
        public void SetSelection(string text)
        {
            if (AceEditor == null)
                return;

            AceEditor.setselection(text);
            IsDirty();
            MarkdownDocument.CurrentText = GetMarkdown();
        }

        /// <summary>
        /// Sets selection, sets focus to the editor and
        /// refreshes the preview
        /// </summary>
        /// <param name="text"></param>
        public void SetSelectionAndFocus(string text)
        {
            if (AceEditor == null)
                return;

            SetSelection(text);
            SetEditorFocus();

            Window.PreviewMarkdown(this, keepScrollPosition: true);                       
        }


        /// <summary>
        /// Retrieves the text of the current line as a string.
        /// </summary>
        /// <returns></returns>
        public string GetCurrentLine()
        {
            return AceEditor?.getCurrentLine(false);
        }

        /// <summary>
        /// Retrieves the text for the given line
        /// </summary>
        /// <param name="rowNumber"></param>
        /// <returns></returns>
        public string GetLine(int rowNumber)
        {
            try
            {
                return AceEditor?.getLine(rowNumber);
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Gets the active line number of the editor
        /// </summary>
        /// <returns></returns>
        public int GetLineNumber()
        {
            if (AceEditor == null)
                return -1;

            try
            {
                int lineNo = AceEditor.getLineNumber(false);
                return lineNo;
            }
            catch
            {
                return -1;
            }
        }


        /// <summary>
        /// Goes to the specified line number in the editor
        /// </summary>
        /// <param name="line">Editor Line to display</param>
        /// <param name="noRefresh">Won't refresh the preview after setting</param>
        /// <param name="noSelection">Only scroll but don't select</param>
        public void GotoLine(int line, bool noRefresh = false, bool noSelection = false)
        {
            if (line < 0)
                line = 0;

            try
            {
                AceEditor?.gotoLine(line, noRefresh, noSelection);
            }
            catch { }
        }

        public void FindAndReplaceTextInCurrentLine(string search, string replace)
        {
            AceEditor?.findAndReplaceTextInCurrentLine(search, replace);
        }


        /// <summary>
        /// REturns the editor's cursor position as row and column values
        /// </summary>
        /// <returns></returns>
        public AcePosition GetCursorPosition()
        {
            dynamic pos = AceEditor.getCursorPosition(false);
            if (pos == null)
                return new AcePosition { row = -1, column = -1 };
            var pt = new AcePosition()
            {
                row = (int) pos.row,
                column = (int) pos.column
            };
            return pt;
        }


        /// <summary>
        /// Set's the editor's row and column position
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public void SetCursorPosition(int col, int row)
        {
            AceEditor?.setCursorPosition(row,col);            
        }

        /// <summary>
        /// Set's the editor's row and column position
        /// </summary>
        public void SetCursorPosition(AcePosition pos)
        {
            AceEditor?.setCursorPosition(pos.row, pos.column);
        }

        public void MoveCursorPosition(int column, int row = 0)
        {
            if (column > 0)
                AceEditor.moveCursorRight(column);
            else if (column < 0)
                AceEditor.moveCursorLeft(column * -1);
        }

        public void SetSelectionRange(AcePosition start, AcePosition end)
        {
            AceEditor?.SetSelectionRange(start.row, start.column, end.row, end.column);
        }
        public void SetSelectionRange(int startRow, int startColumn, int endRow, int endColumn)
        {
            AceEditor?.SetSelectionRange(startRow, startColumn, endRow, endColumn);
        }

        /// <summary>
        /// Returns the editor's vertical scroll position
        /// </summary>
        /// <returns></returns>
        public int GetScrollPosition()
        {
            if (AceEditor == null)
                return -1;

            try
            {
                object st = AceEditor.getscrolltop(false);
                int scrollTop = (int)st;
                return scrollTop;
            }
            catch
            {
                return -1;
            }
            
        }

        /// <summary>
        ///  Sets the vertical scroll position of the document
        /// </summary>
        /// <param name="top"></param>
        public void  SetScrollPosition(int top)
        {
            if (AceEditor == null)
                return;

            try
            {
                AceEditor.setscrolltop(top);                
            }
            catch
            { }
        }


        /// <summary>
        /// Removes markdown formatting from the editor selection.
        /// Non-markdown files don't do anything.
        /// </summary>
        public bool RemoveMarkdownFormatting()
        {            
            if (EditorSyntax != "markdown")
                return false;

            var markdown = GetSelection();

            if (string.IsNullOrEmpty(markdown))
                return false;

            var text = Markdown.ToPlainText(markdown);
            SetSelectionAndFocus(text);

            return true;
        }
        #endregion

        #region Editor Focus and Sizing
        /// <summary>
        /// Focuses the Markdown editor in the Window
        /// </summary>
        public void SetEditorFocus()
        {            
            AceEditor?.setfocus(true);
        }


        /// <summary>
        /// Force focus away from the Markdown Editor by focusing
        /// on one of the controls in the Window
        /// </summary>
        public void SetMarkdownMonsterWindowFocus()
        {
            Window.ComboBoxPreviewSyncModes.Focus();            
        }

        public void ResizeWindow()
        {
            // nothing to do at the moment             
        }
        #endregion

        #region Callback functions from the Html Editor


        /// <summary>
        /// Sets the Markdown Document as having changes
        /// </summary>
        /// <param name="value">ignored</param>
        public bool SetDirty(bool value)
        {
             GetMarkdown();

            if (IsReadOnly)
                return false;

            if (value && MarkdownDocument.CurrentText != MarkdownDocument.OriginalText)
                MarkdownDocument.IsDirty = true;
            else
                MarkdownDocument.IsDirty = false;

            return MarkdownDocument.IsDirty;
        }


        /// <summary>
        /// Determines whether current document is dirty and requires saving
        /// </summary>
        /// <returns></returns>
        public bool IsDirty()
        {
            GetMarkdown();

            if (IsReadOnly)
                return false;

            MarkdownDocument.IsDirty = MarkdownDocument.CurrentText != MarkdownDocument.OriginalText;

            if (MarkdownDocument.IsDirty)
            {
                AddinManager.Current.RaiseOnDocumentChanged();
                if (IsPreview)
                {
                    // flip to regular tab from preview Tab
                    IsPreview = false;
                    Window.PreviewTab = null;
                }
            }

            return MarkdownDocument.IsDirty;
        }


        /// <summary>
        /// Displays a message box
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <param name="icon"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public string ShowMessage(string text, string title, string icon = "Information", string buttons = "Ok")
        {
            var image = MessageBoxImage.Information;
            Enum.TryParse<MessageBoxImage>(icon, out image);

            var btn = MessageBoxButton.OK;
            Enum.TryParse<MessageBoxButton>(buttons, out btn);

            var res = MessageBox.Show(Window,text, title, btn, image);
            return res.ToString();
        }

        /// <summary>
        /// Allows the client to show status messages
        /// </summary>
        /// <param name="text"></param>
        /// <param name="timeoutms"></param>
        public void ShowStatus(string text, int timeoutms = 0)
        {
            Window.ShowStatus(text, timeoutms);
        }
        

        
        /// <summary>
        /// Callback handler callable from JavaScript editor
        /// </summary>
        public void PreviewMarkdownCallback(bool dontGetMarkdown = false, int editorLineNumber = -1)
        {
            if (!dontGetMarkdown)
                GetMarkdown();
            
            Window.PreviewBrowser.PreviewMarkdownAsync(keepScrollPosition: true, editorLineNumber: editorLineNumber);

            Window.UpdateDocumentOutline(editorLineNumber);

            WindowUtilities.DoEvents();
        }

        /// <summary>
        /// Callback to force updating of the status bar document stats
        /// </summary>
        /// <param name="stats"></param>
        public void UpdateDocumentStats(dynamic stats)
        {
            if (stats == null)
            {
                Window.StatusStats.Text = "";
                return;
            }

            int words = Convert.ToInt32(stats.wordCount);
            int lines = Convert.ToInt32(stats.lines);
            int chars = Convert.ToInt32(stats.characters);

            Window.StatusStats.Text = $"{words:n0} words   {lines:n0} lines   {chars:n0} chars" ;
            
            string enc = string.Empty;
            bool hasBom = true;
            if (MarkdownDocument.Encoding.WebName == "utf-8")
                hasBom = (bool)ReflectionUtils.GetField(MarkdownDocument.Encoding, "emitUtf8Identifier");

            if (hasBom)
            {
                enc = MarkdownDocument.Encoding.EncodingName;
                if (MarkdownDocument.Encoding == Encoding.UTF8)
                    enc = "UTF-8";
            }
            else
                enc = "UTF-8 (no BOM)";

            Window.StatusEncoding.Text = enc;

        }

        /// <summary>
        /// Performs the special key operation that is tied
        /// to the key in the application.
        /// 
        /// ctrl-s,ctrl-n, ctrl-o, cltr-i,ctrl-b,ctrl-l,ctrl-k,alt-c,ctrl-shift-v,ctrl-shift-c,ctlr-shift-down,ctrl-shift-up
        /// </summary>
        /// <param name="key"></param>
        public void KeyboardCommand(string key)
        {

            // run this one sync to avoid Browser default Open File popup!
            if (key == "OpenDocument")
            {
                Window.Model.Commands.OpenDocumentCommand.Execute(Window);
                return;
            }

            // invoke out of sync in order to force out of scope of the editor - affects weird key behavior otherwise
            Window.Dispatcher.InvokeAsync(() =>
            {
                if (key == "SaveDocument")
                {
                    Window.Model.Commands.SaveCommand.Execute(Window);
                }
                else if (key == "NewDocument")
                {
                    Window.Model.Commands.NewDocumentCommand.Execute(Window);
                }
                
                else if (key == "PrintPreview")
                {
                    Window.Model.Commands.PrintPreviewCommand.Execute(Window.ButtonPrintPreview);
                }                
                else if (key == "InsertBold")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("bold");
                }
                else if (key == "InsertItalic")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("italic");
                }
                else if (key == "InsertInlineCode")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("inlinecode");
                }
                else if (key == "InsertList")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("list");
                }
                else if (key == "InsertEmoji")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("emoji");
                }
                else if (key == "InsertHyperlink")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("href");
                }
                else if (key == "InsertImage")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("image");
                }
                else if (key == "InsertCodeblock")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("code");
                }
                else if (key == "PasteHtmlAsMarkdown")
                {
                    Window.Model.Commands.PasteMarkdownFromHtmlCommand.Execute(null);
                }
                else if (key == "CopyMarkdownAsHtml")
                {
                    Window.Model.Commands.CopyAsHtmlCommand.Execute(null);
                }
                else if (key == "RemoveMarkdownFormatting")
                {
                    Window.Model.Commands.RemoveMarkdownFormattingCommand.Execute(null);
                }
                else if (key == "ReloadEditor")
                {
                    if (IsDirty())
                    {
                        if (MessageBox.Show(
                            "You have changes in this document that have not been saved.\r\n\r\n" +
                            "Are you sure you want to reload the document from disk?",
                            "Refresh Document", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
                            return;
                    }

                    AceEditor = null;
                    MarkdownDocument.Load();
                    LoadDocument(MarkdownDocument,true);                    
                    PreviewMarkdownCallback();
                    SetEditorFocus();
                    IsDirty(); // refresh dirty flag
                }
                else if (key == "NextTab")
                {
                    var tab = Window.TabControl.SelectedItem;
                    var tabs = Window.TabControl.GetOrderedHeaders().ToList();                    
                    var selIndex = 0;
                    bool found = false;
                    foreach (var t in tabs)
                    {
                        selIndex++;
                        if (t.Content == tab)
                        {
                            found = true;                         
                            if (selIndex >= tabs.Count)
                                selIndex = 0;
                            break;
                        }                        
                    }
                    if (!found)
                        return;
                    
                    Window.TabControl.SelectedItem = tabs[selIndex].Content;
                }

                else if (key == "PreviousTab")
                {
                    var tab = Window.TabControl.SelectedItem;
                    var tabs = Window.TabControl.GetOrderedHeaders().ToList();
                    var selIndex = 0;
                    bool found = false;
                    foreach (var t in tabs)
                    {                        
                        if (t.Content == tab)
                        {
                            found = true;
                            selIndex--;
                            if (selIndex < 0)
                                selIndex = tabs.Count-1;                            
                            break;
                        }
                        selIndex++;
                    }
                    if (!found)
                        return;

                    Window.TabControl.SelectedItem = tabs[selIndex].Content;
                }
                // zooming
                else if (key == "ZoomEditorUp")
                {
                    mmApp.Configuration.Editor.ZoomLevel += 2;
                    RestyleEditor();
                }
                else if (key == "ZoomEditorDown")
                {
                    mmApp.Configuration.Editor.ZoomLevel -= 2;                    
                    RestyleEditor();
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }


        public void FindAndReplaceText(string search, string replace)
        {
            AceEditor?.findAndReplaceText(search, replace);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns>true if the navigation is handled, false to continue letting app handle navigation</returns>
        public bool PreviewLinkNavigation(string url, string src)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            if (AddinManager.Current.RaiseOnPreviewLinkNavigation(url,src))
                return true;

            if (url.StartsWith("http",StringComparison.InvariantCultureIgnoreCase))
            {
                if (mmApp.Configuration.PreviewHttpLinksExternal && !string.IsNullOrEmpty(url))
                {
                    ShellUtils.GoUrl(url);
                    return true;
                }
            }
            // it's a relative URL and ends with .md open in editor
            else if (url.EndsWith(".md",StringComparison.InvariantCultureIgnoreCase))
            {
                // file urls are fully qualified paths with file:/// syntax
                var urlPath = url.Replace("file:///", "");
                urlPath = StringUtils.UrlDecode(urlPath);
                urlPath = FileUtils.NormalizePath(urlPath);
                if (File.Exists(urlPath))
                {
                    var tab = Window.RefreshTabFromFile(urlPath); // open or activate
                    if (tab != null)
                        return true;
                }

                var docPath = Path.GetDirectoryName(MarkdownDocument.Filename);
                urlPath = Path.Combine(docPath,urlPath);
                if (File.Exists(urlPath))
                {
                    var tab = Window.RefreshTabFromFile(docPath); // open or activate
                    if (tab != null)
                        return true;
                }
            }
            
            // default browser behavior
            return false;
        }

        /// <summary>
        /// Handle pasting and handle images
        /// </summary>
        public void PasteOperation()
        {
            if (ClipboardHelper.ContainsImage())
            {
                string imagePath = null;

                using (var bitMap = System.Windows.Forms.Clipboard.GetImage())
                {
                    imagePath = AddinManager.Current.RaiseOnSaveImage(bitMap);

                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        SetSelection($"![]({imagePath.Replace(" ","%20")})");
                        PreviewMarkdownCallback(); // force a preview refresh
                        return;
                    }

                    string initialFolder = null;
                    if (!string.IsNullOrEmpty(MarkdownDocument.Filename) && MarkdownDocument.Filename != "untitled")
                        initialFolder = Path.GetDirectoryName(MarkdownDocument.Filename);

                    var sd = new SaveFileDialog
                    {
                        Filter = "Image files (*.png;*.jpg;*.gif;)|*.png;*.jpg;*.jpeg;*.gif|All Files (*.*)|*.*",
                        FilterIndex = 1,
                        Title = "Save Image from Clipboard as",
                        InitialDirectory = initialFolder,
                        CheckFileExists = false,
                        OverwritePrompt = true,
                        CheckPathExists = true,
                        RestoreDirectory = true
                    };
                    var result = sd.ShowDialog();
                    if (result != null && result.Value)
                    {
                        imagePath = sd.FileName;
                        var ext = Path.GetExtension(imagePath)?.ToLower();

                        try
                        {
                            File.Delete(imagePath);

                            if (ext == ".jpg" || ext == ".jpeg")
                            {
                                using (var bmp = new Bitmap(bitMap))
                                {
                                    ImageUtils.SaveJpeg(bmp, imagePath, mmApp.Configuration.JpegImageCompressionLevel);
                                }
                            }
                            else
                            {
                                var format = ImageUtils.GetImageFormatFromFilename(imagePath);
                                bitMap.Save(imagePath, format);
                                bitMap.Dispose();

                                if (ext == ".png")
                                    mmFileUtils.OptimizePngImage(sd.FileName, 5); // async                            
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Couldn't copy file to new location: \r\n" + ex.Message,
                                mmApp.ApplicationName);
                            return;
                        }

                        string relPath = Path.GetDirectoryName(sd.FileName);
                        if (initialFolder != null)
                        {
                            try
                            {
                                relPath = FileUtils.GetRelativePath(sd.FileName, initialFolder);
                            }
                            catch (Exception ex)
                            {
                                mmApp.Log($"Failed to get relative path.\r\nFile: {sd.FileName}, Path: {imagePath}", ex);
                            }
                            imagePath = relPath;
                        }

                        if (imagePath.Contains(":\\"))
                            imagePath = "file:///" + imagePath;
                        else
                            imagePath = imagePath.Replace("\\", "/");

                        SetSelection($"![]({imagePath.Replace(" ","%20") })");


                        // Force the browser to refresh completely so image changes show up
                        Window.PreviewBrowser.Refresh(true);                        
                        //PreviewMarkdownCallback(); // force a preview refresh
                    }
                }
            }
            else if (Clipboard.ContainsText())
            {
                // just paste as is at cursor or selection
                SetSelection(Clipboard.GetText());
            }
        }

        /// <summary>
        /// Embeds a dropped file as an image. If not an image no action is taken
        /// </summary>
        /// <param name="file"></param>
        public void EmbedDroppedFileAsImage(string file)
        {
            //NavigatingCancelEventArgs e;
            string ext = Path.GetExtension(file).ToLower();

            if (ext == ".png" || ext == ".gif" || ext == ".jpg" || ext == ".jpeg" || ext == ".svg")
            {
                var docPath = Path.GetDirectoryName(MarkdownDocument.Filename);
                if (string.IsNullOrEmpty(docPath))
                    docPath = mmApp.Configuration.LastImageFolder;

                // if lower than 1 level down off base path ask to save the file
                string relFilePath = FileUtils.GetRelativePath(file, docPath);
                if (relFilePath.StartsWith("..\\..") || relFilePath.Contains(":\\"))
                {
                    var sd = new SaveFileDialog
                    {
                        Filter =
                            "Image files (*.png;*.jpg;*.gif;)|*.png;*.jpg;*.jpeg;*.gif|All Files (*.*)|*.*",
                        FilterIndex = 1,
                        Title = "Save dropped Image as",
                        InitialDirectory = docPath,
                        FileName = Path.GetFileName(file),
                        CheckFileExists = false,
                        OverwritePrompt = true,
                        CheckPathExists = true,
                        RestoreDirectory = true
                    };
                    var result = sd.ShowDialog();
                    if (result == null || !result.Value)
                        return;

                    relFilePath = FileUtils.GetRelativePath(sd.FileName, docPath);

                    File.Copy(file, sd.FileName, true);
                }

                if (!relFilePath.Contains(":\\"))
                    relFilePath = relFilePath.Replace("\\", "/");
                else
                    relFilePath = "file:///" + relFilePath;

                AceEditor.setselpositionfrommouse(false);

                Window.Dispatcher.InvokeAsync(() =>
                {
                    SetSelectionAndFocus($"\r\n![]({relFilePath.Replace(" ", "%20")})\r\n");

                    // Force the browser to refresh completely so image changes show up
                    Window.PreviewBrowser.Refresh(true); 
                }, DispatcherPriority.ApplicationIdle);
                
                    

                Window.Activate();
            }
            else if (mmApp.AllowedFileExtensions.Contains($",{ext},"))
            {
                Window.OpenTab(file, rebindTabHeaders: true);
            }
        }

        public void PreviewContextMenu(dynamic position)
        {
            Window.PreviewBrowser.ExecuteCommand("PreviewContextMenu");
        }


        /// <summary>
        /// Return keyboard bindings object as a JSON string so we can bind inside
        /// of the editor JavaScript
        /// </summary>
        /// <returns></returns>
        public string GetKeyBindingsJson()
        {
            return JsonSerializationUtils.Serialize(
                mmApp.Model.Window.KeyBindings.KeyBindings.Where(kb=> kb.HasJavaScriptHandler));            
        }
        #endregion


        #region SpellChecking interactions

        /// <summary>
        /// Check spelling of an individual word - called from ACE Editor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="language"></param>
        /// <param name="reload"></param>
        /// <returns></returns>
        public bool CheckSpelling(string text,string language = "en-US",bool reload = false)
        {
            try
            {
                return SpellChecker.CheckSpelling(text, language, reload);
            }
            catch
            {
                Window.ShowStatusError("Spell checker failed to load.");
                return false;
            }
        }

        /// <summary>
        /// Forces the document to be spell checked again
        /// </summary>
        /// <returns></returns>
        public void SpellCheckDocument()
        {
            if (mmApp.Configuration.Editor.EnableSpellcheck)
               AceEditor.spellcheckDocument(true);
        }

        public void SetSpellChecking(bool turnOff)
        {
            mmApp.Configuration.Editor.EnableSpellcheck = !turnOff;
            if (turnOff)
                Window.ShowStatusError("Spell checking has been turned off as there are too many spelling errors. Please check your language.");
        }
        
        /// <summary>
        /// Shows spell check context menu options
        /// </summary>
        /// <param name="text"></param>
        /// <param name="language"></param>
        /// <param name="reload"></param>
        /// <returns></returns>
        public void GetSuggestions(string text, string language=null, bool reload = false, object range = null)
        {
            if (language == null)
                language = Window.Model.Configuration.Editor.Dictionary;

            var suggestions = SpellChecker.GetSuggestions(text, language, reload);

            var cm = new EditorContextMenu();
            cm.ShowContextMenuAll(suggestions,range);               
        }

        /// <summary>
        /// Shows the Editor Context Menu at the current position
        /// 
        /// called from editor
        /// </summary>
        public void EditorContextMenu()
        {
            AceEditor?.showSuggestions(false);            
        }
        

        /// <summary>
        /// Adds a new word to add-on the dictionary for a given locale
        /// </summary>
        /// <param name="word">Word to add</param>
        /// <param name="lang">Dictionary to add to or the current one in use</param>
        public void AddWordToDictionary(string word, string lang = null)
        {
            if (lang == null)
                lang = Window.Model.Configuration.Editor.Dictionary;

            if (!SpellChecker.AddWordToDictionary(word, lang))
                Window.ShowStatusError("Couldn't add word to dictionary. Most likely you don't have write access in the settings folder.");
            else
            {
                AceEditor.isDirty = true;
            }
        }
        #endregion


        #region Events Raised by the editor calling back to WPF
        /// <summary>
        /// Allows the Editor to raise events that can be captured by 
        /// Addins that are subscribed to OnNotifyAddin.
        /// </summary>
        public void NotifyAddins(string command, object parameter)
        {
            if (parameter == null || parameter == DBNull.Value)
                parameter = this;

            AddinManager.Current.RaiseOnNotifyAddin(command, parameter);
        }

        #endregion

        /// <summary>
        /// Handle dropping of files 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebBrowser_NavigatingAndDroppingFiles(object sender, NavigatingCancelEventArgs e)
        {
            var url = e.Uri.ToString().ToLower();

            if (url.Contains("editor.htm") || url.Contains("editorsimple.htm"))
                return; // continue navigating

            // otherwise we either handle or don't allow
            e.Cancel = true;

            // if it's a URL or ??? don't navigate
            if (!e.Uri.IsFile)
                return;

            string file = e.Uri.LocalPath;

            EmbedDroppedFileAsImage(file);            
        }

        public override string ToString()
		{
			return MarkdownDocument?.Filename ?? base.ToString();
		}

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    [DebuggerDisplay("Row: {row},Col: {column}")]
    public class AcePosition
    {
        public int row { get; set; }
        public int column { get; set; }
    }

    public struct EditorStyle
    {
        public string Theme;
        public string Font;

        //public int FontSize;

        //public decimal LineHeight;

        public bool WrapText;

        public bool ShowWhiteSpace;

        public bool ShowLineNumbers;

        public bool HighlightActiveLine;

        public string KeyboardHandler;

    }
}
