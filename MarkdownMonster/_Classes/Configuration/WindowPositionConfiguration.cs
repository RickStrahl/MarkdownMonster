using System.Windows;

namespace MarkdownMonster
{
    /// <summary>
    /// Holds the current Window position and splitter settings
    /// </summary>
    public class WindowPositionConfiguration
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        /// Hold last window state.
        /// </summary>
        public WindowState WindowState { get; set; }

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
    }
}