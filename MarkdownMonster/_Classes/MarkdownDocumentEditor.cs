#region License
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MarkdownMonster.AddIns;
using MarkdownMonster.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using NHunspell;
using Westwind.Utilities;

namespace MarkdownMonster
{
    [ComVisible(true)]
    public class MarkdownDocumentEditor 
    {
        public WebBrowser WebBrowser { get; set; }

        public MainWindow Window { get; set;  }

        public MarkdownDocument MarkdownDocument { get; set; }

        public dynamic AceEditor { get; set; }
        public string EditorSyntax { get; set; }


        #region Loading And Initialization
        public MarkdownDocumentEditor(WebBrowser browser)
        {
            WebBrowser = browser;        
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
                WebBrowser.Navigate(Path.Combine(Environment.CurrentDirectory, "Editor\\editor.htm"));
            }
            SetMarkdown();

            FindSyntaxFromFileType(MarkdownDocument.Filename);            
        }


        private void OnDocumentCompleted(object sender, NavigationEventArgs e)
        {
            if (AceEditor == null)
            {
                // Get the JavaScript Ace Editor Instance
                dynamic doc = WebBrowser.Document;
                var window = doc.parentWindow;
                AceEditor = window.initializeinterop("", this);
                               
               if (EditorSyntax != "markdown")
                    AceEditor.setlanguage(EditorSyntax);                

                WebBrowser.Visibility = Visibility.Visible;
                RestyleEditor();
            }
            SetMarkdown();            
        }

        #endregion

        #region Markdown Access and Manipulation

        /// <summary>
        /// Looks up and sets the EditorSyntax based on a file name
        /// So .cs file gets csharp, .xml get xml etc.        
        /// </summary>
        /// <param name="filename"></param>
        public void FindSyntaxFromFileType(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return;

            EditorSyntax = "markdown";

            if (filename.ToLower() == "untitled")
                return;

            var ext = Path.GetExtension(MarkdownDocument.Filename).ToLower().Replace(".", "");
            if (ext == "md") { }                
            else if (ext == "json")
                EditorSyntax = "json";
            else if (ext == "html" || ext == "htm")
                EditorSyntax = "html";

            else if (ext == "xml" || ext == "config")
                EditorSyntax = "xml";
            else if (ext == "js")
                EditorSyntax = "javascript";
            else if (ext == "ts")
                EditorSyntax = "typescript";
            else if (ext == "cs")
                EditorSyntax = "csharp";
            else if (ext == "cshtml")
                EditorSyntax = "razor";
            else if (ext == "css")
                EditorSyntax = "css";
            else if (ext == "prg")
                EditorSyntax = "foxpro";
            else if (ext == "txt")
                EditorSyntax = "text";
            else if (ext == "php")
                EditorSyntax = "php";
            else if (ext == "py")
                EditorSyntax = "python";
            else if (ext == "ps1")
                EditorSyntax = "powershell";
            else if (ext == "sql")
                EditorSyntax = "sqlserver";
            else
                EditorSyntax = "";
            
        }

        /// <summary>
        /// Sets the markdown text into the editor control
        /// </summary>
        /// <param name="markdown"></param>
        public void SetMarkdown(string markdown = null, object position = null)
        {
            if (MarkdownDocument != null)
            {
                if (string.IsNullOrEmpty(markdown))
                    markdown = MarkdownDocument.CurrentText;
                else if (markdown != MarkdownDocument.CurrentText)
                     SetDirty(true);                
            }
            if (AceEditor != null)
                AceEditor.setvalue(markdown,position);
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
        /// Saves the active document to file using the filename
        /// defined on the MarkdownDocument.
        /// 
        /// If there's no active filename a file save dialog
        /// If there's no active filename a file save dialog
        /// is popped up. 
        /// </summary>1
        public bool SaveDocument()
        {
            if (MarkdownDocument == null || AceEditor == null || 
               !AddinManager.Current.RaiseOnBeforeSaveDocument(MarkdownDocument))
                return false;
            
            GetMarkdown();

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
                }
            }

