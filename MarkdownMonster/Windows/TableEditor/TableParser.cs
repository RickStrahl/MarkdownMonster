using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.Annotations;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    public class TableParser
    {


        public string TableHeaders { get; set; }

        /// <summary>
        /// Maximum column width that's to be padded.
        /// If greater values are rendering ragged
        /// </summary>
        public int MaxColumnWidth { get; set; } = 40;


        private ObservableCollection<ObservableCollection<ColumnText>> _tableData;

        public ObservableCollection<ObservableCollection<ColumnText>> TableData
        {
            get
            {
                if (_tableData == null)
                    _tableData = new ObservableCollection<ObservableCollection<ColumnText>>();
                return _tableData;
            }
            set { _tableData = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableData"></param>
        /// <returns></returns>
        public string ParseDataToHtml(ObservableCollection<ObservableCollection<ColumnText>> tableData = null, string tableHeaders=null)
        {
            if (tableData == null)
                tableData = TableData;

            if (tableData == null || tableData.Count < 1)
                return string.Empty;

            for (int i = tableData.Count-1; i > -1; i--)
            {
                if (tableData[i] == null || tableData[i].Count == 0)
                    tableData.Remove(tableData[i]);
            }

            var columnInfo = GetColumnInfo(tableData, tableHeaders);         

            StringBuilder sb = new StringBuilder();                        
            sb.Clear();

            string line = "\n| ";
            for (int i = 0; i < columnInfo.Count; i++)
            {
                var colInfo = columnInfo[i];                
                line += $"{colInfo.Title.PadRight(colInfo.MaxWidth)} | ";
            }
            sb.AppendLine(line.TrimEnd());

            
            sb.Append("|");
            for (int i = 0; i < line.Length-4; i++)
                sb.Append("-");
            sb.AppendLine("|");

            foreach (var row in tableData)
            {
                line = "| ";
                for (int i = 0; i < row.Count; i++)
                {
                    var col = row[i];
                    col.Text = col.Text.Replace("\n", "<br>").Replace("\r", "");

                    var colInfo = columnInfo[i];
                    line += col.Text.PadRight(colInfo.MaxWidth) + " | ";
                }

                sb.AppendLine(line.Trim());
            }

            return sb + "\n";
        }

        public List<ColumnInfo> GetColumnInfo(ObservableCollection<ObservableCollection<ColumnText>> data, string tableHeaders)
        {
            var cols = new List<ColumnInfo>();
            var headers = tableHeaders.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
                var colInfo = new ColumnInfo
                {
                    Title = header,
                    MaxWidth = header.Length
                };
               
                var maxWidth = data.Max(d => d[i].Text.Length);
                if (maxWidth > colInfo.MaxWidth)
                    colInfo.MaxWidth = maxWidth;
                if (colInfo.MaxWidth > MaxColumnWidth)
                    colInfo.MaxWidth = MaxColumnWidth;

                cols.Add(colInfo);
            }

            return cols;
        }
    }

    public class ColumnInfo
    {
        public string Title;
        public int MaxWidth;        
    }

    public class ColumnText : INotifyPropertyChanged
    {
        public string Text 
        {
            get { return _text; }
            set
            {
                if (value == _text) return;
                _text = value;
                OnPropertyChanged();
            }
        }
        private string _text;

        

        public int Row
        {
            get { return _row; }
            set
            {
                if (value == _row) return;
                _row = value;
                OnPropertyChanged();
            }
        }
        private int _row;

        

        public int Column
        {
            get { return _column; }
            set
            {
                if (value == _column) return;
                _column = value;
                OnPropertyChanged();
            }
        }
        private int _column;

        public ColumnText(string text)
        {
            Text = text;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
