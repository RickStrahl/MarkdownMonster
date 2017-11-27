using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for PreviewBrowserWindow.xaml
    /// </summary>
    public partial class PreviewBrowserWindow : MetroWindow, IPreviewBrowser
    {

        public PreviewWebBrowser PreviewBrowser;

        public bool IsClosed { get; set; }

        public AppModel Model { get; set; }

        public PreviewBrowserWindow()
        {
            InitializeComponent();

            mmApp.SetThemeWindowOverride(this);

            PreviewBrowser = new PreviewWebBrowser(Browser);

            SetWindowPositionFromConfig();

            Model = mmApp.Model;
            DataContext = Model;
        }

        public void SetWindowPositionFromConfig()
        {
            var config = mmApp.Model.Configuration.WindowPosition;

            Left = config.PreviewLeft;
            Top = config.PreviewTop;
            Width = config.PreviewWidth;
            Height = config.PreviewHeight;


            FixMonitorPosition();
        }

        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor, bool keepScrollPosition)
        {
            PreviewBrowser.PreviewMarkdownAsync(editor, keepScrollPosition);
        }

        public void PreviewMarkdown(MarkdownDocumentEditor editor, bool keepScrollPosition, bool showInBrowser)
        {
            PreviewBrowser.PreviewMarkdown(editor, keepScrollPosition);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            IsClosed = true;

            var config = mmApp.Model.Configuration.WindowPosition;

            config.PreviewLeft = Convert.ToInt32(Left);
            config.PreviewTop = Convert.ToInt32(Top);
            config.PreviewWidth = Convert.ToInt32(Width);
            config.PreviewHeight = Convert.ToInt32(Height);

        }


        /// <summary>
        /// Check to see if the window is visible in the bounds of the
        /// virtual screen space. If not adjust to main monitor off 0 position.
        /// </summary>
        /// <returns></returns>
        void FixMonitorPosition()
        {
            var virtualScreenHeight = SystemParameters.VirtualScreenHeight;
            var virtualScreenWidth = SystemParameters.VirtualScreenWidth;


            if (Left > virtualScreenWidth - 250)
                Left = 20;
            if (Top > virtualScreenHeight - 250)
                Top = 20;

            if (Left < SystemParameters.VirtualScreenLeft)
                Left = SystemParameters.VirtualScreenLeft;
            if (Top < SystemParameters.VirtualScreenTop)
                Top = SystemParameters.VirtualScreenTop;

            if (Width > virtualScreenWidth)
                Width = virtualScreenWidth - 40;
            if (Height > virtualScreenHeight)
                Height = virtualScreenHeight - 40;
        }

        private void Button_Handler(object sender, RoutedEventArgs e)
        {
            Model.Window.Button_Handler(Model.Window.MenuItemPreviewConfigureSync, null);
        }
    }
}
