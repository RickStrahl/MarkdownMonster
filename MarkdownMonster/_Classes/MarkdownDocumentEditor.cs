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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Markdig;
using MarkdownMonster.AddIns;
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
    public class MarkdownDocumentEditor 
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
        public string EditorSyntax { get; set; }
        public int InitialLineNumber { get; set; }


        /// <summary>
        /// Optional identifier that lets you specify what type of
        /// document we're dealing with.
        /// 
        /// Can be used by Addins to create customer editors or handle
        /// displaying the document a different way.
        /// </summary>
        public string Identifier { get; set; } = "MarkdownDocument";


        #region Loading And Initialization
        public MarkdownDocumentEditor(WebBrowser browser)
        {
            WebBrowser = browser;
            WebBrowser.Navigating += WebBrowser_NavigatingAndDroppingFiles;        
        }
        
        /// <summary>
        /// Loads a new document into the active editor using 
        /// MarkdownDocument instance.
        /// </summary>
        /// <param name="mdDoc"></param>
        public void LoadDocument(MarkdownDocument mdDoc = null)
        {            
            if (mdDoc != null)
                MarkdownDocument = mdDoc;

            if (AceEditor == null)
            {
                WebBrowser.LoadCompleted += OnDocumentCompleted;
                WebBrowser.Navigate(new Uri(Path.Combine(Environment.CurrentDirectory, "Editor\\editor.htm")));                      
               
                //WebBrowser.Navigate("http://localhost:8080/editor.htm");
            }

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
                SetShowLineNumbers(mmApp.Configuration.EditorShowLineNumbers);
                SetShowInvisibles(mmApp.Configuration.EditorShowInvisibles);


                if (InitialLineNumber > 0)
                {
                    GotoLine(InitialLineNumber);
                }

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
                        mmApp.Configuration.StatusTimeout);
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
                    editor.RestyleEditor();
                    editor.AceEditor?.setShowLineNumbers(mmApp.Configuration.EditorShowLineNumbers);
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
                html = wrapValue(input, "**", "**",stripSpaces: true);
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
                        html = $"![{form.ImageText}]({image.Replace(" ","%20")})";
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

                if (res != null && res.Value)
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
                        html = $"![{form.ImageText}]({image.Replace(" ","%20")})";
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
                        SetSelection(form.TableHtml.TrimEnd()+"\n");
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


        public bool IsPreviewToEditorSync()
        {
            var mode = mmApp.Configuration.PreviewSyncMode;
            if (mode == PreviewSyncMode.PreviewToEditor || mode == PreviewSyncMode.EditorAndPreview)
                return true;

            return false;
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
            
            var result  = MarkupMarkdown(action, html);

            
            if (!string.IsNullOrEmpty(result.Html) && html != result.Html)
                SetSelectionAndFocus(result.Html);
            else
                SetEditorFocus();

            if (result.CursorMovement != 0)
                MoveCursorPosition(result.CursorMovement);

            //if (result.CursorMovement != 0)
            //    Window.Dispatcher.InvokeAsync(() => MoveCursorPosition(result.CursorMovement),DispatcherPriority.ApplicationIdle);
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

                        var fontSize = mmApp.Configuration.EditorFontSize *  ((decimal) mmApp.Configuration.EditorZoomLevel / 100) * dpiRatio;
                        //Debug.WriteLine(fontSize + " " + (int) fontSize + "  " +  mmApp.Configuration.EditorFontSize  + " * " +  mmApp.Configuration.EditorZoomLevel + " * " + dpiRatio);

                        AceEditor.settheme(
                            mmApp.Configuration.EditorTheme,
                            mmApp.Configuration.EditorFont,
                            fontSize,
                            mmApp.Configuration.EditorWrapText,
                            mmApp.Configuration.EditorHighlightActiveLine,
                            mmApp.Configuration.EditorKeyboardHandler);

                        if (EditorSyntax == "markdown" || EditorSyntax == "text")
                            AceEditor.enablespellchecking(!mmApp.Configuration.EditorEnableSpellcheck,
                                mmApp.Configuration.EditorDictionary);
                        else
                            // always disable for non-markdown text
                            AceEditor.enablespellchecking(true, mmApp.Configuration.EditorDictionary);
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
                show = mmApp.Configuration.EditorShowLineNumbers;

            AceEditor?.setShowLineNumbers(show.Value);
        }

        /// <summary>
        /// Enables or disables the display of invisible characters.
        /// </summary>
        /// <param name="show"></param>
        public void SetShowInvisibles(bool? show = null)
        {
            if (show == null)
                show = mmApp.Configuration.EditorShowInvisibles;

            AceEditor?.setShowInvisibles(show.Value);
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

        #region Selection and Line Operations
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
        /// <param name="row"></param>
        /// <returns></returns>
        public string GetLine(int rowNumber)
        {
            return AceEditor?.getLine(rowNumber);
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
        /// <param name="line"></param>
        public void GotoLine(int line)
        {
            if (line < 0)
                line = 0;

            try
            {
                AceEditor?.gotoLine(line);
            }
            catch { }
        }

        public void FindAndReplaceTextInCurrentLine(string search, string replace)
        {
            AceEditor?.findAndReplaceTextInCurrentLine(search, replace);
        }


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

        public void ExecEditorCommand(string action, object parm = null)
        {
            AceEditor?.execcommand(action, parm);
        }

        public void ResizeWindow()
        {
            // nothing to do at the moment             
        }
        public void PreviewContextMenu(dynamic position)
        {
            Window.PreviewBrowser.ExecuteCommand("PreviewContextMenu");
        }


        #region Callback functions from the Html Editor


        /// <summary>
        /// Sets the Markdown Document as having changes
        /// </summary>
        /// <param name="value">ignored</param>
        public bool SetDirty(bool value)
        {
             GetMarkdown();

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
            MarkdownDocument.IsDirty = MarkdownDocument.CurrentText != MarkdownDocument.OriginalText;

            if (MarkdownDocument.IsDirty)
                AddinManager.Current.RaiseOnDocumentChanged();

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
        public void PreviewMarkdownCallback(bool dontGetMarkdown = false)
        {
            if (!dontGetMarkdown)
                GetMarkdown();                        

            Window.PreviewBrowser.PreviewMarkdownAsync(keepScrollPosition: true);
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
        public void SpecialKey(string key)
        {

            // run this one sync to avoid Browser default Open File popup!
            if (key == "ctrl-o")
            {
                Window.Model.Commands.OpenDocumentCommand.Execute(Window);
                return;
            }

            // invoke out of sync in order to force out of scope of the editor - affects weird key behavior otherwise
            Window.Dispatcher.InvokeAsync(() =>
            {
                if (key == "ctrl-s")
                {
                    Window.Model.Commands.SaveCommand.Execute(Window);
                }
                else if (key == "ctrl-n")
                {
                    Window.Model.Commands.NewDocumentCommand.Execute(Window);
                }
                
                else if (key == "ctrl-p")
                {
                    Window.Model.PrintPreviewCommand.Execute(Window.ButtonPrintPreview);
                }                
                else if (key == "ctrl-b")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("bold");
                }
                else if (key == "ctrl-i")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("italic");
                }
                else if (key == "ctrl-`")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("inlinecode");
                }
                else if (key == "ctrl-l")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("list");
                }
                else if (key == "ctrl-j")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("emoji");
                }
                else if (key == "ctrl-k")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("href");
                }
                else if (key == "alt-i")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("image");
                }
                else if (key == "alt-c")
                {
                    Window.Model.Commands.ToolbarInsertMarkdownCommand.Execute("code");
                }
                else if (key == "ctrl-shift-v")
                {
                    Window.Button_PasteMarkdownFromHtml(WebBrowser, null);
                }
                else if (key == "ctrl-shift-c")
                {
                    Window.Model.CopyAsHtmlCommand.Execute(WebBrowser);
                }
                else if (key == "ctrl-shift-z")
                {
                    Window.Model.Commands.RemoveMarkdownFormattingCommand.Execute(WebBrowser);
                }               
                else if (key == "ctrl-tab")
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

                else if (key == "ctrl-shift-tab")
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
                else if (key == "ctrl-=")
                {
                    mmApp.Configuration.EditorZoomLevel += 2;
                    RestyleEditor();
                }
                else if (key == "ctrl--")
                {
                    mmApp.Configuration.EditorZoomLevel -= 2;                    
                    RestyleEditor();
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }


        public void FindAndReplaceText(string search, string replace)
        {
            AceEditor?.findAndReplaceText(search, replace);
        }

        /// <summary>
        /// Allows the PreviewBrowser to navigate to a URL for external links
        /// so links open in the default browser rather than IE.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>true if handled (navigated) false if passed through and expected to navigate</returns>
        public bool NavigateExternalUrl(string url)
        {
            if (mmApp.Configuration.PreviewHttpLinksExternal &&  !string.IsNullOrEmpty(url))
            {
                ShellUtils.GoUrl(url);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handle pasting and handle images
        /// </summary>
        public void PasteOperation()
        {
            if (Clipboard.ContainsImage())
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
                        PreviewMarkdownCallback(); // force a preview refresh
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

                Window.Dispatcher.InvokeAsync(() => SetSelectionAndFocus($"\r\n![]({relFilePath.Replace(" ","%20")})\r\n"),
                    DispatcherPriority.ApplicationIdle);

                Window.Activate();
            }
            else if (mmApp.AllowedFileExtensions.Contains($",{ext},"))
            {
                Window.OpenTab(file, rebindTabHeaders: true);
            }
        }

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

        #endregion

        #region SpellChecking interactions
        static Hunspell GetSpellChecker(string language = "EN_US", bool reload = false)
        {
            if (reload || _spellChecker == null)
            {
                string dictFolder = Path.Combine(Environment.CurrentDirectory,"Editor\\");

                string aff = dictFolder + language + ".aff";
                string dic = Path.ChangeExtension(aff,"dic");

                _spellChecker = new Hunspell(aff, dic);

                // Custom Dictionary if any
                string custFile = Path.Combine(mmApp.Configuration.CommonFolder,language + "_custom.txt");
                if (File.Exists(custFile))
                {
                    var lines = File.ReadAllLines(custFile);
                    foreach (var line in lines)
                    {
                        _spellChecker.Add(line);
                    }
                }
            }

            return _spellChecker;
        }
        private static Hunspell _spellChecker = null;
        

        /// <summary>
        /// Check spelling of an individual word
        /// </summary>
        /// <param name="text"></param>
        /// <param name="language"></param>
        /// <param name="reload"></param>
        /// <returns></returns>
        public bool CheckSpelling(string text,string language = "EN_US",bool reload = false)
        {
            var hun = GetSpellChecker(language, reload);
            return hun.Spell(text);
        }


        /// <summary>
        /// Shows spell check context menu options
        /// </summary>
        /// <param name="text"></param>
        /// <param name="language"></param>
        /// <param name="reload"></param>
        /// <returns></returns>
        public void GetSuggestions(string text, string language = "EN_US", bool reload = false, object range = null)
        {
            IEnumerable<string> suggestions = null;

            if (!string.IsNullOrEmpty(text) && range != null)           
            {
                var hun = GetSpellChecker(language, reload);
                suggestions = hun.Suggest(text).Take(10);
            }

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
        /// <param name="word"></param>
        /// <param name="lang"></param>
        public void AddWordToDictionary(string word, string lang = "EN_US")
        {
            File.AppendAllText(Path.Combine(mmApp.Configuration.CommonFolder + "\\",  lang + "_custom.txt"),word  + "\n");
            _spellChecker.Add(word);            
        }
		#endregion

		public override string ToString()
		{
			return MarkdownDocument?.Filename ?? base.ToString();
		}

    }


    [DebuggerDisplay("Row: {row},Col: {column}")]
    public class AcePosition
    {
        public int row { get; set; }
        public int column { get; set; }
    }
}
