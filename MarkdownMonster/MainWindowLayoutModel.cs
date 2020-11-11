
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using MarkdownMonster.Annotations;

namespace MarkdownMonster.Windows {
    /// <summary>
    /// Class that manages the display of the center panel of panes in the Main Window
    /// </summary>
    public class MainWindowLayoutModel : INotifyPropertyChanged
    {
        private MainWindow Window;
        private AppModel Model;

        //private  GridLength DefaultSeparatorWidth = new GridLength(12);
        //private GridLength ZeroWidth = new GridLength(0);

        public MainWindowLayoutModel(MainWindow mainWindow)
        {
            Window = mainWindow;
            Model = mainWindow.Model;            
            

            if (Model.Configuration != null)
                PreviewWidth = new GridLength(Model.Configuration.WindowPosition.InternalPreviewWidth);
        }

        #region Editor Container
        public bool IsEditorOpen
        {
            get => _isEditorOpen;
            set
            {
                if (value == _isEditorOpen) return;
                _isEditorOpen = value;
                OnPropertyChanged();
            }
        }
        private bool _isEditorOpen;


        public bool IsConsolePanelVisible
        {
            get { return _isConsolePanelVisible; }
            set
            {
                if (value == _isConsolePanelVisible) return;
                _isConsolePanelVisible = value;
                OnPropertyChanged(nameof(IsConsolePanelVisible));
            }
        }
        private bool _isConsolePanelVisible;



        public string ConsoleText
        {
            get { return _ConsoleText; }
            set
            {
                if (value == _ConsoleText) return;
                _ConsoleText = value;
                OnPropertyChanged(nameof(ConsoleText));
            }
        }
        private string _ConsoleText;


        public GridLength EditorWidth
        {
            get => _editorWidth;
            set
            {
                if (value.Equals(_editorWidth)) return;
                _editorWidth = value;
                OnPropertyChanged();
            }
        }

        private GridLength _editorWidth = GridLengthHelper.Star;

        #endregion


        #region LeftSidebar

        public bool IsLeftSidebarVisible
        {
            get => _isLeftSidebarVisible;
            set
            {
                if (!value)
                {
                    if (_leftSidebarWidth.Value > 20)
                        mmApp.Configuration.FolderBrowser.WindowWidth = Convert.ToInt32(_leftSidebarWidth.Value);

                    LeftSidebarWidth =  GridLengthHelper.Zero;
                    //LeftSidebarSeparatorWidth = GridLengthHelper.Zero;
                }
                else
                {
                    //LeftSidebarSeparatorWidth = GridLengthHelper.WindowSeparator;
                    LeftSidebarWidth = new GridLength(mmApp.Configuration.FolderBrowser.WindowWidth);
                    if (LeftSidebarWidth.Value < 20)
                        LeftSidebarWidth = new GridLength(300);                    
                }

                _isLeftSidebarVisible = value;
                OnPropertyChanged(nameof(IsLeftSidebarVisible));
            }
        }
        private bool _isLeftSidebarVisible = true;



        public GridLength LeftSidebarWidth
        {
            get => _leftSidebarWidth;
            set
            {
                if (value.Equals(_leftSidebarWidth)) return;
                _leftSidebarWidth = value;

                OnPropertyChanged(nameof(LeftSidebarWidth));

                if (_leftSidebarWidth.IsAbsolute && _leftSidebarWidth.Value < 1 && _isLeftSidebarVisible)
                {
                    _isLeftSidebarVisible = false;
                    OnPropertyChanged(nameof(IsLeftSidebarVisible));
                }
                else if (_leftSidebarWidth.IsAbsolute && _leftSidebarWidth.Value > 0 && !_isLeftSidebarVisible)
                {
                    _isLeftSidebarVisible = true;
                    OnPropertyChanged(nameof(IsLeftSidebarVisible));
                }                
            }
        }
        private GridLength _leftSidebarWidth;

        

        public GridLength LeftSidebarSeparatorWidth
        {
            get => _leftSidebarSeparatorWidth;
            set
            {
                if (value.Equals(_leftSidebarSeparatorWidth)) return;
                _leftSidebarSeparatorWidth = value;
                OnPropertyChanged(nameof(LeftSidebarSeparatorWidth));
            }
        }
        private GridLength _leftSidebarSeparatorWidth;

        #endregion


