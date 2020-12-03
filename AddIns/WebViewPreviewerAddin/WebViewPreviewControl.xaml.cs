using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using MarkdownMonster;
using MarkdownMonster.Windows;
using MarkdownMonster.Windows.PreviewBrowser;
using WebViewPreviewerAddin;
using Westwind.Utilities;

namespace WebViewPreviewerAddin
{
    /// <summary>
    /// Interaction logic for ChromiumPreviewControl.xaml
    /// </summary>
    public partial class WebViewPreviewControl : UserControl, IPreviewBrowser
    {
        
        public bool firstload = true;

        public WebViewPreviewControl()
        {
            InitializeComponent();


            Model = mmApp.Model;
            Window = Model.Window;

            PreviewBrowser = new WebViewPreviewHandler(WebBrowser);
            
            DataContext = Model;
        }

        private void WebViewPreviewControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        public AppModel Model { get; set; }

        public MainWindow Window { get; set; }

        IPreviewBrowser PreviewBrowser { get; set; }



        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false,
            string renderedHtml = null, int editorLineNumber = -1)
        {
            if (editor == null)
            {
                editor = mmApp.Model?.ActiveEditor;
                if (editor == null)
                    return; // not ready
            }
            
            PreviewBrowser.PreviewMarkdownAsync(editor, keepScrollPosition, renderedHtml);
        }

        public void PreviewMarkdown(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false, bool showInBrowser = false,
            string renderedHtml = null, int editorLineNumber = -1)
        {
            if (editor == null)
            {
                editor = mmApp.Model?.ActiveEditor;
                if (editor == null)
                    return; // not ready
            }

            PreviewBrowser.PreviewMarkdown(editor, keepScrollPosition, showInBrowser, renderedHtml);
        }

        public void Navigate(string url)
        {
            PreviewBrowser.Navigate(url);
        }

        public void Refresh(bool forceRefresh)
        {
            PreviewBrowser.Refresh(forceRefresh);
        }

        public void ExecuteCommand(string command, params object[] args)
        {
            PreviewBrowser.ExecuteCommand(command, args);
        }

        public void ShowDeveloperTools()
        {
            WebBrowser.CoreWebView2.OpenDevToolsWindow();
        }

        private void WebBrowser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (e.NewSize.Width > 100)
            //{
            //    int width = Convert.ToInt32(Window.MainWindowPreviewColumn.Width.Value);
            //    if (width > 100)
            //        mmApp.Configuration.WindowPosition.SplitterPosition = width;
            //}
        }

        public void ScrollToEditorLine(int editorLineNumber = -1, bool updateCodeBlocks = false, bool noScrollContentTimeout = false, bool noScrollTopAdjustment = false)
        {
            PreviewBrowser.ScrollToEditorLine(editorLineNumber, updateCodeBlocks, noScrollContentTimeout,
                noScrollTopAdjustment);
        }

        public void ScrollToEditorLineAsync(int editorLineNumber = -1, bool updateCodeBlocks = false, bool noScrollContentTimeout = false, bool noScrollTopAdjustment = false)
        {
            PreviewBrowser.ScrollToEditorLineAsync(editorLineNumber, updateCodeBlocks, noScrollContentTimeout,
                noScrollTopAdjustment);
        }
        

        public void Dispose()
        {
            
        }
    }

    public class WebViewControlModel : INotifyPropertyChanged
    {

        public string Url
        {
            get { return _Url; }
            set
            {
                if (value == _Url) return;
                _Url = value;
                OnPropertyChanged(nameof(Url));
            }
        }
        private string _Url = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
