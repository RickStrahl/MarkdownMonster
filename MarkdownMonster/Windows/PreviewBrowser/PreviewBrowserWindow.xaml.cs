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

            Model = mmApp.Model;
            DataContext = Model;
        
            SetWindowPositionFromConfig();
        }
        
        public void SetWindowPositionFromConfig()
        {
            var config = mmApp.Model.Configuration.WindowPosition;

            Left = config.PreviewLeft;
            Top = config.PreviewTop;
            Width = config.PreviewWidth;
            Height = config.PreviewHeight;

            Topmost = config.PreviewAlwaysOntop;
            if (config.PreviewDocked)
                AttachDockingBehavior();

            FixMonitorPosition();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            IsClosed = true;

            var config = mmApp.Model.Configuration.WindowPosition;

            config.PreviewLeft = Convert.ToInt32(Left);
            config.PreviewTop = Convert.ToInt32(Top);
            config.PreviewWidth = Convert.ToInt32(Width);
            config.PreviewHeight = Convert.ToInt32(Height);

            AttachDockingBehavior(true);
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


        #region AlwaysOnTop and Docking Behaviors
        public void AttachDockingBehavior(bool turnOn = true)
        {
            if (turnOn)
            {
                Model.Window.LocationChanged += Window_LocationChanged;
                Model.Window.SizeChanged += Window_SizeChanged;
                DockToMainWindow();
                FixMonitorPosition();
            }
            else
            {
                Model.Window.LocationChanged -= Window_LocationChanged;
                Model.Window.SizeChanged -= Window_SizeChanged;
            }
        }


        public void DockToMainWindow()
        {
            Left = Model.Window.Left + Model.Window.Width + 5;
            Top = Model.Window.Top;
            Height = Model.Window.Height;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DockToMainWindow();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            DockToMainWindow();
        }


        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor, bool keepScrollPosition)
        {
            PreviewBrowser.PreviewMarkdownAsync(editor, keepScrollPosition);
        }

        public void PreviewMarkdown(MarkdownDocumentEditor editor, bool keepScrollPosition, bool showInBrowser)
        {
            PreviewBrowser.PreviewMarkdown(editor, keepScrollPosition);
        }


        private void CheckPreviewAlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            if (Model.Configuration.WindowPosition.PreviewAlwaysOntop)
                Topmost = true;
            else
                Topmost = false;
        }

        private void CheckPreviewDocked_Click(object sender, RoutedEventArgs e)
        {
            if (Model.Configuration.WindowPosition.PreviewDocked)
                AttachDockingBehavior();
            else
                AttachDockingBehavior(false);
        }
        #endregion
    }
}
