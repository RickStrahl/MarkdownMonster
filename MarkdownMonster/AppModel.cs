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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Threading;
using FontAwesome.WPF;
using MarkdownMonster.AddIns;
using MarkdownMonster.Annotations;
using MarkdownMonster.Windows;
using Westwind.Utilities;

namespace MarkdownMonster
{
    /// <summary>
    /// Global App model for the application. Holds references to
    /// top level components like the Window, configuration and more
    /// as well as includes a number of helper functions.
    ///
    /// Available to Addins as `this.Model`.
    /// </summary>
    public class AppModel : INotifyPropertyChanged
    {


        /// <summary>
        /// An instance of the main application WPF form
        /// </summary>
        public MainWindow Window { set; get; }

        /// <summary>
        /// The application's main configuration object
        /// </summary>
        public ApplicationConfiguration Configuration { get; set; }

        /// <summary>
        /// Commands
        /// </summary>
        public AppCommands Commands { get; }

        /// <summary>
        /// Contains Main Window layout settings and functionality
        /// </summary>
        public MainWindowLayoutModel WindowLayout { get; internal set; }




        #region Document Open/Active State

        /// <summary>
        /// Returns an instance of the Active Editor instance. The editor contains
        /// editor behavior of the browser control as well as all interactions with
        /// the editor's event model and text selection interfaces.
        ///
        /// Contains an `AceEditor` property that references the underlying
        /// JavaScript editor wrapper instance.
        /// </summary>
        public MarkdownDocumentEditor ActiveEditor
        {
            get
            {
                var editor = Window.GetActiveMarkdownEditor();
                return editor;
            }
        }


        /// <summary>
        /// Returns the active Tab's file name - this can either be
        /// the name of a Markdown Doducment editor or just a filename
        /// or URL from a preview tab.
        /// </summary>
        public string ActiveTabFilename
        {
            get
            {
                var editor = Window.GetActiveMarkdownEditor();
                if (editor != null)                
                    return editor.MarkdownDocument?.Filename;

                var tab = Window.TabControl.SelectedItem as TabItem;
                return tab?.ToolTip as string;
            }
        }

        /// <summary>
        /// Returns the MarkdownDocument instance of the active editor
        /// </summary>
        public MarkdownDocument ActiveDocument
        {
            get => _activeDocument;
            set
            {
                // DO ON ALL ASSIGNMENTS ALWAYS
                _activeDocument = value;

                OnPropertyChanged(nameof(ActiveDocument));
                OnPropertyChanged(nameof(ActiveEditor));
                OnPropertyChanged(nameof(IsEditorActive));
                OnPropertyChanged(nameof(IsTabOpen));
                OnPropertyChanged(nameof(IsNoTabOpen));
            }
        }
        private MarkdownDocument _activeDocument;


        /// <summary>
        /// Gives a list of all the open documents as Markdown document instances
        /// </summary>
        public List<MarkdownDocument> OpenDocuments
        {
            get => _openDocuments;
            set
            {
                if (Equals(value, _openDocuments)) return;
                _openDocuments = value;
                OnPropertyChanged(nameof(OpenDocuments));
            }
        }

        private List<MarkdownDocument> _openDocuments;

        /// <summary>
        /// Returns a list of open editor instances inside of open tabs
        /// </summary>
        public List<MarkdownDocumentEditor> OpenEditors
        {
            get
            {
                var list  = new List<MarkdownDocumentEditor>();
                if (Window.TabControl.Items.Count < 1)
                    return list;

                foreach (System.Windows.Controls.TabItem tab in Window.TabControl.Items)
                {
                    var editor = tab.Tag as MarkdownDocumentEditor;
                    if (editor != null)
                        list.Add(editor);
                }
                return list;
            }
        }


        /// <summary>
        /// Determines whether there are open tabs
        /// </summary>
        public bool IsTabOpen
        {
            get
            {
                return  Window.TabControl.Items.Count > 0;
            }
        }
        public bool IsNoTabOpen
        {
            get
            {
                return Window.TabControl.Items.Count < 1;
            }
        }


        /// <summary>
        /// Determines if there's a document loaded
        /// </summary>
        public bool IsEditorActive
        {
            get
            {
                if (ActiveEditor != null && ActiveDocument != null)
                    return true;

                return false;
            }
        }


        /// <summary>
        /// Determines whether the editor currently has focus
        /// </summary>
        public bool IsEditorFocused
        {
            get => _isEditorFocused;
            set
            {
  			    if (Equals(value, _openDocuments)) return;
                _isEditorFocused = value;
                OnPropertyChanged(nameof(IsEditorFocused));
            }
        }
        private bool _isEditorFocused;

        #endregion

        #region Display Modes

        /// <summary>
        /// Determines whether the preview browser is visible or not
        /// </summary>
        public bool IsPreviewBrowserVisible
        {
            get => Configuration.IsPreviewVisible;
            set
            {
                if (value == Configuration.IsPreviewVisible) return;
                Configuration.IsPreviewVisible = value;
                OnPropertyChanged(nameof(IsPreviewBrowserVisible));
            }
        }


        /// <summary>
        /// Determines whether the preview is shown in an Exteranl Browser Window
        /// </summary>
        public bool IsExternalPreview
        {
            get { return Configuration.PreviewMode == PreviewModes.ExternalPreviewWindow; }
            set
            {
                if (value)
                    Configuration.PreviewMode = PreviewModes.ExternalPreviewWindow;
                else
                {
                    Configuration.PreviewMode = PreviewModes.InternalPreview;
                }
                OnPropertyChanged(nameof(IsExternalPreview));
                OnPropertyChanged(nameof(IsInternalPreview));
            }
        }


