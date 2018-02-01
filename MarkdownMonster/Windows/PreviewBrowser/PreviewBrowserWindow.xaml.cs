using System;
using System.ComponentModel;
using System.Windows;
using MahApps.Metro.Controls;
using MarkdownMonster.AddIns;
using MarkdownMonster.Windows.PreviewBrowser;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for PreviewBrowserWindow.xaml
    /// </summary>
    public partial class PreviewBrowserWindow : MetroWindow, IPreviewBrowser
    {

        public bool IsClosed { get; set; }

        public AppModel Model { get; set; }

        IPreviewBrowser PreviewBrowser { get; set; }

         public PreviewBrowserWindow()
        {
            InitializeComponent();

            mmApp.SetThemeWindowOverride(this);

            Model = mmApp.Model;
            DataContext = Model;        
            
            LoadInternalPreviewBrowser();
            SetWindowPositionFromConfig();
        }

        void LoadInternalPreviewBrowser()
        {
            PreviewBrowser = AddinManager.Current.RaiseGetPreviewBrowserControl();
            if (PreviewBrowser == null)
                PreviewBrowser = new IEWebBrowserControl() { Name = "PreviewBrowser" };

            PreviewBrowserContainer.Children.Add(PreviewBrowser as UIElement);            
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

        #region IPreviewBrowser
        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false, string renderedHtml = null)
        {            
            PreviewBrowser.PreviewMarkdownAsync(editor, keepScrollPosition, renderedHtml);
        }

        public void PreviewMarkdown(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false, bool showInBrowser = false, string renderedHtml = null)
        {
            PreviewBrowser.PreviewMarkdown(editor, keepScrollPosition,showInBrowser,renderedHtml);
        }

        public void Navigate(string url)
        {
            PreviewBrowser.Navigate(url);
        }


        public void ExecuteCommand(string command, params dynamic[] args)
        {
            PreviewBrowser.ExecuteCommand(command, args);
        }
        #endregion


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