        #region Preview Browser
        public bool IsPreviewVisible
        {
            get => _isPreviewVisible;
            set
            {
                //if (value == _isPreviewVisible) return;

                _isPreviewVisible = value;
                OnPropertyChanged();

                if (value == false)
                {
                    if (PreviewWidth.IsAbsolute) 
                        mmApp.Configuration.WindowPosition.PreviewWidth = Convert.ToInt32(PreviewWidth.Value);                
                    PreviewWidth = GridLengthHelper.Zero;
                    PreviewSeparatorWidth = GridLengthHelper.Zero;                    
                }
                else
                {
                    if (mmApp.Configuration.WindowPosition.InternalPreviewWidth < 20)
                        PreviewWidth = new GridLength(350);
                    else
                        PreviewWidth = new GridLength(mmApp.Configuration.WindowPosition.InternalPreviewWidth);

                    PreviewSeparatorWidth = GridLengthHelper.Auto;
                    

                    if (PreviewWidth.IsAbsolute)
                        mmApp.Configuration.WindowPosition.PreviewWidth = Convert.ToInt32(PreviewWidth.Value);
                }

                EditorWidth = GridLengthHelper.Star;
            }
        }

        private bool _isPreviewVisible = true;


        public GridLength PreviewWidth
        {
            get => _previewWidth;
            set
            {
                if (value.Equals(_previewWidth)) return;
                _previewWidth = value;
                OnPropertyChanged();

                if (value.IsAbsolute && value.Value > 20)
                    Model.Configuration.WindowPosition.InternalPreviewWidth = Convert.ToInt32(_previewWidth.Value);


                // HACK: Don't allow preview width to get larger than the container grid.
                // Force resizing to actual width rather than START (*) sizing which causes window sizing/border issues
                try
                {
                    var editorPreviewPane = Model?.ActiveEditor?.EditorPreviewPane;
                    var contentGrid = editorPreviewPane?.ContentGrid;
                    if (contentGrid == null) return;
                    var sepWidth = editorPreviewPane.EditorWebBrowserSeparatorColumn.ActualWidth;
                    if (_previewWidth.Value > editorPreviewPane.ContentGrid.ActualWidth - sepWidth)
                    {
                        _previewWidth = new GridLength(editorPreviewPane.ContentGrid.ActualWidth - sepWidth);
                    }
                }
                catch { }
            }
        }

        private GridLength _previewWidth;

        

        public GridLength PreviewSeparatorWidth
        {
            get => _previewSeparatorWidth;
            set
            {
                if (value.Equals(_previewSeparatorWidth)) return;
                _previewSeparatorWidth = value;
                OnPropertyChanged();
            }
        }

        private GridLength _previewSeparatorWidth = GridLengthHelper.Auto;

        #endregion

        #region Right Sidebar

        public bool IsRightSidebarVisible
        {
            get => _isRightSidebarVisible;
            set
            {
                if (value == _isRightSidebarVisible) return;
                
                if (!value)
                {
                    if (_rightSidebarWidth.Value > 20)
                        mmApp.Configuration.WindowPosition.RightSidebardWidth = Convert.ToInt32(_rightSidebarWidth.Value);

                    RightSidebarWidth = GridLengthHelper.Zero;                    
                }
                else
                {
                    var width = mmApp.Configuration.WindowPosition.RightSidebardWidth;
                    if (width < 50)
                        mmApp.Configuration.WindowPosition.RightSidebardWidth = 300;

                    RightSidebarWidth = new GridLength(mmApp.Configuration.WindowPosition.RightSidebardWidth);
                }

                _isRightSidebarVisible = value;
                OnPropertyChanged(nameof(IsRightSidebarVisible));
                OnPropertyChanged(nameof(RightSidebarSeparatorWidth));
            }
        }
        private bool _isRightSidebarVisible = false;


        public GridLength RightSidebarWidth
        {
            get => _rightSidebarWidth;
            set
            {
                if (value.Equals(_rightSidebarWidth)) return;
                _rightSidebarWidth = value;
                OnPropertyChanged();

                if (_rightSidebarWidth.IsAbsolute && _rightSidebarWidth.Value < 1 && _isRightSidebarVisible)
                {
                    _isRightSidebarVisible = false;
                    OnPropertyChanged(nameof(IsRightSidebarVisible));
                }
                else if (_rightSidebarWidth.IsAbsolute && _rightSidebarWidth.Value > 0 && !_isRightSidebarVisible)
                {
                    _isRightSidebarVisible = true;
                    OnPropertyChanged(nameof(IsRightSidebarVisible));
                }
            }
        }