        /// <summary>
        /// Determines whether the preview is shown in the Internal Preview Pane
        /// </summary>
        public bool IsInternalPreview
        {
            get { return Configuration.PreviewMode == PreviewModes.InternalPreview; }
            set
            {
                if (value)
                    Configuration.PreviewMode = PreviewModes.InternalPreview;
                else
                    Configuration.PreviewMode = PreviewModes.ExternalPreviewWindow;

                OnPropertyChanged(nameof(IsInternalPreview));
                OnPropertyChanged(nameof(IsExternalPreview));
            }
        }



        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                if (value == _isFullScreen) return;
                _isFullScreen = value;
                OnPropertyChanged(nameof(IsFullScreen));
            }
        }

        private bool _isFullScreen = false;


        public bool IsPresentationMode
        {
            get => _isPresentationMode;
            set
            {
                if (_isPresentationMode == value) return;
                _isPresentationMode = value;
                OnPropertyChanged(nameof(IsPresentationMode));
            }
        }

        private bool _isPresentationMode;


        /// <summary>
        /// Determines whether the application is compiled in Debug Mode
        /// Provided here mainly as an aid for turning on and off debugging menu
        /// and UI options.
        /// </summary>
        public bool IsDebugMode
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
        private bool _isDebugMode;


#endregion


#region Statusbar Item Props

        /// <summary>
        /// A list of PreviewThemes as retrieved based on the folder structure of hte
        /// Preview folder.
        /// </summary>
        public List<string> PreviewThemeNames
        {
            get
            {
                if (_previewThemeNames == null || _previewThemeNames.Count < 1)
                {
                    var directories =
                        Directory.GetDirectories(Path.Combine(App.InitialStartDirectory, "PreviewThemes"));
                    foreach (string dir in directories.OrderBy(d => d))
                    {
                        var dirName = Path.GetFileName(dir);

                        if (dirName.ToLower() == "scripts")
                            continue;

                        _previewThemeNames.Add(dirName);
                    }
                }

                return _previewThemeNames;
            }
        }

        private readonly List<string> _previewThemeNames = new List<string>();





        public List<PreviewSyncModeItem> PreviewSyncModeItems
        {
            get
            {
                if (_previewSyncModeItems != null)
                    return _previewSyncModeItems;

                _previewSyncModeItems = ((PreviewSyncMode[]) Enum.GetValues(typeof(PreviewSyncMode)))
                    .Select(e =>
                    {
                        var item = new PreviewSyncModeItem
                        {
                            Name = StringUtils.FromCamelCase(e.ToString()),
                            Value = e
                        };

                        if (e == PreviewSyncMode.PreviewToEditor)
                            item.Icon = FontAwesomeIcon.ArrowCircleLeft;
                        else if (e == PreviewSyncMode.EditorAndPreview)
                            item.Icon = FontAwesomeIcon.Exchange;
                        else if (e == PreviewSyncMode.None)
                            item.Icon = FontAwesomeIcon.Ban;
                        else
                            item.Icon = FontAwesomeIcon.ArrowCircleRight;

                        char c = (char) ((int) item.Icon);
                        item.IconString = c.ToString() + "   " + item.Name;

                        return item;
                    })
                    .ToList();
                return _previewSyncModeItems;
            }
        }

        private List<PreviewSyncModeItem> _previewSyncModeItems;


        /// <summary>
        /// A list of Ace Editor themes retrieved from the Editor script folder
        /// </summary>
        public List<string> EditorThemeNames
        {
            get
            {
                if (_editorThemeNames == null || _editorThemeNames.Count < 1)
                {
                    var files = Directory.GetFiles(Path.Combine(App.InitialStartDirectory, "Editor\\Scripts\\Ace"),
                        "theme-*.js");
                    foreach (string file in files.OrderBy(d => d))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        fileName = fileName.Replace("theme-", "");

                        _editorThemeNames.Add(fileName);
                    }
                }
                return _editorThemeNames;
            }
        }

        private readonly List<string> _editorThemeNames = new List<string>();


        /// <summary>
        /// List of registered Markdown Parsers
        /// </summary>
        public List<string> MarkdownParserNames
        {
            get
            {
                if (!AddinManager.Current.AddinsLoadingComplete)
                    return null;

                var parsers = MarkdownParserFactory.GetParserNames();
                return parsers;
            }
        }

        public List<string> DocumentTypes
        {
            get
            {
                if (_documentTypes != null)
                    return _documentTypes;

                _documentTypes = mmApp.Configuration.EditorExtensionMappings
                        .Select(kv => kv.Value)
                        .Distinct()
                        .OrderBy(s=> s.ToLower())
                        .ToList();

                return _documentTypes;
            }
        }
        List<string> _documentTypes = null;


        /// <summary>
        /// Returns the width of the column containing
        /// the Markdown Parser selection combo box
        /// </summary>
        public int MarkdownParserColumnWidth
        {
            get
            {
                if (MarkdownParserFactory.GetParserNames().Count > 1)
                    return 180;

                return 0;
            }
        }

#endregion

#region Initialization

        public AppModel(MainWindow window)
        {

            Configuration = mmApp.Configuration;
            _openDocuments = new List<MarkdownDocument>();
            Window = window;

            Commands = new AppCommands(this);
            mmApp.Model = this;


        }

#endregion




#region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

#endregion

    }

    public class PreviewSyncModeItem
    {
        public string Name { get; set; }
        public PreviewSyncMode Value { get; set; }
        public FontAwesomeIcon Icon { get; set; }
        public string IconString { get; set; }
    }


}
