using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MarkdownMonster.Annotations;
using Microsoft.Win32;
using Westwind.Utilities;
using Binding = System.Windows.Data.Binding;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for PasteHref.xaml
    /// </summary>
    public partial class TableEditor : MetroWindow, INotifyPropertyChanged
    {
        public bool Cancelled { get; set; }
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

        public string TableHeaders
        {
            get { return _TableHeaders; }
            set
            {
                if (value == _TableHeaders) return;
                _TableHeaders = value;
                OnPropertyChanged(nameof(TableHeaders));
            }
        }
        private string _TableHeaders = "";

        public CommandBase ColumnKeyCommand { get; set; }

        public TableEditor(string tableHtml = null)
        {
            InitializeComponent();
            
            mmApp.SetThemeWindowOverride(this);


            var data = new List<string[]>();

            if (tableHtml == null)
                CreateInitialTableData();
            else
                ParseTableData(tableHtml);

           
            ColumnKeyCommand = new CommandBase((parameter, command) =>
            {
               Debug.WriteLine("column key pressed.");                
            }, (p, c) => true);

            
            DataContext = this;
        }

        private void ParseTableData(string tableHtml)
        {
         
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
            TableHeaders = "Column1,Column2,Column3";
            
            TableData.Clear();
            TableData.Add(new ObservableCollection<CellContent>
            {
                new CellContent("Header 1"),
                new CellContent("Header 2"),
                new CellContent("Header 3")
            });
            TableData.Add(new ObservableCollection<CellContent>
            {
                new CellContent("Column 1"),
                new CellContent("Column 2"),
                new CellContent("Column 3")
            });
            TableData.Add(new ObservableCollection<CellContent>
            {
                new CellContent("Column 4"),
                new CellContent("Column 5"),
                new CellContent("Column 6")
            });
            BindTable();
        }
        


        private void BindTable()
        {
            DataGridTableEditor.ParentWindow = this;
            DataGridTableEditor.AppModel = mmApp.Model;
            DataGridTableEditor.TableSource = TableData;


            //var editStyle = DataGridTableContent.Resources["GridTextboxStyle"] as Style;
            //var displayStyle = DataGridTableContent.Resources["GridTextblockStyle"] as Style;
            
            
            //var headers = TableHeaders.Split(new char [] { ',', ';'}, StringSplitOptions.RemoveEmptyEntries );
            //DataGridTableContent.Columns.Clear();
            
            //for (int i = 0; i < TableData[0].Count; i++)
            //{                
            //    var header = headers[i];
            //    var column = TableData[0][i];
                
            //    var binding = new Binding("Text")
            //    {
            //        Source = column,
            //        Mode = System.Windows.Data.BindingMode.TwoWay
            //    };

            //    var col = new DataGridTextColumn();
            //    col.Binding = binding;
            //    col.Header = header;
            //    col.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            //    col.EditingElementStyle = editStyle;
            //    col.ElementStyle = displayStyle;

            //    DataGridTableContent.Columns.Add(col);                
            //}
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
            var focusedTextBox = FocusManager.GetFocusedElement(this) as TextBox;

            if (sender == ButtonOk)
            {
                var parser = new TableParser();
                TableHtml = parser.ParseDataToHtml(TableData, TableHeaders);                
                Close();
            }
            else if(sender == ButtonCancel)
            {
                Cancelled = true;
                Close();
            }            
            else if (sender == ButtonInsertColumn || "MenuInsertColumnRight" == name)
            {
                var pos = focusedTextBox.Tag as TablePosition;
                DataGridTableEditor.AddColumn(pos.Row, pos.Column, ColumnInsertLocation.Right);
            }
            else if (sender == ButtonInsertColumn || "MenuInsertColumnLeft" == name)
            {
                var pos = focusedTextBox.Tag as TablePosition;
                DataGridTableEditor.AddColumn(pos.Row, pos.Column, ColumnInsertLocation.Left);
            }
            else if (sender == ButtonInsertRow || "MenuInsertRowBelow" == name)
            {
                var pos = focusedTextBox.Tag as TablePosition;
                DataGridTableEditor.AddRow(pos.Row, pos.Column, RowInsertLocation.Below);
            }
            else if (sender == ButtonInsertRow || "MenuInsertRowAbove" == name)
            {
                var pos = focusedTextBox.Tag as TablePosition;
                DataGridTableEditor.AddRow(pos.Row, pos.Column, RowInsertLocation.Above);
            }
        }

        
    }
}