            return true;
        }

        /// <summary>
        /// Takes action on the selected string in the editor using
        /// predefined commands.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="input"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public string MarkupMarkdown(string action, string input, string style = null)
        {
            if (string.IsNullOrEmpty(action))
                return input;

            action = action.ToLower();

            if (string.IsNullOrEmpty(input) && !StringUtils.Inlist(action, new string[] { "image", "href", "code" }))
                return null;

            string html = input;

            if (action == "bold")
                html = "**" + input + "**";
            else if (action == "italic")
                html = "*" + input + "*";
            else if (action == "small")
                html = "<small>" + input + "</small>";
            else if (action == "underline")
                html = "<u>" + input + "</u>";
            else if (action == "strikethrough")
                html = "~~" + input + "~~";
            else if (action == "inlinecode")
                html = "`" + input + "`";
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

            else if (action == "quote")
            {
                StringBuilder sb = new StringBuilder();
                var lines = StringUtils.GetLines(input);
                foreach (var line in lines)
                {
                    sb.AppendLine("> " + line);
                }
                html = sb.ToString();
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
                        html = $"<a href=\"{form.Link}\" target=\"top\">{form.LinkText}</a>";
                    else
                        html = $"[{form.LinkText}]({form.Link})";
                }
            }
            else if (action == "image")
            {
                var form = new PasteImage
                {
                    Owner = Window,
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
                if (res != null && res.Value)
                {
                    var image = form.Image;
                    html = $"![{form.ImageText}]({form.Image})";
                }
            }
            else if (action == "code")
            {
                var form = new PasteCode();
                form.Owner = Window;
                form.Code = input;
                form.CodeLanguage = "csharp";

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
            
            return html;
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

        public int GetFontSize()
        {
            if (AceEditor == null)
                return 0;

            object fontsize = AceEditor.getfontsize(false);
            if (fontsize == null || !(fontsize is double || fontsize is int) )
                return 0;

            // If we have a fontsize, force the zoom level to 100% 
            // The font-size has been adjusted to reflect the zoom percentage
            var wb = (dynamic)WebBrowser.GetType().GetField("_axIWebBrowser2",
                      BindingFlags.Instance | BindingFlags.NonPublic)
                      .GetValue(WebBrowser);
            int zoomLevel = 100; // Between 10 and 1000
            wb.ExecWB(63, 2, zoomLevel, ref zoomLevel);   // OLECMDID_OPTICAL_ZOOM (63) - don't prompt (2)
            
            return Convert.ToInt32(fontsize);
        }

        /// <summary>
        /// Gets the current selection of the editor
        /// </summary>
        /// <returns></returns>
        public string GetSelection()
        {            
            return AceEditor?.getselection(false);            
        }

        public int GetLineNumber()
        {
            if (AceEditor == null)
                return -1;

            return AceEditor.getLineNumber(false);
        }

        public void GotoLine(int line)
        {
            if (AceEditor == null)
                return;            
            AceEditor.gotoLine(line);
        }

        public bool IsPreviewToEditorSync()
        {
            var mode = mmApp.Configuration.PreviewSyncMode;
            if (mode == PreviewSyncMode.BrowserToPreview || mode == PreviewSyncMode.PreviewAndBrowser)
                return true;

            return false;
        }

        /// <summary>
        /// Focuses the Markdown editor in the Window
        /// </summary>
        public void SetEditorFocus()
        {            
            AceEditor?.setfocus(true);
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
            
            string newhtml = MarkupMarkdown(action, html);

            if (!string.IsNullOrEmpty(newhtml) && newhtml != html)            
                SetSelectionAndFocus(newhtml);                            
        }
        #endregion

        #region Callback functions from the Html Editor


        /// <summary>
        /// Sets the Markdown Document as having changes
        /// </summary>
        /// <param name="value"></param>
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

            var res = MessageBox.Show(text, title, btn, image);
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
        public void PreviewMarkdownCallback()
        {
            GetMarkdown();                        
            Window.PreviewMarkdownAsync(null,true);
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

            Window.StatusStats.Text = $"{words:n0} words, {lines:n0} lines";
            
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
            if (key == "ctrl-s")
            {
                Window.Model.SaveCommand.Execute(Window);
            }
            else if (key == "ctrl-n")
            {
                Window.Button_Handler(Window.ButtonNewFile,null);
            }
            else if (key == "ctrl-o")
            {
                Window.Button_Handler(Window.ButtonOpenFile, null);
            }
            else if (key == "ctrl-b")
            {
                Window.Model.ToolbarInsertMarkdownCommand.Execute("bold");
            }
            else if (key == "ctrl-i")
            {
                Window.Model.ToolbarInsertMarkdownCommand.Execute("italic");
            }
            else if (key == "ctrl-`")
            {
                Window.Model.ToolbarInsertMarkdownCommand.Execute("inlinecode");
            }
            else if (key == "ctrl-l")
            {
                Window.Model.ToolbarInsertMarkdownCommand.Execute("list");
            }
            if (key == "ctrl-k")
            {
                Window.Model.ToolbarInsertMarkdownCommand.Execute("href");
            }
            if (key == "alt-c")
            {
                Window.Model.ToolbarInsertMarkdownCommand.Execute("code");
            }
            if (key == "ctrl-shift-v")
            {
                Window.Button_PasteMarkdownFromHtml(WebBrowser, null);
            }
            if (key == "ctrl-shift-c")
            {
                Window.Button_CopyMarkdownAsHtml(WebBrowser, null);
            }
            if (key == "ctrl-shift-down")
            {
                if (Window.PreviewBrowser.IsVisible)
                {
                    dynamic dom = Window.PreviewBrowser.Document;
                    dom.documentElement.scrollTop += 150;
                }                
            }
            if (key == "ctrl-shift-up")
            {
                if (Window.PreviewBrowser.IsVisible)
                {
                    dynamic dom = Window.PreviewBrowser.Document;
                    dom.documentElement.scrollTop -= 150;
                }
            }
            // zooming
            if (key == "ctrl-=")
            {
                mmApp.Configuration.EditorFontSize++;
                RestyleEditor();
            }
            if (key == "ctrl--")
            {
                mmApp.Configuration.EditorFontSize--;
                RestyleEditor();
            }
        }

        /// <summary>
        /// Handle pasting and handle images
        /// </summary>
        public void PasteOperation()
        {
            if (Clipboard.ContainsImage())
            {
                string imagePath = null;

                var bmpSource = Clipboard.GetImage();
                using (var bitMap = WindowUtilities.BitmapSourceToBitmap(bmpSource))
                {
                    imagePath = AddinManager.Current.RaiseOnSaveImage(bitMap);
                }
                if (!string.IsNullOrEmpty(imagePath))
                {
                    SetSelection($"![]({imagePath})");
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
                    try
                    {
                        var ext = Path.GetExtension(imagePath)?.ToLower();                        
                        using (var fileStream = new FileStream(imagePath, FileMode.Create))
                        {

                            BitmapEncoder encoder = null;
                            if (ext == ".png")
                                encoder = new PngBitmapEncoder();                            
                            else if(ext == ".jpg" || ext == ".jpeg")
                                encoder = new JpegBitmapEncoder();
                            else if (ext == ".gif")
                                encoder = new GifBitmapEncoder();

                            encoder.Frames.Add(BitmapFrame.Create(bmpSource));
                            encoder.Save(fileStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Couldn't copy file to new location: \r\n" + ex.Message, mmApp.ApplicationName);
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
                        if (!relPath.StartsWith("..\\"))
                            imagePath = relPath;
                    }

                    if (imagePath.Contains(":\\"))
                        imagePath = "file:///" + imagePath;
                    SetSelection($"![]({imagePath})");
                    PreviewMarkdownCallback(); // force a preview refresh
                }
            }
            else if (Clipboard.ContainsText())
            {
                // just paste as is at cursor or selection
                SetSelection(Clipboard.GetText());
            }
        }

        public void ExecEditorCommand(string action, object parm = null)
        {
            AceEditor.execcommand(action, parm);
        }

        /// <summary>
        /// Restyles the current editor with configuration settings
        /// from the mmApp.Configuration object (or Model.Configuration
        /// from an addin).
        /// </summary>
        public void RestyleEditor()
        {
            try
            {
                AceEditor.settheme(
                    mmApp.Configuration.EditorTheme,
                    mmApp.Configuration.EditorFontSize,
                    mmApp.Configuration.EditorWrapText,
                    mmApp.Configuration.EditorHighlightActiveLine,
                    mmApp.Configuration.EditorShowLineNumbers);

                if (EditorSyntax == "markdown" || this.EditorSyntax == "text")
                    AceEditor.enablespellchecking(!mmApp.Configuration.EditorEnableSpellcheck, mmApp.Configuration.EditorDictionary);
                else
                    // always disable for non-markdown text
                    AceEditor.enablespellchecking(true, mmApp.Configuration.EditorDictionary);
            }
            catch { }
        }

        public void ResizeWindow()
        {
            return;
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
        /// Returns spelling suggestions for an individual word
        /// </summary>
        /// <param name="text"></param>
        /// <param name="language"></param>
        /// <param name="reload"></param>
        /// <returns></returns>
        public string GetSuggestions(string text, string language = "EN_US", bool reload = false)
        {
            var hun = GetSpellChecker(language, reload); 

            var sugg = hun.Suggest(text).Take(10).ToArray();

            return JsonConvert.SerializeObject(sugg);            
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


    }


}