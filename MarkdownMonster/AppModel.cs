using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Win32;

namespace MarkdownMonster
{
    public class AppModel : INotifyPropertyChanged
    {
        private string _activeDocumentTitle;
        private MarkdownDocument _activeDocument;
        private List<MarkdownDocument> _openDocuments;
        private string _markdownEditAction;

        public MainWindow Window { set; get; }

        public ApplicationConfiguration Configuration { get; set; }

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

        public bool IsEditorActive
        {
            get
            {
                if (ActiveDocument != null)
                    return true;

                return false;
            }            
        }

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

        public CommandBase ToolbarInsertMarkdownCommand { get; set; }

        public CommandBase SettingsCommand { get; set; }

        public CommandBase TabItemClosedCmd  { get; set; }

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
                else
                    doc.SaveDocument();

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

                SaveFileDialog sd = new SaveFileDialog
                {
                    Filter = "Markdown files (*.md)|*.md|All files (*.*)|*.*",
                    FilterIndex = 1,
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
                    doc.SaveDocument();
                }

                Window.PreviewMarkdown(doc, keepScrollPosition: true);
            }, (s, e) =>
            {
                if (ActiveDocument == null)
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


                // show or hide preview browser
                Window.ShowPreviewBrowser(!IsPreviewBrowserVisible);


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

                Window.OpenTab(file, syntax: "json");
            }, null);

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

}
