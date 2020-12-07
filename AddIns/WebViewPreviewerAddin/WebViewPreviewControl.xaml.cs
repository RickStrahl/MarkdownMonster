using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using MarkdownMonster;
using MarkdownMonster.Windows.PreviewBrowser;

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
            Loaded += WebViewPreviewControl_Loaded;
            
            DataContext = Model;

            PreviewBrowser = new WebViewPreviewHandler(WebBrowser);
        }

        private void WebViewPreviewControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
