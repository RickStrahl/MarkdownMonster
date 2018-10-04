using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MarkdownMonster.Annotations;
using Microsoft.Win32;
using Westwind.Utilities;
using Binding = System.Windows.Data.Binding;
using Clipboard = System.Windows.Clipboard;
using Control = System.Windows.Controls.Control;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextBox = System.Windows.Controls.TextBox;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for PasteHref.xaml
    /// </summary>
    public partial class TableEditor : MetroWindow, INotifyPropertyChanged
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
        
        public ObservableCollection<ObservableCollection<CellContent>> TableData
        {
            get
            {
                if (_tableData == null)
                    _tableData = new ObservableCollection<ObservableCollection<CellContent>>();

                return _tableData;
            }
            set
            {
                if (Equals(value, _tableData)) return;
                _tableData = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<ObservableCollection<CellContent>> _tableData;


        

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

        public ObservableCollection<string> TableModes { get; set; } = new ObservableCollection<string> { "Pipe Table", "Grid Table", "HTML Table" };

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


        public CommandBase ColumnKeyCommand { get; set; }

        public TableEditor(string tableHtml = null)
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
                var parser = new TableParser();
                TableData = parser.ParseMarkdownToData(tableHtml);
                if (tableHtml.StartsWith("+"))
                    TableMode = "Grid Table";
            }

            DataGridTableEditor.ParentWindow = this;
            DataGridTableEditor.AppModel = mmApp.Model;
            DataGridTableEditor.TableSource = TableData;

            ColumnKeyCommand = new CommandBase((parameter, command) =>
            {
                            
            }, (p, c) => true);

            
            DataContext = this;
        }


        private void CreateTable()
        {
            //var cols = TableHeaders.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            
            //foreach (var col in cols)
            //{
            //    TableData.Columns.Add(StringUtils.RandomString(5,false));
            //}                        
        }

        private void CreateInitialTableData()
        {
            
            TableData.Clear();
            TableData.Add(new ObservableCollection<CellContent>
            {
                new CellContent("Header 1"),
                new CellContent("Header 2"),                
            });
            TableData.Add(new ObservableCollection<CellContent>
            {
                new CellContent("Column 1"),
                new CellContent("Column 2"),                
            });
            //TableData.Add(new ObservableCollection<CellContent>
            //{
            //    new CellContent("Column 3"),
            //    new CellContent("Column 4"),
            
            //});            
        }
        


    

        private void Column_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var txt = sender as TextBox;            
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
                var parser = new TableParser();
                
                if (TableMode == "Grid Table")
                    TableHtml = parser.ToGridTableMarkdown(TableData);
                else if(TableMode == "HTML Table")
                    TableHtml = parser.ToTableHtml(TableData);
                else
                    TableHtml = parser.ToPipeTableMarkdown(TableData);

                Cancelled = false;
                DialogResult = true; 
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
                
                var parser = new TableParser();

                bool deleteCsvFile = false;
                string csvFile = form.CsvFilename;

                if (form.ImportFromClipboard)
                {
                    string csvText = Clipboard.GetText();
                    csvFile = Path.GetTempFileName();
                    csvFile = Path.ChangeExtension(csvFile, "csv");
                    File.WriteAllText(csvFile,csvText);
                    deleteCsvFile = true;
                }


                var data = parser.ParseCsvFileToData(csvFile, form.CsvSeparator);
                    if (data == null || data.Count < 1)
                    {
                        AppModel.Window.ShowStatusError($"Couldn\'t open file {csvFile} or the file is empty.");
                        return;
                    }

                    TableData = data;                    
                    DataGridTableEditor.TableSource = TableData;

                if (deleteCsvFile)
                    File.Delete(csvFile);
            }

            var focusedTextBox = FocusManager.GetFocusedElement(this) as TextBox;
            if (focusedTextBox == null)
                return;

            if ("MenuInsertColumnRight" == name)
            {
                var pos = focusedTextBox.Tag as TablePosition;
                DataGridTableEditor.AddColumn(pos.Row, pos.Column, ColumnInsertLocation.Right);
            }
            else if ("MenuInsertColumnLeft" == name)
            {
                var pos = focusedTextBox.Tag as TablePosition;
                DataGridTableEditor.AddColumn(pos.Row, pos.Column, ColumnInsertLocation.Left);
            }
            else if ("MenuDeleteColumn" == name)
            {
                var pos = focusedTextBox.Tag as TablePosition;
                DataGridTableEditor.DeleteColumn(pos.Row, pos.Column);
            }
            else if ("MenuInsertRowBelow" == name)
            {
                var pos = focusedTextBox.Tag as TablePosition;
                DataGridTableEditor.AddRow(pos.Row, pos.Column, RowInsertLocation.Below);
            }
            else if ("MenuInsertRowAbove" == name)
            {
                var pos = focusedTextBox.Tag as TablePosition;
                DataGridTableEditor.AddRow(pos.Row, pos.Column, RowInsertLocation.Above);
            }
            else if ("MenuDeleteRow" == name)
            {
                var pos = focusedTextBox.Tag as TablePosition;
                DataGridTableEditor.DeleteRow(pos.Row, pos.Column);
            }

        }

        public void CreateTableFromClipboardHtml(string html = null)
        {
            if (string.IsNullOrEmpty(html))
            {
                html = ClipboardHelper.GetHtmlFromClipboard();
                if (string.IsNullOrEmpty(html))
                    html = Clipboard.GetText();
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

            TableData = data;
            DataGridTableEditor.TableSource = TableData;
        }

        
    }
}
