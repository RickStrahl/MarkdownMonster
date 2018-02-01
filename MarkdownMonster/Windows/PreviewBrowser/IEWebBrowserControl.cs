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
    public partial class IEWebBrowserControl    : UserControl, IPreviewBrowser
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
                if (e.NewSize.Width > 100)
                {
                    int width = Convert.ToInt32(Window.MainWindowPreviewColumn.Width.Value);
                    if (width > 100)
                        mmApp.Configuration.WindowPosition.SplitterPosition = width;                
            }
        }


        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false, string renderedHtml = null)
        {
            PreviewBrowserHandler.PreviewMarkdownAsync(editor, keepScrollPosition, renderedHtml);
        }

        public void PreviewMarkdown(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false, bool showInBrowser = false, string renderedHtml = null)
        {            
            PreviewBrowserHandler.PreviewMarkdown(editor, keepScrollPosition, showInBrowser, renderedHtml);
        }

        public void Navigate(string url)
        {
            WebBrowser.Navigate(url);
        }

        public void ExecuteCommand(string command, params dynamic[] args)
        {

            if (command == "PreviewContextMenu")
            {
                var ctm = WebBrowser.ContextMenu;
                ctm.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                ctm.PlacementTarget = WebBrowser;
                ctm.IsOpen = true;
            }

            if (command == "PrintPreview")
            {
                dynamic dom = WebBrowser.Document;
                dom.execCommand("print", true, null);
            }
        }
    }
}