        private GridLength _rightSidebarWidth = GridLengthHelper.Zero;



        public GridLength RightSidebarSeparatorWidth
        {
            get
            {
                if (Window.RightSidebarContainer.Items.Count < 1)
                    return GridLengthHelper.Zero;

                return _rightSidebarSeparatorWidth;
            }
            set
            {
                //if (value.Equals(_rightSidebarSeparatorWidth)) return;
                _rightSidebarSeparatorWidth = value;
                OnPropertyChanged();
            }
        }

        private GridLength _rightSidebarSeparatorWidth = GridLengthHelper.Auto;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Major Display Mode Operations

        private int savedEditorWidth = 0;

        public void SetDistractionFreeMode(bool hide = false)
        {
            GridLength glToolbar = GridLengthHelper.Zero;
            GridLength glMenu = GridLengthHelper.Zero;
            GridLength glStatus = GridLengthHelper.Zero; 
            
            if (hide)
            {
                Window.SaveSettings();

                glToolbar = GridLength.Auto;
                glMenu = GridLength.Auto;
                glStatus = GridLength.Auto;

                //mmApp.Configuration.WindowPosition.IsTabHeaderPanelVisible = true;
                Window.TabControl.IsHeaderPanelVisible = true;

                Model.IsPreviewBrowserVisible = true;
                Window.PreviewMarkdown();

                Window.WindowState = mmApp.Configuration.WindowPosition.WindowState;

                Model.IsFullScreen = false;

                Window.ShowFolderBrowser(!mmApp.Configuration.FolderBrowser.Visible);

                Model.Configuration.Editor.CenteredModeMaxWidth = savedEditorWidth;
                Model.ActiveEditor?.RestyleEditor();
                Model.ActiveEditor?.AceEditor?.AdjustPadding(true);
            }
            else
            {
                savedEditorWidth = Model.Configuration.Editor.CenteredModeMaxWidth;

                // normalize first if we're in presentation mode
                if (Model.IsPresentationMode)                
                    SetPresentationMode(hide: true);
                
                var tokens = mmApp.Configuration.DistractionFreeModeHideOptions.ToLower()
                    .Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);

                if (tokens.All(d => d != "menu"))
                    glMenu = GridLength.Auto;

                if (tokens.All(d => d != "toolbar"))
                    glToolbar = GridLength.Auto;

                if (tokens.All(d => d != "statusbar"))
                    glStatus = GridLength.Auto;

                if (tokens.Any(d => d == "tabs"))
                    Window.TabControl.IsHeaderPanelVisible = false;

                if (tokens.Any(d => d == "preview"))
                {
                    Model.IsPreviewBrowserVisible = false;
                    Window.ShowPreviewBrowser(hide: true);
                }

                mmApp.Configuration.WindowPosition.WindowState = Window.WindowState;
                if (tokens.Any(d => d == "maximized"))
                    Window.WindowState = WindowState.Maximized;

                if (tokens.Any(d => d.StartsWith("maxwidth:")))
                {
                    var token = tokens.First(d => d.StartsWith("maxwidth:"));
                    token = token.Replace("maxwidth:", "");

                    if (int.TryParse(token, out int width))
                    {
                        Model.Configuration.Editor.CenteredModeMaxWidth = width;
                        Model.ActiveEditor?.RestyleEditor();
                    }
                }

                Window.ShowFolderBrowser(true);

                Model.IsFullScreen = true;
            }

            // toolbar     
            Window.MainMenuGridRow.Height = glMenu;
            Window.ToolbarGridRow.Height = glToolbar;
            Window.StatusBarGridRow.Height = glStatus;
        }

