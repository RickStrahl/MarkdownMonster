using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MarkdownMonster;
using MarkdownMonster.Windows;
using Newtonsoft.Json;
using NHunspell;
using Westwind.Utilities;
using Timer = System.Threading.Timer;

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

        public MarkdownDocumentEditor(WebBrowser browser)
        {
            WebBrowser = browser;
        }

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
                dynamic doc = WebBrowser.Document;
                var window = doc.parentWindow;

                object t = this as object;
                AceEditor = window.initializeinterop("", this);
                               
               if (EditorSyntax != "markdown")
                    AceEditor.setlanguage(EditorSyntax);                

                WebBrowser.Visibility = Visibility.Visible;
                RestyleEditor();
            }
            SetMarkdown();            
        }

        public void FindSyntaxFromFileType(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return;

            EditorSyntax = "markdown";

            var ext = Path.GetExtension(MarkdownDocument.Filename).ToLower().Replace(".", "");
            if (ext == "json")
                EditorSyntax = "json";
            else if (ext == "html" || ext == "htm")
                EditorSyntax = "html";

            else if (ext == "xml" || ext == "config")
                EditorSyntax = "xml";
            else if (ext == "js")
                EditorSyntax = "javascript";
            else if (ext == "cs")
                EditorSyntax = "csharp";
            else if (ext == "cshtml")
                EditorSyntax = "razor";
            else if (ext == "css")
                EditorSyntax = "css";
            else if (ext == "prg")
                EditorSyntax = "foxpro";
            
        }

        /// <summary>
        /// Sets the markdown text into the editor control
        /// </summary>
        /// <param name="markdown"></param>
        public void SetMarkdown(string markdown = null)
        {
            if (string.IsNullOrEmpty(markdown) && MarkdownDocument != null)
                markdown = MarkdownDocument.CurrentText;


            if (AceEditor != null)
                AceEditor.setvalue(markdown);
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


        public void SaveDocument()
        {
            if (MarkdownDocument == null || AceEditor == null)
                return;

            GetMarkdown();
            MarkdownDocument.Save();
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
        }


        /// <summary>
        /// Takes a command  like bold,italic,href etc., reads the
        /// text from editor selection, transforms it and pastes
        /// it back into the document.
        /// </summary>
        /// <param name="action"></param>
        public void ProcessEditorUpdateCommand(string action)
        {
            string html = AceEditor.getselection(false);
            
            string newhtml = MarkupMarkdown(action, html);

            if (!string.IsNullOrEmpty(newhtml) && newhtml != html)
            {
                AceEditor.setselection(newhtml);
                AceEditor.setfocus(true);
                MarkdownDocument.CurrentText = GetMarkdown();
                Window.PreviewMarkdown(this, true);
            }
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
            
            action = action.ToLower();

            if (string.IsNullOrEmpty(input) && !StringUtils.Inlist(action, new string[] { "image", "code" }))
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
                var form = new PasteHref();
                form.Owner = Window;
                form.LinkText = input;

                // check for links in input or on clipboard
                string link = input;
                if (string.IsNullOrEmpty(link))
                    link = Clipboard.GetText();

                if (!(input.StartsWith("http:") || input.StartsWith("https:") || input.StartsWith("mailto:") || input.StartsWith("ftp:")))                
                    link = string.Empty;
                
                form.Link = link;

                bool? res = form.ShowDialog();
                if (res != null && res.Value)
                    html = $"[{form.LinkText}]({form.Link})";
            }
            else if (action == "image")
            {
                var form = new PasteImage();
                form.Owner = Window;
                form.ImageText = input;
                form.MarkdownFile = MarkdownDocument.Filename;
                

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
            
            return html;
        }

        

        #region Callback functions from the Html Editor

        public void SetDirty(bool value)
        {            
            MarkdownDocument.IsDirty = value;                                         
        }

        public void PreviewMarkdownCallback()
        {
            GetMarkdown();                        
            Window.PreviewMarkdownAsync(null,true);
        }


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
            else if (key == "ctrl-l")
            {
                Window.Model.ToolbarInsertMarkdownCommand.Execute("list");
            }
            if (key == "ctrl-k")
            {
                Window.Model.ToolbarInsertMarkdownCommand.Execute("href");
            }            
        }

       static Hunspell GetSpellChecker(string language = "EN_US", bool reload = false)
        {
            if (reload || _spellChecker == null)
            {
                string aff = Path.Combine(Environment.CurrentDirectory,"Editor\\" + language + ".aff");
                string dic = Path.ChangeExtension(aff,"dic");

                _spellChecker = new Hunspell(aff, dic);
            }

            return _spellChecker;
        }
        private static Hunspell _spellChecker = null;

        public bool CheckSpelling(string text,string language = "EN_US",bool reload = false)
        {
            var hun = GetSpellChecker(language, reload);
            return hun.Spell(text);
        }

        public string GetSuggestions(string text, string language = "EN_US", bool reload = false)
        {
            var hun = GetSpellChecker(language, reload); 

            var sugg = hun.Suggest(text).Take(10).ToArray();

            return JsonConvert.SerializeObject(sugg);            
        }
    

        /// <summary>
        /// Restyles the current editor with configuration settings
        /// </summary>
        public void RestyleEditor()
        {
            try
            {
                AceEditor.settheme(mmApp.Configuration.EditorTheme,
                    mmApp.Configuration.EditorFontSize,
                    mmApp.Configuration.EditorWrapText);

                if (this.EditorSyntax == "markdown" || this.EditorSyntax == "text")
                    AceEditor.enablespellchecking(!mmApp.Configuration.EditorEnableSpellcheck,mmApp.Configuration.EditorDictionary);
                else
                    // always disable for non-markdown text
                    AceEditor.enablespellchecking(true, mmApp.Configuration.EditorDictionary);
            }
            catch{ }
        }

#endregion

    }


}