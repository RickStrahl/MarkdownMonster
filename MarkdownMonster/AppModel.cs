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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using FontAwesome.WPF;
using MarkdownMonster.AddIns;
using MarkdownMonster.Windows;
using Microsoft.Win32;
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

        #region Top Level Model Properties

        /// <summary>
        /// An instance of the main application WPF form
        /// </summary>
        public MainWindow Window { set; get; }

        /// <summary>
        /// The application's main configuration object
        /// </summary>
        public ApplicationConfiguration Configuration { get; set; }

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
        /// Returns the MarkdownDocument instance of the active editor
        /// </summary>
        public MarkdownDocument ActiveDocument
        {
            get => _activeDocument;
            set
            {
                if (value == _activeDocument)
                    return;

                _activeDocument = value;

                OnPropertyChanged(nameof(ActiveDocument));
                OnPropertyChanged(nameof(ActiveEditor));                
                OnPropertyChanged(nameof(IsEditorActive));

                Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    CommandManager.InvalidateRequerySuggested();
                    //SaveCommand.InvalidateCanExecute();
                    //SaveAsHtmlCommand.InvalidateCanExecute();
                });

                Window.ToolbarEdit.IsEnabled = IsEditorActive;
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
                bool value = Window.TabControl.Items.Count < 1;
                if (value)
                {
                    var zeroWidth = new GridLength(0);
                    Window.MainWindowPreviewColumn.Width = zeroWidth;
                    Window.MainWindowSeparatorColumn.Width = zeroWidth;
                }
                return value;
            }
        }
        

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
        /// Determines if there's a document loaded 
        /// </summary>
        public bool IsEditorActive
        {
            get
            {
                if (ActiveDocument != null)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Commands
        /// </summary>
        public AppCommands Commands { get; } 

        #endregion


        #region Statusbar Item Props

        /// <summary>
        /// A list of RenderThemes as retrieved based on the folder structure of hte
        /// Preview folder.
        /// </summary>
        public List<string> RenderThemeNames
        {
            get
            {
                if (_renderThemeNames == null || _renderThemeNames.Count < 1)
                {
                    var directories =
                        Directory.GetDirectories(Path.Combine(Environment.CurrentDirectory, "PreviewThemes"));
                    foreach (string dir in directories.OrderBy(d => d))
                    {
                        var dirName = Path.GetFileName(dir);

                        if (dirName.ToLower() == "scripts")
                            continue;

                        _renderThemeNames.Add(dirName);
                    }
                }

                return _renderThemeNames;
            }
        }

        private readonly List<string> _renderThemeNames = new List<string>();





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
                    var files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Editor\\Scripts\\Ace"),
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
            CreateCommands();

            mmApp.Model = this;
            

        }

        #endregion


        #region Commands
       
  
        
        public CommandBase SettingsCommand { get; set; }

        void Command_Settings()
        {
            // Settings
            SettingsCommand = new CommandBase((s, e) =>
            {
                try
                {
                    var file = Path.Combine(mmApp.Configuration.CommonFolder, "MarkdownMonster.json");

                    // save settings first so we're looking at current setting
                    Configuration.Write();

                    string fileText = File.ReadAllText(file);
                    if (!fileText.StartsWith("//"))
                    {
                        fileText = "// Reference: http://markdownmonster.west-wind.com/docs/_4nk01yq6q.htm\r\n" +
                                   fileText;
                        File.WriteAllText(file, fileText);
                    }

                    Window.OpenTab(file, syntax: "json");
                }
                catch
                {
                    if (mmApp.Configuration.CommonFolder != mmApp.Configuration.InternalCommonFolder)
                    {
                        mmApp.Configuration.CommonFolder = mmApp.Configuration.InternalCommonFolder;
                        Command_Settings();
                    }
                    else
                    {
                        var msg = $@"We couldn't load the configuration file.

Please check that the configuration folder for Markdown Monster exists. The default location is:

{FileUtils.ExpandPathEnvironmentVariables("%appdata%\\Markdown Monster")}

and that the file contains `markdownmonster.json`. You should also remove `commonfolderlocation.txt` if it exists and points at an invalid location.

If all this fails shut down Markdown Monster, rename or delete `MarkdownMonster.json` and `commonfolderlocation.txt` (if it exists) and restart Markdown Monster.

We're now shutting down the application.
";
                        MessageBox.Show(msg, mmApp.ApplicationName, MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        App.Current.Shutdown();
                    }

                }
            }, null);
        }




                

        public CommandBase ViewInExternalBrowserCommand { get; set; }

        void Command_ViewInExternalBrowser()
        {
            // EXTERNAL BROWSER VIEW
            ViewInExternalBrowserCommand = new CommandBase((p, e) =>
            {
                if (ActiveDocument == null) return;

                ActiveDocument.RenderHtmlToFile();
                mmFileUtils.ShowExternalBrowser(ActiveDocument.HtmlRenderFilename);
            }, (p, e) =>  IsEditorActive);
        }

        public CommandBase ViewHtmlSourceCommand { get; set; }

        void Command_ViewHtmlSource()
        {
            ViewHtmlSourceCommand = new CommandBase((p, e) =>
            {
                if (ActiveDocument == null) return;
                ActiveDocument.RenderHtmlToFile();
                Window.OpenTab(ActiveDocument.HtmlRenderFilename);
            }, (p, e) => IsEditorActive);
        }


        public CommandBase PrintPreviewCommand { get; set; }

        void Command_PrintePreview()
        {
            PrintPreviewCommand = new CommandBase(
                (p, e) =>  Window.PreviewBrowser.ExecuteCommand("PrintPreview"),
                (p, e) => IsEditorActive);

        }


        public CommandBase ShowFolderBrowserCommand { get; set; }


        void Command_ShowFolderBrowser()
        {
            // SHOW FILE BROWSER COMMAND
            ShowFolderBrowserCommand = new CommandBase((s, e) =>
            {
                mmApp.Configuration.FolderBrowser.Visible = !mmApp.Configuration.FolderBrowser.Visible;
                mmApp.Model.Window.ShowFolderBrowser(!mmApp.Configuration.FolderBrowser.Visible);

            });
        }

        

        public CommandBase GeneratePdfCommand { get; set; }

        void Command_GeneratePdf()
        {
            // PDF GENERATION PREVIEW
            GeneratePdfCommand = new CommandBase((s, e) =>
            {
                var form = new GeneratePdfWindow()
                {
                    Owner = mmApp.Model.Window
                };
                form.Show();
            }, (s, e) => IsPreviewBrowserVisible);
        }


        public CommandBase CommitToGitCommand { get; set; }


        void Command_CommitToGit()
        {
            // COMMIT TO GIT Command
            CommitToGitCommand = new CommandBase(async (s, e) =>
            {
                string file = ActiveDocument?.Filename;
                if (string.IsNullOrEmpty(file))
                    return;

                Window.ShowStatus("Committing and pushing to Git...");
                WindowUtilities.DoEvents();

                string error = null;

                bool pushToGit = mmApp.Configuration.GitCommitBehavior == GitCommitBehaviors.CommitAndPush;
                bool result = await Task.Run(() => mmFileUtils.CommitFileToGit(file, pushToGit, out error));

                if (result)
                    Window.ShowStatus($"File {Path.GetFileName(file)} committed and pushed.", 6000);
                else
                {
                    Window.ShowStatus(error, 7000);
                    Window.SetStatusIcon(FontAwesomeIcon.Warning, Colors.Red);
                }
            }, (s, e) => IsEditorActive);
        }


        public CommandBase CopyAsHtmlCommand { get; set; }

        void Command_CopyAsHtml()
        {
            CopyAsHtmlCommand = new CommandBase((parameter, command) =>
            {
                if (ActiveEditor == null)
                    return;

                var editor = ActiveEditor;
                if (editor == null)
                    return;

                var markdown = editor.GetSelection();
                var html = editor.RenderMarkdown(markdown);

                if (!string.IsNullOrEmpty(html))
                {
                    // copy to clipboard as html
                    ClipboardHelper.CopyHtmlToClipboard(html, html);
                    Window.ShowStatus("Html has been pasted to the clipboard.", mmApp.Configuration.StatusTimeout);
                }
                editor.SetEditorFocus();
                editor.Window.PreviewMarkdownAsync();
            }, (p, c) => IsEditorActive);

        }



        private void CreateCommands()
        {
            Command_Settings();            
            
            Command_ViewInExternalBrowser();
            Command_ViewHtmlSource();
            Command_PrintePreview();

            Command_ShowFolderBrowser();
            
            Command_GeneratePdf();
            Command_CommitToGit();            
            Command_CopyAsHtml();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;


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
