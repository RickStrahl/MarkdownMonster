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
        public int Width { get; set; }
        public int Height { get; set; }

        public int PreviewTop { get; set; }
        public int PreviewLeft { get; set; }
        public int PreviewHeight { get; set; } = 700;
        public int PreviewWidth { get; set; } = 1000;        


        public bool PreviewAlwaysOntop
        {
            get { return _PreviewAlwaysOntop; }
            set
            {
                if (value == _PreviewAlwaysOntop) return;
                _PreviewAlwaysOntop = value;
                OnPropertyChanged(nameof(PreviewAlwaysOntop));
            }
        }
        private bool _PreviewAlwaysOntop;


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
                //Debug.WriteLine(value);
            }   
        }
        private int _splitterPosition;
        

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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
