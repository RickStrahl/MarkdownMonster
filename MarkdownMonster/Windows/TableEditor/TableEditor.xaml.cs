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





        public DataTable TableData
        {
            get { return _tableData; }
            set
            {
                if (Equals(value, _tableData)) return;
                _tableData = value;
                OnPropertyChanged();
            }
        }
        private DataTable _tableData;

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
            var cols = TableHeaders.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            TableData = new DataTable();
            foreach (var col in cols)
            {
                TableData.Columns.Add(StringUtils.RandomString(5,false));
            }                        
        }

        private void CreateInitialTableData()
        {
            TableHeaders = "Column1,Column2,Column3";
            CreateTable();
            
            var row = TableData.NewRow();
            row[0] = "Column1";
            row[1] = "Column2";
            row[2] = "Column3";
            TableData.Rows.Add(row);

            row = TableData.NewRow();
            row[0] = "Column 4";
            row[1] = "Column 5";
            row[2] = "Column 6";
            TableData.Rows.Add(row);

            BindTable();
        }

        private void SetTableHtmlFromData()
        {
            var headers = TableHeaders.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sb = new StringBuilder();

            string line = "| ";
            foreach (var header in headers)
            {
                line += $"{header} | ";
            }

            sb.AppendLine(line.Trim());

            sb.Append("|");
            for (int i = 0; i < line.Length + 4 * headers.Length; i++ ) 
                sb.Append("-");
            sb.AppendLine("|");
            
            foreach (DataRow row in TableData.Rows)
            {
                line = "| ";
                foreach (DataColumn col in TableData.Columns)
                {
                    line += $"{row[col]} | ";
                }

                sb.AppendLine(line.Trim());
            }

            TableHtml = sb.ToString();
        }


        private void BindTable()
        {
            var editStyle = DataGridTableContent.Resources["GridTextboxStyle"] as Style;
            var displayStyle = DataGridTableContent.Resources["GridTextblockStyle"] as Style;
            
            
            var headers = TableHeaders.Split(new char [] { ',', ';'}, StringSplitOptions.RemoveEmptyEntries );
            DataGridTableContent.Columns.Clear();

            
            for (int i = 0; i < TableData.Columns.Count; i++)

            {
                var fieldname = TableData.Columns[i].ColumnName;

                var header = headers[i];
                var binding = new Binding(fieldname);
                binding.Mode = System.Windows.Data.BindingMode.OneWay;

                var binding2 = new Binding(fieldname);
                binding2.Mode = System.Windows.Data.BindingMode.Default;
                
                var col = new DataGridTextColumn();
                col.Binding = binding2;
                col.Header = header;
                col.Width = new DataGridLength(80, DataGridLengthUnitType.Star);
                col.EditingElementStyle = editStyle;
                col.ElementStyle = displayStyle;

                DataGridTableContent.Columns.Add(col);                
            }
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
            if (sender == ButtonOk)
            {
                SetTableHtmlFromData();
                Close();

            }
            else
            {
                Cancelled = true;
                Close();
            }
        }

        
    }
}
