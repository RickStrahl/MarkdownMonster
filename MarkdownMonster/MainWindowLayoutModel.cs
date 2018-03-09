
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MarkdownMonster.Annotations;

namespace MarkdownMonster.Windows {
    /// <summary>
    /// Class that manages the display of the center panel of panes in the Main Window
    /// </summary>
    public class MainWindowLayoutModel : INotifyPropertyChanged
    {
        private MainWindow Window;
        private AppModel Model;

        private  GridLength DefaultSeparatorWidth = new GridLength(12);
        private GridLength ZeroWidth = new GridLength(0);

        public MainWindowLayoutModel(MainWindow mainWindow)
        {
            Window = mainWindow;
            Model = mainWindow.Model;            

            if (Model.Configuration != null)
                PreviewWidth = new GridLength(Model.Configuration.WindowPosition.InternalPreviewWidth);
        }
        
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

        

        public double EditorWidth
        {
            get => _editorWidth;
            set
            {
                if (value.Equals(_editorWidth)) return;
                _editorWidth = value;
                OnPropertyChanged();
            }
        }
        private double _editorWidth;


        

        public bool IsLeftSidebarVisible
        {
            get => _isLeftSidebarVisible;
            set
            {
                if (value == _isLeftSidebarVisible) return;

                if (!value)
                {
                    if (_leftSidebarWidth.Value > 20)
                        mmApp.Configuration.FolderBrowser.WindowWidth = Convert.ToInt32(_leftSidebarWidth.Value);

                    LeftSidebarWidth = ZeroWidth;
                    LeftSidebarSeparatorWidth = ZeroWidth;                               
                }
                else
                {
                    LeftSidebarSeparatorWidth = DefaultSeparatorWidth;
                    LeftSidebarWidth = new GridLength(mmApp.Configuration.FolderBrowser.WindowWidth);
                    if (LeftSidebarWidth.Value < 20)
                        LeftSidebarWidth = new GridLength(300);
                    
                }

                _isLeftSidebarVisible = value;
                OnPropertyChanged(nameof(IsLeftSidebarVisible));
            }
        }
        private bool _isLeftSidebarVisible;



        public GridLength LeftSidebarWidth
        {
            get => _leftSidebarWidth;
            set
            {
                if (value.Equals(_leftSidebarWidth)) return;
                _leftSidebarWidth = value;
                OnPropertyChanged(nameof(LeftSidebarWidth));
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

        public bool IsPreviewVisible
        {
            get => _isPreviewVisible;
            set
            {
                if (value == _isPreviewVisible) return;
                _isPreviewVisible = value;
                OnPropertyChanged();

                if (value == false)
                {
                    if (PreviewWidth.IsAbsolute) 
                        mmApp.Configuration.WindowPosition.PreviewWidth = Convert.ToInt32(PreviewWidth.Value);                
                    PreviewWidth = ZeroWidth;
                    PreviewSeparatorWidth = ZeroWidth;
                }
                else
                {
                    if (mmApp.Configuration.WindowPosition.InternalPreviewWidth < 20)
                        PreviewWidth = new GridLength(350);
                    else
                        PreviewWidth = new GridLength(mmApp.Configuration.WindowPosition.InternalPreviewWidth);

                    PreviewSeparatorWidth = DefaultSeparatorWidth;

                    if (PreviewWidth.IsAbsolute)
                        mmApp.Configuration.WindowPosition.PreviewWidth = Convert.ToInt32(PreviewWidth.Value);
                }
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

        private GridLength _previewSeparatorWidth = new GridLength(12);

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

                    RightSidebarWidth = ZeroWidth;
                    RightSidebarSeparatorWidth = ZeroWidth;
                }
                else
                {
                    RightSidebarWidth = new GridLength(mmApp.Configuration.WindowPosition.RightSidebardWidth);
                    RightSidebarSeparatorWidth = new GridLength(DefaultSeparatorWidth.Value);
                }

                _isRightSidebarVisible = value;
                OnPropertyChanged(nameof(IsRightSidebarVisible));            
            }
        }
        private bool _isRightSidebarVisible;


        public GridLength RightSidebarWidth
        {
            get => _rightSidebarWidth;
            set
            {
                if (value.Equals(_rightSidebarWidth)) return;
                _rightSidebarWidth = value;
                OnPropertyChanged();
            }
        }
        private GridLength _rightSidebarWidth = new GridLength(0);



        public GridLength RightSidebarSeparatorWidth
        {
            get => _rightSidebarSeparatorWidth;
            set
            {
                if (value.Equals(_rightSidebarSeparatorWidth)) return;
                _rightSidebarSeparatorWidth = value;
                OnPropertyChanged();
            }
        }
        private GridLength _rightSidebarSeparatorWidth = new GridLength(0);

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


    }

}
