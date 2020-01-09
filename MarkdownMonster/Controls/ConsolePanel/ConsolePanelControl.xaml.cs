using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MarkdownMonster.Controls
{
    public partial class ConsolePanelControl : UserControl
    {

        public AppModel Model { get; set; }

        public ConsolePanelControl()
        {
            InitializeComponent();
            Loaded += ConsolePanelControl_Loaded;
        }

        private void ConsolePanelControl_Loaded(object sender, RoutedEventArgs e)
        {
            Model = mmApp.Model;
            DataContext = Model;
        }

        /// <summary>
        /// Writes a line with linefeed to the console
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public void WriteLine(string text, Brush color = null)
        {

            Write(text + "\n", color);
        }


        /// <summary>
        /// Writes text tot he console panel without a line feed.
        /// Next write continues on the active line
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public void Write(string text, Brush color = null)
        {
            if (!Model.WindowLayout.IsConsolePanelVisible)
                Show();

            Model.WindowLayout.ConsoleText += text;
            ConsolePanelScroll.ScrollToVerticalOffset(99999999);
        }

        /// <summary>
        /// Clears the content of the console window.
        /// </summary>
        public void Clear()
        {
            Model.WindowLayout.ConsoleText = null;
        }

        /// <summary>
        /// Makes the panel visible
        /// </summary>
        public void Show()
        {
            Model.WindowLayout.IsConsolePanelVisible = true;
            Model.Window.ConsolePanelGridRow.Height = new GridLength(Model.Configuration.WindowPosition.ConsolePanelHeight, GridUnitType.Pixel);
            Model.Window.ContentConsoleSplitterGridRow.Height = new GridLength(1, GridUnitType.Auto);
        }


        /// <summary>
        /// Hides the panel
        /// </summary>
        public void Hide()
        {
            Model.WindowLayout.IsConsolePanelVisible = false;
            Model.Configuration.WindowPosition.ConsolePanelHeight = Model.Window.ConsolePanelGridRow.Height.Value;
            Model.Window.ConsolePanelGridRow.Height = new GridLength(1, GridUnitType.Auto);
            Model.Window.ContentConsoleSplitterGridRow.Height = new GridLength(1, GridUnitType.Auto);
        }

    }
}
