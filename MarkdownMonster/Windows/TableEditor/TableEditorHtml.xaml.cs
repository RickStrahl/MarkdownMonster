using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
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


        public TableEditorDotnetInterop Interop { get; set; }

        public TableEditorJavaScriptCallbacks JavaScriptCallbacks { get; set; }

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

        public TableEditorHtml(string tableMarkdownOrHtml = null)
        {
            InitializeComponent();

            AppModel = mmApp.Model;

            mmApp.SetThemeWindowOverride(this);
            Owner = AppModel.Window;

            var data = new List<string[]>();

            if (tableMarkdownOrHtml == null)
                CreateInitialTableData();
            else
            {
                ParseHtmlAndRender(tableMarkdownOrHtml);
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
            WebBrowser.Focus();

            try
            {
                JavaScriptCallbacks = new TableEditorJavaScriptCallbacks(this);
                var inst = WebBrowser.InvokeScript("InitializeInterop", JavaScriptCallbacks, json);

                Interop = new TableEditorDotnetInterop(inst);
            }
            catch
            {
                mmApp.Model.Window.ShowStatusError("Unable to open table editor. Unable to navigate to editor template.");
                this.Close();
            }
        }


        private void CreateInitialTableData()
        {
            var td = new TableData();
            td.Headers.Add("Header 1");
            td.Headers.Add("Header 2:");

            for (int i = 0; i < 1; i++)
            {
                var colList = new List<string>(new[] { $"Row {i + 1} Column 1", $"Row {i + 1} Column 2"});
                td.Rows.Add(colList);    
            }

            TableData = td;
        }

        public void RenderTable()
        {
            
#if DEBUG
            var file = Path.Combine("c:\\projects\\MarkdownMonster\\MarkdownMonster", "PreviewThemes", "TableEditor.html");
            var outputFile = file.Replace("TableEditor.html", "_TableEditor.html");

            // IN DEBUG MODE USE LIVERELOAD SERVER while testing (if needed)
            var url = "https://localhost:5200/_TableEditor.html";
#else
            var file = Path.Combine(App.InitialStartDirectory, "PreviewThemes", "TableEditor.html");
            var outputFile = file.Replace("TableEditor.html", "_TableEditor.html");
            var url = outputFile;
#endif

            try
            {
                string template = File.ReadAllText(file);
                template = template.Replace("{{Theme}}", mmApp.Configuration.PreviewTheme);
                File.WriteAllText(outputFile, template);
            }
            catch
            {
                // if this fails use the template shipped
            }

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
             var name = (sender as Control).Name;
            
            if (sender == ButtonOk)
            {
                var parser = new TableParserHtml();
                
                if (TableMode == "Grid Table")
                    TableHtml = parser.ToGridTableMarkdown(TableData);
                else if(TableMode == "HTML Table")
                    TableHtml = parser.ToTableHtml(TableData);
                else
                    TableHtml = parser.ToPipeTableMarkdown(TableData);


                mmApp.Model.ActiveEditor.SetSelectionAndFocus(TableHtml);

                //Cancelled = false;
                Close();
            }
            else if(sender == ButtonCancel)
            {
                Cancelled = true;
                Close();
            }
            else if (sender == ButtonPasteHtml)
            {
                CreateTableFromClipboardHtml();
            }
            else if (sender == ButtonImportCsv)
            {
                var form = new TableEditorCsvImport();
                form.Owner = this;
                form.ShowDialog();

                if (form.IsCancelled)
                    return;

                var parser = new TableParserHtml();

                bool deleteCsvFile = false;
                string csvFile = form.CsvFilename;

                if (form.ImportFromClipboard)
                {
                    string csvText = ClipboardHelper.GetText();
                    csvFile = Path.GetTempFileName();
                    csvFile = Path.ChangeExtension(csvFile, "csv");
                    File.WriteAllText(csvFile, csvText);
                    deleteCsvFile = true;
                }


                var data = parser.ParseCsvFileToData(csvFile, form.CsvSeparator);
                if (data == null || data.Headers.Count < 1 && data.Rows.Count < 1)
                {
                    AppModel.Window.ShowStatusError($"Couldn\'t open file {csvFile} or the file is empty.");
                    return;
                }

                TableData = data;
                RenderTable();

                if (deleteCsvFile)
                    File.Delete(csvFile);
            }
        }

        public void CreateTableFromClipboardHtml(string html = null)
        {
            if (string.IsNullOrEmpty(html))
            {
                html = ClipboardHelper.GetHtmlFromClipboard();
                if (string.IsNullOrEmpty(html))
                    html = ClipboardHelper.GetText();
            }

            ParseHtmlAndRender(html);
        }

        private void ParseHtmlAndRender(string markdownOrHtmlTable)
        {
            var parser = new TableParserHtml();

            var data = parser.ParseMarkdownToData(markdownOrHtmlTable);
            if (data == null || data.Headers.Count < 1 && data.Rows.Count < 1)
            {
                AppModel.Window.ShowStatusError("No HTML Table to process found...");
                return;
            }

            TableData = data;
            RenderTable();
        }
    }


    public class TableData
    {
        public TableLocation ActiveCell { get; set; } = new TableLocation();

        public List<string> Headers {get; set; }= new List<string>();

        public List<List<string>> Rows { get; set; } = new List<List<string>>();

        public List<string> GetEmptyRow()
        {
            int count = 2;
            if (Headers != null && Headers.Count > 0)
                count = Headers.Count;
            else if (Rows.Count > 0)
                count = Rows[0].Count;

            var list = new List<string>();
            for (var i = 0; i < count; i++)
                list.Add(string.Empty);

            return list;
        }
    }

    [DebuggerDisplay("r{Row}:c{Column}")]
    public class TableLocation
    {
        public int Row {get; set; }
        public int Column {get; set; }
        public bool IsHeader { get; set; }
    }
}
