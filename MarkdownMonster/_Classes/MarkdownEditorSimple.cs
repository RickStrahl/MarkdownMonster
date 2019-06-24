#region License
/*
 **************************************************************
 *  Author: Rick Strahl
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 *
 * Created: 04/28/2016
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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using MarkdownMonster.Windows;

namespace MarkdownMonster
{
    /// <summary>
    /// A stripped down version of the MarkdownEditor class that is used
    /// for displaying various edtibable code snippets for the Code Editor
    /// as well as in various addins. Similar behavior and hookup, but doesn't
    /// include all the Markdown parsing features and uses a separate template
    /// that is simpler and less resource intensive.
    ///
    /// If you need to use a code editor as part of an Addin this is the class
    /// to use. For a usage example, see the PasteCode.xaml form and code behind.
    /// </summary>
    [ComVisible(true)]
    public class MarkdownEditorSimple
    {
        private WebBrowser WebBrowser;

        
        public AceEditorInterop AceEditor { get; set; }

        string InitialValue { get; set; }

        public Func<bool> IsDirtyAction { get; set; }

        public string CurrentText { get; set; }

        public string EditorSyntax { get; set; } = "markdown";



        public MarkdownEditorSimple(WebBrowser wb, string initialValue = null, string editorSyntax = "markdown")
        {
            EditorSyntax = editorSyntax;

            WebBrowser = wb;
            wb.Visibility = Visibility.Hidden;

            wb.LoadCompleted += OnDocumentCompleted;
            string path = System.IO.Path.Combine(App.InitialStartDirectory, "Editor\\editor.htm");
            wb.Navigate("file:///" + path);

            InitialValue = initialValue;
        }

        private void OnDocumentCompleted(object sender, NavigationEventArgs e)
        {
            if (AceEditor == null)
            {
                try
                {
                    // Get the JavaScript Ace Editor Instance
                    var inst = WebBrowser.InvokeScript("initializeinteropSimple", this);
                    AceEditor = new AceEditorInterop(inst);
                }
                catch (Exception ex)
                {
                    mmApp.Log($"Editor failed to load initializeinteropSimple {e.Uri}", ex);
                    //throw;
                }

                WindowUtilities.DoEvents();

                WebBrowser.Visibility = Visibility.Visible;
                RestyleEditor();

                if (InitialValue != null)
                    SetMarkdown(InitialValue);

                if (EditorSyntax != "markdown")
                    AceEditor?.SetLanguage(EditorSyntax);

            }
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
                AceEditor.SetValue(markdown,position,false);
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

            CurrentText = AceEditor.GetValue();
            return CurrentText;

        }

        /// <summary>
        /// Focuses the Markdown editor in the Window
        /// </summary>
        public void SetEditorFocus()
        {
            AceEditor?.SetFocus();
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

            AceEditor.SetSelection(text);
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

            object fontsize = AceEditor.Invoke("getFontSize", false);
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
            return AceEditor?.GetSelection();
        }

        public int GetLineNumber()
        {
            if (AceEditor == null)
                return -1;

            int lineNo = AceEditor.GetLineNumber();
            return lineNo;
        }

        public void GotoLine(int line)
        {
            if (AceEditor == null)
                return;
            AceEditor.GotoLine(line,false, false);
        }

        public void SetEditorSyntax(string syntax = "markdown")
        {
            AceEditor?.SetLanguage(syntax);
        }




        /// <summary>
        /// Restyles the current editor with configuration settings
        /// from the mmApp.Configuration object (or Model.Configuration
        /// from an addin).
        /// </summary>
        /// <param name="forceSync">Forces higher priority on this operation - use when editor initializes at first</param>
        public void RestyleEditor(bool forceSync = false, bool initialize = false)
        {
            if (AceEditor == null)
                return;

            mmApp.Model.Window.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        // pass config settings to the editor and force
                        // editor to apply new settings
                        AceEditor.SetEditorStyling();

                        if (EditorSyntax == "markdown" || EditorSyntax == "text")
                            AceEditor.EnableSpellChecking(!mmApp.Configuration.Editor.EnableSpellcheck,
                                mmApp.Configuration.Editor.Dictionary);
                        else
                            // always disable for non-markdown text
                            AceEditor.EnableSpellChecking(true, mmApp.Configuration.Editor.Dictionary);
                    }
                    catch
                    {
                    }
                },
                forceSync
                    ? System.Windows.Threading.DispatcherPriority.Send
                    : System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        ///// <summary>
        ///// Restyles the current editor with configuration settings
        ///// from the mmApp.Configuration object (or Model.Configuration
        ///// from an addin).
        ///// </summary>
        //public void RestyleEditor()
        //{
        //    if (AceEditor == null)
        //        return;

        //    try
        //    {
        //        // determine if we want to rescale the editor fontsize
        //        // based on DPI Screen Size
        //        decimal dpiRatio = 1;
        //        try
        //        {
        //            var window = VisualTreeHelper.GetParent(WebBrowser);
        //            while (!(window is Window))
        //            {
        //                window = VisualTreeHelper.GetParent(window);
        //            }
        //            dpiRatio = WindowUtilities.GetDpiRatio(window as Window);
        //        }
        //        catch { }

        //        AceEditor.SetEditorStyling();

        //        if (EditorSyntax == "markdown" || this.EditorSyntax == "text")
        //            AceEditor.EnableSpellChecking(!mmApp.Configuration.Editor.EnableSpellcheck, mmApp.Configuration.Editor.Dictionary);
        //        else
        //            // always disable for non-markdown text
        //            AceEditor.EnableSpellChecking(false,mmApp.Configuration.Editor.Dictionary);
        //    }
        //    catch { }
        //}

        #endregion

        #region Callback functions from the Html Editor

        /// <summary>
        /// Callback handler callable from JavaScript editor
        /// </summary>
        public void PreviewMarkdownCallback(bool dontGetMarkdown = false, int editorLineNumber = -1)
        {
        }

        /// <summary>
        /// ACE Editor Notification when focus is lost
        /// </summary>
        public void LostFocus()
        {
         
        }

        /// <summary>
        /// ACE Editor Notification when focus is set to the editor
        /// </summary>
        public void GotFocus()
        {
        }
        
        public void ResizeWindow()
        {
            // nothing to do at the moment
        }

        /// <summary>
        /// Callback to force updating of the status bar document stats
        /// </summary>
        /// <param name="stats"></param>
        public void UpdateDocumentStats(object stats)
        {
        }


        public bool IsDirty(bool previewIfDirty = false)
        {
            if (IsDirtyAction != null)
                return IsDirtyAction.Invoke();

            return true;

        }
        #endregion
    }
}
