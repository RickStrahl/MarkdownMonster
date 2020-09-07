using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MarkdownMonster.Annotations;

namespace MarkdownMonster
{
    /// <summary>
    /// Holds the current Window position and splitter settings
    /// </summary>
    public class WindowPositionConfiguration : INotifyPropertyChanged
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; } = 1100;
        public int Height { get; set; } = 640;

        public int InternalPreviewWidth { get; set; } = 450;

    
        public int PreviewTop { get; set; }
        public int PreviewLeft { get; set; }
        public int PreviewHeight { get; set; } = 700;
        public int PreviewWidth { get; set; } = 500;


        public PreviewWindowDisplayModes PreviewDisplayMode
        {
            get => _previewDisplayMode;
            set
            {
                if (value == _previewDisplayMode)
                    return;
                _previewDisplayMode = value;
                OnPropertyChanged();
            }
        }
        private PreviewWindowDisplayModes _previewDisplayMode = PreviewWindowDisplayModes.ActivatedByMainWindow;


        /// <summary>
        /// Determines whether the preview docks and moves with the main window
        /// when the main window is moved, sized, restored or hidden.
        /// </summary>
        public bool PreviewDocked
        {
            get { return _PreviewDocked; }
            set
            {
                if (value == _PreviewDocked) return;
                _PreviewDocked = value;
                OnPropertyChanged(nameof(PreviewDocked));
            }
        }

        private bool _PreviewDocked;




        /// <summary>
        /// X offset to the position of the splitter
        /// </summary>
        public int SplitterPosition
        {
            get { return _splitterPosition; }
            set
            {
                _splitterPosition = value;
            }
        }

        private int _splitterPosition;

        /// <summary>
        /// Determines the width of the right side bar
        /// </summary>
        public int RightSidebardWidth { get; set; } = 250;


        /// <summary>
        /// Height of the Console Output Panel on the bottom of the main window
        /// </summary>
        public double ConsolePanelHeight
        {
            get { return _ConsolePanelHeight; }
            set
            {
                if (value < 100)
                    value = 100;
                if (value == _ConsolePanelHeight) return;
                _ConsolePanelHeight = value;
                OnPropertyChanged(nameof(ConsolePanelHeight));
            }
        }
        private double _ConsolePanelHeight = 100;


        /// <summary>
        /// Determines if the tabs are visible
        /// </summary>
        public bool IsTabHeaderPanelVisible
        {
            get { return _IsTabHeaderPanelVisible; }
            set
            {
                if (value == _IsTabHeaderPanelVisible) return;
                _IsTabHeaderPanelVisible = value;
                OnPropertyChanged(nameof(IsTabHeaderPanelVisible));
            }
        }

        private bool _IsTabHeaderPanelVisible = true;
        

        /// <summary>
        /// Hold last window state.
        /// </summary>
        public WindowState WindowState { get; set; }


        /// <summary>
        /// A pre-defined list of Window Sizes you can resize
        /// Markdown Monster to with the COntrol menu.
        /// </summary>
        public HashSet<string> WindowSizes { get; set; }

        public WindowPositionConfiguration()
        {
            if (WindowSizes == null)
            {
                WindowSizes = new HashSet<string>()
                {
                    "2500 x 1750",
                    "2250 x 1500",
                    "1900 x 1020",
                    "1600 x 850",
                    "1250 x 680",
                    "950 x 620",
                    "770 x 560"
                };
            }
        }
        


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public enum PreviewWindowDisplayModes
    {
        ActivatedByMainWindow,
        AlwaysOnTop,
        ManualActivation
    }
}
