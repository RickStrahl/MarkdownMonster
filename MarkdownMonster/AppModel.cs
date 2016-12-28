using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using FontAwesome.WPF;
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
        //private string _activeDocumentTitle;
        private MarkdownDocument _activeDocument;
        private List<MarkdownDocument> _openDocuments;
        private string _markdownEditAction;


        /// <summary>
        /// An instance of the main application WPF form
        /// </summary>
        public MainWindow Window { set; get; }

        /// <summary>
        /// The application's main configuration object
        /// </summary>
        public ApplicationConfiguration Configuration { get; set; }

        /// <summary>
        /// Determines whether the preview browser is visible or not
        /// </summary>
        public bool IsPreviewBrowserVisible
        {
            get { return Configuration.IsPreviewVisible; }
            set
            {
                if (value == Configuration.IsPreviewVisible) return;
                Configuration.IsPreviewVisible = value;
                OnPropertyChanged(nameof(IsPreviewBrowserVisible));
            }
        }

        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            set
            {
                if (value == _isFullScreen) return;
                _isFullScreen = value;
                OnPropertyChanged(nameof(IsFullScreen));
            }
        }
        private bool _isFullScreen = false;


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
        /// 
        /// </summary>
        public string MarkdownEditAction
        {
            get { return _markdownEditAction; }
            set
            {
                if (value == _markdownEditAction) return;
                _markdownEditAction = value;
                OnPropertyChanged(nameof(MarkdownEditAction));
            }
        }


        /// <summary>
        /// Returns the MarkdownDocument instance of the active editor
        /// </summary>
        public MarkdownDocument ActiveDocument
        {
            get { return _activeDocument; }
            set
            {                
                if (value == _activeDocument)
                    return;

                _activeDocument = value;

                if (_activeDocument != null)
                {
                    _activeDocument.PropertyChanged += (a, b) =>
                    {
                        if (b.PropertyName == nameof(_activeDocument.IsDirty))                        
                            SaveCommand.InvalidateCanExecute();                        
                    };
                }
                OnPropertyChanged(nameof(ActiveDocument));
                OnPropertyChanged(nameof(ActiveEditor));
                OnPropertyChanged(nameof(IsEditorActive));

                SaveCommand.InvalidateCanExecute();
                SaveAsHtmlCommand.InvalidateCanExecute();
            }
        }

        /// <summary>
        /// Returns an instance of the active Markdown Editor 
        /// WebBrowser control which also contains a number
        /// of support features that can access the underlying
        /// AceEditor document itself.
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
        /// Gives a list of all the open documents as Markdown document instances
        /// </summary>
        public List<MarkdownDocument> OpenDocuments
        {
            get { return _openDocuments; }
            set
            {
                if (Equals(value, _openDocuments)) return;
                _openDocuments = value;
                OnPropertyChanged(nameof(OpenDocuments));
            }
        }


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
                    var directories = Directory.GetDirectories(Path.Combine(Environment.CurrentDirectory, "PreviewThemes"));
                    foreach (string dir in directories.OrderBy( d => d))
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

                _previewSyncModeItems = ( (PreviewSyncMode[])Enum.GetValues(typeof(PreviewSyncMode)))
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
                    var files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Editor\\Scripts\\Ace"),"theme-*.js");
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

        #region Initialization

        public AppModel(MainWindow window)
        {

            Configuration = mmApp.Configuration;
            _openDocuments = new List<MarkdownDocument>();
            Window = window;

            CreateCommands();
        }

        #endregion


        #region Commands

        public CommandBase PreviewBrowserCommand { get; set; }

        public CommandBase SaveCommand { get; set; }

        public CommandBase SaveAsCommand { get; set; }

        public CommandBase SaveAsHtmlCommand { get; set; }

        public CommandBase ToolbarInsertMarkdownCommand { get; set; }

        public CommandBase SettingsCommand { get; set; }

        public CommandBase TabItemClosedCmd  { get; set; }

        public CommandBase DistractionFreeModeCommand { get; set; }

        public CommandBase PrintPreviewCommand { get; set; }

        private void CreateCommands()
        {
            // SAVE COMMAND
            SaveCommand = new CommandBase((s, e) =>
            {
                var tab = Window.TabControl?.SelectedItem as TabItem;
                if (tab == null)
                    return;
                var doc = tab.Tag as MarkdownDocumentEditor;

                if (doc.MarkdownDocument.Filename == "untitled")
                    SaveAsCommand.Execute(tab);
                else if (!doc.SaveDocument())
                {
                    SaveAsCommand.Execute(tab);
                }

                Window.PreviewMarkdown(doc, keepScrollPosition: true);

            }, (s, e) =>
            {
                if (ActiveDocument == null)
                    return false;

                return this.ActiveDocument.IsDirty;
            });

            // SAVEAS COMMAND
            SaveAsCommand = new CommandBase((s, e) =>
            {
                var tab = Window.TabControl?.SelectedItem as TabItem;
                if (tab == null)
                    return;
                var doc = tab.Tag as MarkdownDocumentEditor;
                if (doc == null)
                    return;

                var folder = Path.GetDirectoryName(doc.MarkdownDocument.Filename);
                if (doc.MarkdownDocument.Filename == "untitled")                
                    folder = mmApp.Configuration.LastFolder;

                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                    folder = KnownFolders.GetPath(KnownFolder.Libraries);
                
                SaveFileDialog sd = new SaveFileDialog
                {
                    Filter = "Markdown files (*.md)|*.md|All files (*.*)|*.*",
                    FilterIndex = 1,
                    InitialDirectory=folder,
                    FileName = doc.MarkdownDocument.Filename,
                    CheckFileExists = false,
                    OverwritePrompt = false,
                    CheckPathExists = true,
                    RestoreDirectory = true
                };

                var result = sd.ShowDialog();
                if (result != null && result.Value)
                {
                    doc.MarkdownDocument.Filename = sd.FileName;
                    if (!doc.SaveDocument())
                    {
                        MessageBox.Show(Window, $"{sd.FileName}\r\n\r\nThis document can't be saved in this location. The file is either locked or you don't have permissions to save it. Please choose another location to save the file.",
                            "Unable to save Document",MessageBoxButton.OK,MessageBoxImage.Warning);
                        SaveAsCommand.Execute(tab);
                        return;
                    }
                }

                Window.PreviewMarkdown(doc, keepScrollPosition: true);
            }, (s, e) =>
            {
                if (ActiveDocument == null)
                    return false;

                return true;
            });

            // SAVEASHTML COMMAND
            SaveAsHtmlCommand = new CommandBase((s, e) =>
            {
                var tab = Window.TabControl?.SelectedItem as TabItem;
                if (tab == null)
                    return;
                var doc = tab.Tag as MarkdownDocumentEditor;
                if (doc == null)
                    return;

                var folder = Path.GetDirectoryName(doc.MarkdownDocument.Filename);

                SaveFileDialog sd = new SaveFileDialog
                {
                    Filter = "Html files (Html only) (*.html)|*.html|Html files (Html and dependencies in a folder)|*.html",
                    FilterIndex = 1,
                    InitialDirectory = folder,
                    FileName = Path.ChangeExtension(doc.MarkdownDocument.Filename,"html"),
                    CheckFileExists = false,
                    OverwritePrompt = false,
                    CheckPathExists = true,
                    RestoreDirectory = true                    
                };

                var result = sd.ShowDialog();
                
                if (result != null && result.Value)
                {
                    if (sd.FilterIndex != 2)
                    {
                        var html = doc.RenderMarkdown(doc.GetMarkdown());

                        if (!doc.MarkdownDocument.WriteFile(sd.FileName, html))
                        {
                            MessageBox.Show(Window,
                                $"{sd.FileName}\r\n\r\nThis document can't be saved in this location. The file is either locked or you don't have permissions to save it. Please choose another location to save the file.",
                                "Unable to save Document", MessageBoxButton.OK, MessageBoxImage.Warning);
                            SaveAsHtmlCommand.Execute(null);
                            return;
                        }
                    }
                    else
                    {
                        string msg = @"This feature is not available yet.

For now, you can use 'View in Web Browser' to view the document in your favorite Web Browser and use 'Save As...' to save the Html document with all CSS and Image dependencies.

Do you want to View in Browser now?
";
                       var mbResult = MessageBox.Show(msg, 
                            mmApp.ApplicationName, 
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Asterisk,
                            MessageBoxResult.Yes);

                        if(mbResult == MessageBoxResult.Yes)
                            Window.ButtonViewInBrowser_Click(Window, null);
                    }
                }

                Window.PreviewMarkdown(doc, keepScrollPosition: true);
            }, (s, e) =>
            {
                if (ActiveDocument == null || ActiveEditor == null)
                    return false;
                if (ActiveDocument.Filename == "untitled")
                    return true;
                if (ActiveEditor.EditorSyntax != "markdown")
                    return false;

                return true;
            });

            // PREVIEW BUTTON COMMAND
            PreviewBrowserCommand = new CommandBase((s, e) =>
            {
                var tab = Window.TabControl.SelectedItem as TabItem;
                if (tab == null)
                    return;

                var editor = tab.Tag as MarkdownDocumentEditor;

                Configuration.IsPreviewVisible = IsPreviewBrowserVisible;

                Window.ShowPreviewBrowser(!IsPreviewBrowserVisible);
                if (IsPreviewBrowserVisible)
                    Window.PreviewMarkdown(editor);

            }, null);

            // MARKDOWN EDIT COMMANDS TOOLBAR COMMAND
            ToolbarInsertMarkdownCommand = new CommandBase((s, e) =>
            {
                var editor = Window.GetActiveMarkdownEditor();
                if (editor == null || editor.AceEditor == null)
                    return;

                string action = s as string;

                editor.ProcessEditorUpdateCommand(action);
            }, null);

            // Settings
            SettingsCommand = new CommandBase((s, e) =>
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
            }, null);

            // DISTRACTION FREE MODE
            DistractionFreeModeCommand = new CommandBase((s, e) =>
            {                
                GridLength gl = new GridLength(0);
                if (Window.WindowGrid.RowDefinitions[1].Height == gl)
                {
                    gl = new GridLength(30);
                    IsPreviewBrowserVisible = true;
                    Window.PreviewMarkdown();
                    IsFullScreen = false;
                }
                else
                {
                    IsPreviewBrowserVisible = false;
                    Window.ShowPreviewBrowser(hide: true);
                    IsFullScreen = true;                    
                }
                
                Window.WindowGrid.RowDefinitions[1].Height = gl;
                //Window.WindowGrid.RowDefinitions[3].Height = gl;  
            }, null);

            // PRINT PREVIEW
            PrintPreviewCommand = new CommandBase((s, e) =>
            {
                dynamic dom = Window.PreviewBrowser.Document;
                dom.execCommand("print",true,null);
            }, (s, e) => IsPreviewBrowserVisible);
        }


        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
