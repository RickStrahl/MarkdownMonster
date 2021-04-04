using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MarkdownMonster.Annotations;
using MarkdownMonster.Windows.PreviewBrowser;
using Control = System.Windows.Controls.Control;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for PasteHref.xaml
    /// </summary>
    public partial class TableEditorHtml : MetroWindow, INotifyPropertyChanged
    {
        public bool Cancelled { get; set; } = true;
        private string _tableHtml;

        public string TableHtml

        {
            get { return _tableHtml; }
            set
            {
                if (value == _tableHtml) return;
                _tableHtml = value;
                OnPropertyChanged();
            }
        }

        public bool EmbedAsHtml
        {
            get { return _embedAsHtml; }
            set
            {
                if (value == _embedAsHtml) return;
                _embedAsHtml = value;
                OnPropertyChanged();
            }
        }

        private bool _embedAsHtml;

        public AppModel AppModel { get; set; }

        public TableData TableData { get; set; } = new TableData();

        public ObservableCollection<string> TableModes { get; set; } =
            new ObservableCollection<string> {"Pipe Table", "Grid Table", "HTML Table"};


        private TableEditorDotnetInterop Interop { get; set; }

        private TableEditorJavaScriptCallbacks JavaScriptCallbacks { get; set; }

        public string TableMode
        {
            get { return _tableMode; }
            set
            {
                if (value == _tableMode) return;
                _tableMode = value;
                OnPropertyChanged();
            }
        }

        private string _tableMode = "Pipe Table";
        private IEWebBrowserEditorHandler IEHandler { get; }

        public TableEditorHtml(string tableHtml = null)
        {
            InitializeComponent();

            AppModel = mmApp.Model;

            mmApp.SetThemeWindowOverride(this);
            Owner = AppModel.Window;

            var data = new List<string[]>();

            if (tableHtml == null)
                CreateInitialTableData();
            else
            {

            }

            DataContext = this;
            Loaded += TableEditorHtml_Loaded;

            WebBrowser.LoadCompleted += WebBrowser_LoadCompleted;
            IEHandler = new IEWebBrowserEditorHandler(WebBrowser);
        }



        private void TableEditorHtml_Loaded(object sender, RoutedEventArgs e)
        {
            RenderTable();
        }

        private void WebBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (!e.Uri.ToString().ToLower().Contains("tableeditor.html")) return;


            var json = BaseBrowserInterop.SerializeObject(TableData);
            Debug.WriteLine(json);

            WebBrowser.Focus();

            JavaScriptCallbacks = new TableEditorJavaScriptCallbacks(this);
            var inst = WebBrowser.InvokeScript("InitializeInterop", JavaScriptCallbacks, json);

            Interop = new TableEditorDotnetInterop(inst);
   
        }


        private void CreateInitialTableData()
        {
            var td = new TableData();
            td.Headers.Add("Header 1");
            td.Headers.Add("Header 2");

            var colList = new List<string>(new[] {"Row 1 Column 1", "Row 1 Column 2"});
            td.Rows.Add(colList);

            colList = new List<string>(new[] {"Row 2 Column 1", "Row 2 Column 2"});
            td.Rows.Add(colList);

            TableData = td;
        }

        private void RenderTable()
        {
#if DEBUG
            //var url = @"c:\projects\markdownmonster\markdownmonster\PreviewThemes\TableEditor.html";
            var url = "https://localhost:5200/TableEditor.html";
#else
            var url = Path.Combine(App.InitialStartDirectory, "PreviewThemes", "TableEditor.html");
#endif
            WebBrowser.Navigate(new System.Uri(url));
        }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


        }

        public void CreateTableFromClipboardHtml(string html = null)
        {
            if (string.IsNullOrEmpty(html))
            {
                html = ClipboardHelper.GetHtmlFromClipboard();
                if (string.IsNullOrEmpty(html))
                    html = ClipboardHelper.GetText();
            }

            var parser = new TableParser();

            ObservableCollection<ObservableCollection<CellContent>> data = null;
            if (html.Contains("<tr>"))
            {
                data = parser.ParseHtmlToData(html);
            }
            else if (html.Contains("-|-") || html.Contains("- | -") || html.Contains(""))
            {
                data = parser.ParseMarkdownToData(html);
            }
            else if (html.Contains("-|-") || html.Contains("- | -") || html.Contains(""))
            {
                data = parser.ParseMarkdownToData(html);
            }
            else if (html.Contains("-+-"))
            {
                data = parser.ParseMarkdownGridTableToData(html);
            }

            if (data == null || data.Count < 1)
            {
                AppModel.Window.ShowStatusError("No HTML Table to process found...");
                return;
            }

        }
    }


    public class TableData
    {
        public ColLocation ActiveCell { get; set; } = new ColLocation();

        public List<string> Headers {get; set; }= new List<string>();

        public List<List<string>> Rows { get; set; } = new List<List<string>>();
    }

    [DebuggerDisplay("r{Row}:c{Column}")]
    public class ColLocation
    {
        public int Row {get; set; }

        public int Column {get; set; }
    }
}