        public void SetPresentationMode(bool hide = false)
        {
            if (Model.IsFullScreen)
                SetDistractionFreeMode(hide: true);

            var layout = Model.WindowLayout;
            if (hide)
            {
                layout.IsEditorOpen = true;
                layout.EditorWidth = GridLengthHelper.Star;
                layout.PreviewWidth = new GridLength(mmApp.Configuration.WindowPosition.InternalPreviewWidth);
                layout.LeftSidebarSeparatorWidth = GridLengthHelper.WindowSeparator;

                Window.MainWindowEditorColumn.Width = GridLengthHelper.Star;
                layout.RightSidebarSeparatorWidth = GridLengthHelper.Zero;

                layout.IsLeftSidebarVisible = mmApp.Configuration.FolderBrowser.Visible;
                //Model.WindowLayout.IsRightSidebarVisible = false;

                layout.PreviewWidth = new GridLength(mmApp.Configuration.WindowPosition.InternalPreviewWidth);
                layout.EditorWidth = GridLengthHelper.Star;

                // make toolbar visible again
                Model.Window.ToolbarGridRow.Height = GridLengthHelper.Auto;
                Window.TabControl.IsHeaderPanelVisible = true;
                Window.StatusBarGridRow.Height = GridLengthHelper.Auto;

                WindowUtilities.DoEvents();

                //window.MainWindowPreviewColumn.Width =
                //    new GridLength(mmApp.Configuration.WindowPosition.SplitterPosition);

                Window.PreviewMarkdown();

                WindowUtilities.DoEvents();

                Window.ShowFolderBrowser(!mmApp.Configuration.FolderBrowser.Visible);

                WindowUtilities.DoEvents();

                Model.IsPresentationMode = false;
            }
            else
            {
                Window.SaveSettings();

                if(Model.Configuration.PreviewMode != PreviewModes.InternalPreview)
                    Model.Commands.PreviewModesCommand.Execute("InternalPreview");
                
                // force internal preview to become active
                Model.Window.Dispatcher.Invoke(() =>
                {
                    //mmApp.Configuration.WindowPosition.SplitterPosition =
                    //    Convert.ToInt32(window.MainWindowPreviewColumn.Width.Value);

                    // don't allow presentation mode for non-Markdown documents
                    var editor = Window.GetActiveMarkdownEditor();
                    if (editor != null)
                    {
                        var file = editor.MarkdownDocument.Filename.ToLower();
                        var ext = Path.GetExtension(file).Replace(".", "");

                        Model.Configuration.EditorExtensionMappings.TryGetValue(ext, out string mappedTo);
                        mappedTo = mappedTo ?? string.Empty;
                        if (file != "untitled" && mappedTo != "markdown" && mappedTo != "html")
                        {
                            // don't allow presentation mode for non markdown files
                            Model.IsPresentationMode = false;
                            Model.IsPreviewBrowserVisible = false;
                            Window.ShowPreviewBrowser(true);
                            return;
                        }
                    }

                    layout.IsLeftSidebarVisible = false;
                    layout.IsRightSidebarVisible = false;
                    layout.PreviewWidth = GridLengthHelper.Star;
                    layout.EditorWidth = GridLengthHelper.Zero;

                    Model.Window.ToolbarGridRow.Height = GridLengthHelper.Zero;
                    Window.TabControl.IsHeaderPanelVisible = false;
                    Window.StatusBarGridRow.Height = GridLengthHelper.Zero;

                    //window.ShowPreviewBrowser();
                    Window.ShowFolderBrowser(true);

                    Model.IsPresentationMode = true;
                    Model.IsPreviewBrowserVisible = true;
                },System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }            
        }
        #endregion


    }


    public class GridLengthHelper
    {
        public static GridLength Zero { get; } = new GridLength(0);
        public static GridLength Star { get; } = new GridLength(0.5, GridUnitType.Star);

        public static GridLength Auto
        {
            get => GridLength.Auto;
        }
        public static GridLength WindowSeparator { get; } = new GridLength(12);

        public static GridLength FromDouble(double size)
        {
            return new GridLength(size);            
        }
        public static GridLength FromInt(double size)
        {
            return new GridLength(size);
        }

        /// <summary>
        /// Converts a GridLength to an Int Value checking for
        /// IsAbsolute and if not returning -1
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Int value or Int.MinValue if not absolute</returns>
        public static int ParseInt(GridLength value)
        {
            if (value.IsAbsolute)
                return Convert.ToInt32(value.Value);

            return int.MinValue;            
        }


        public static bool TryParseInt(GridLength value, out int intValue)
        {
            intValue = -1;
            if (!value.IsAbsolute)
                return false;

            intValue = Convert.ToInt32(value.Value);
            return true;
        }
    }

}
