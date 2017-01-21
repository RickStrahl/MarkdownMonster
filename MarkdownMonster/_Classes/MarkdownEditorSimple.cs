using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MarkdownMonster;
using MarkdownMonster.Windows;

namespace MarkdownMonster
{
    [ComVisible(true)]
    public class MarkdownEditorSimple
    {
        private WebBrowser WebBrowser;

        public dynamic AceEditor;

        string InitialValue { get; set; }

        public Func<bool> IsDirtyAction { get; set; }

        public string CurrentText { get; set; }

        public string EditorSyntax { get; set; } = "markdown";
               


        public MarkdownEditorSimple(WebBrowser wb, string initialValue = null, string editorSyntax = "markdown")
        {
            EditorSyntax = editorSyntax;

            WebBrowser = wb;
            wb.Visibility = Visibility.Hidden;
            wb.Margin = new Thickness(-11, 0, 0, 15);

            wb.LoadCompleted += OnDocumentCompleted;
            string path = System.IO.Path.Combine(Environment.CurrentDirectory, "Editor\\editorSimple.htm");
            wb.Navigate("file:///" + path);

            InitialValue = initialValue;


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
                    mmApp.Log($"Editor failed to load initializeinterop {e.Uri}", ex);
                    //throw;
                }


                AceEditor?.setlanguage("markdown");

                WindowUtilities.DoEvents();
                WebBrowser.Visibility = Visibility.Visible;
                //RestyleEditor();

                if (InitialValue != null)
                    SetMarkdown(InitialValue);

                if (EditorSyntax != "markdown")
                    AceEditor?.setlanguage(EditorSyntax);

            }
        }

        public string FindSyntaxFromFileType(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return "markdown";

            EditorSyntax = "markdown";

            if (filename.ToLower() == "untitled")
                return "markdown";

            var ext = Path.GetExtension(filename).ToLower().Replace(".", "");
            if (ext == "md" || ext == "markdown") { }
            else if (ext == "json")
                EditorSyntax = "json";
            else if (ext == "html" || ext == "htm")
                EditorSyntax = "html";

            else if (ext == "xml" || ext == "config" || ext == "xaml")
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
            else if (ext == "rb" || ext == "ruby" || ext=="rake")
                EditorSyntax = "ruby";
            else
                EditorSyntax = "";

            return EditorSyntax;
        }

        #region Markdown Editor Methods


        /// <summary>
        /// Sets the markdown text into the editor control
        /// </summary>
        /// <param name="markdown"></param>
        public void SetMarkdown(string markdown = null, object position = null)
        {
            if (string.IsNullOrEmpty(markdown))
                markdown = CurrentText;

            if (markdown == null)
                markdown = string.Empty;

            if (AceEditor != null)
            {
                if (position == null)
                    position = -2; // keep position
                AceEditor.setvalue(markdown, position);
            }
        }

        /// <summary>
        /// Reads the markdown text from the editor control
        /// </summary>
        /// <returns></returns>
        public string GetMarkdown()
        {
            if (AceEditor == null)
                return "";

            CurrentText = AceEditor.getvalue(false);
            return CurrentText;

        }

        /// <summary>
        /// Focuses the Markdown editor in the Window
        /// </summary>
        public void SetEditorFocus()
        {
            AceEditor?.setfocus(true);
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
        }

        public int GetFontSize()
        {
            if (AceEditor == null)
                return 0;

            object fontsize = AceEditor.getfontsize(false);
            if (fontsize == null || !(fontsize is double || fontsize is int))
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

            int lineNo = AceEditor.getLineNumber(false);
            return lineNo;
        }

        public void GotoLine(int line)
        {
            if (AceEditor == null)
                return;
            AceEditor.gotoLine(line);
        }

        public void SetEditorSyntax(string syntax = "markdown")
        {
            AceEditor?.setlanguage(syntax);
        }

        #endregion

        #region Markdown Editor Callbacks

        public bool IsDirty()
        {
            if (IsDirtyAction != null)
                return IsDirtyAction.Invoke();

            return true;

        }
        #endregion
    }
}
