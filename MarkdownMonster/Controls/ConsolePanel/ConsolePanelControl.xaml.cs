using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
            Write(text, color);
            ConsolePanelText.Inlines.Add(new LineBreak());
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

            var run = new Run()
            {
                Text = text
            };
            if (color != null)
                run.Foreground = color;

            ConsolePanelText.Inlines.Add(run);

            //Model.WindowLayout.ConsoleText += text;
            ConsolePanelScroll.ScrollToVerticalOffset(99999999);
        }

        public void Write(string text, ConsoleColor color)
        {
            var brush = ConsoleColorToBrush(color);
            Write(text, brush);
        }

        public void WriteLine(string text, ConsoleColor color)
        {
            var brush = ConsoleColorToBrush(color);
            WriteLine(text, brush);
        }

        /// <summary>
        /// Clears the content of the console window.
        /// </summary>
        public void Clear()
        {
            ConsolePanelText.Inlines.Clear();
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


        private Brush ConsoleColorToBrush(ConsoleColor color)
        {
            Brush brush = Brushes.Black;
            switch (color)
            {
                case ConsoleColor.Red:
                    brush = Brushes.Red;
                    break;
                case ConsoleColor.Green:
                    brush = Brushes.Green;
                    break;
                case ConsoleColor.Yellow:
                    brush = Brushes.Yellow;
                    break;
                case ConsoleColor.Blue:
                    brush = Brushes.Blue;
                    break;
                case ConsoleColor.White:
                    brush = Brushes.White;
                    break;
                case ConsoleColor.Gray:
                    brush = Brushes.Gray;
                    break;
                case ConsoleColor.Cyan:
                    brush = Brushes.Cyan;
                    break;
                case ConsoleColor.Magenta:
                    brush = Brushes.Magenta;
                    break;
                case ConsoleColor.DarkGreen:
                    brush = Brushes.DarkGreen;
                    break;
                case ConsoleColor.DarkMagenta:
                    brush = Brushes.DarkMagenta;
                    break;
                case ConsoleColor.DarkBlue:
                    brush = Brushes.DarkBlue;
                    break;
                case ConsoleColor.DarkGray:
                    brush = Brushes.DarkGray;
                    break;
                case ConsoleColor.DarkYellow:
                    brush = Brushes.Goldenrod;
                    break;
                case ConsoleColor.DarkCyan:
                    brush = Brushes.DarkCyan;
                    break;
                case ConsoleColor.DarkRed:
                    brush = Brushes.DarkRed;
                    break;
            }

            return brush;
        }


    }
}
