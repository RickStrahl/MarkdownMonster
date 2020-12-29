using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MarkdownMonster.Windows.PreviewBrowser
{
    /// <summary>
    /// This is the actual IE WebBrowser control that handles all rendering for the IE 
    /// Web Browser control. All other implementations of IPreviewBrowser, simply 
    /// proxy into this control for Navigate, PreviewMarkdown operations.
    /// 
    /// This class in turn forwards all handling of the actual rendering to the PreviewBrowserHandler
    /// which is an IE handler
    /// </summary>
    public partial class IEWebBrowserControl    : UserControl, IPreviewBrowser, IDisposable
    {
        public AppModel Model { get; set; }

        public MainWindow Window { get; set; }

        public IEWebBrowserPreviewHandler PreviewBrowserHandler;

        public IEWebBrowserControl()
        {
            InitializeComponent();

            Model = mmApp.Model;
            this.Window = Model.Window;

            PreviewBrowserHandler = new IEWebBrowserPreviewHandler(WebBrowser);

            Loaded += PreviewBrowserWebBrowserControl_Loaded;

            DataContext = Model;
        }

        private void PreviewBrowserWebBrowserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void PreviewBrowserWebBrowserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //    if (e.NewSize.Width > 100)
            //    {
            //        int width = Convert.ToInt32(Window.MainWindowPreviewColumn.Width.Value);
            //        if (width > 100)
            //            mmApp.Configuration.WindowPosition.SplitterPosition = width;                
            //}
        }


        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false, string renderedHtml = null, int editorLineNumber = -1)
        {
            PreviewBrowserHandler.PreviewMarkdownAsync(editor, keepScrollPosition, renderedHtml, editorLineNumber);
        }


        public void PreviewMarkdown(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false, bool showInBrowser = false, string renderedHtml = null, int editorLineNumber = -1)
        {            
            PreviewBrowserHandler.PreviewMarkdown(editor, keepScrollPosition, showInBrowser, renderedHtml);
        }

        
        public void ScrollToEditorLine(int editorLineNumber = -1, bool updateCodeBlocks = false, bool noScrollTimeout = false, bool noScrollTopAdjustment = false)
        {
            PreviewBrowserHandler.ScrollToEditorLine(editorLineNumber, updateCodeBlocks, noScrollTimeout, noScrollTopAdjustment);
        }

        public async Task ScrollToEditorLineAsync(int editorLineNumber = -1, bool updateCodeBlocks = false, bool noScrollTimeout = false, bool noScrollTopAdjustment = false)
        {
            await PreviewBrowserHandler.ScrollToEditorLineAsync(editorLineNumber, updateCodeBlocks, noScrollTimeout, noScrollTopAdjustment);
        }

        public void Navigate(string url)
        {
            WebBrowser.Navigate(new Uri(url));
        }

        public void Refresh(bool noCache)
        {
            WebBrowser.Refresh(noCache);
            PreviewMarkdownAsync();
        }

        public void ExecuteCommand(string command, params object[] args)
        {
            PreviewBrowserHandler.ExecuteCommand(command, args);
        }

        public void ShowDeveloperTools()
        {
            MessageBox.Show(mmApp.Model.Window,
                "This browser doesn't support Developer tools.", "Developer Tools",
                MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
        }

        void IDisposable.Dispose()
        {
            WebBrowser?.Dispose();
        }
    }
}
