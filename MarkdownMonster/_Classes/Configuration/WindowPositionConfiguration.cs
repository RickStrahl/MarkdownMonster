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



        public Visibility TabHeadersVisible
        {
            get { return _TabHeadersVisible; }
            set
            {
                if (value == _TabHeadersVisible) return;
                _TabHeadersVisible = value;
                OnPropertyChanged(nameof(TabHeadersVisible));
            }
        }
        private Visibility _TabHeadersVisible = Visibility.Visible;


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