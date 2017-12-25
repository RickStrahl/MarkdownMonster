using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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



        private ObservableCollection<ObservableCollection<CellContent>> _tableData;

        public ObservableCollection<ObservableCollection<CellContent>> TableData
        {
            get
            {
                if (_tableData == null)
                    _tableData = new ObservableCollection<ObservableCollection<CellContent>>();
                return _tableData;
            }
            set { _tableData = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableData"></param>
        /// <returns></returns>
        public string ParseDataToMarkdown(ObservableCollection<ObservableCollection<CellContent>> tableData = null)
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

            var columnInfo = GetColumnInfo(tableData);         

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

            foreach (var row in tableData.Skip(1))
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

        /// <summary>
        /// Takes the input collection and parses it into an HTML string. First row is considered to be the
        /// header of the table.
        /// </summary>
        /// <param name="tableData"></param>
        /// <returns></returns>
        public string ParseDataToHtml(ObservableCollection<ObservableCollection<CellContent>> tableData = null)
        {
            if (tableData == null)
                tableData = TableData;

            if (tableData == null || tableData.Count < 1)
                return string.Empty;

            for (int i = tableData.Count - 1; i > -1; i--)
            {
                if (tableData[i] == null || tableData[i].Count == 0)
                    tableData.Remove(tableData[i]);
            }

            var columnInfo = GetColumnInfo(tableData);

            StringBuilder sb = new StringBuilder();
            sb.Clear();
            
            sb.AppendLine("\n<table>");
            sb.AppendLine("<thead>");
            sb.AppendLine("\t<tr>");

            for (int i = 0; i < columnInfo.Count; i++)
            {
                var colInfo = columnInfo[i];
                sb.AppendLine($"\t\t<th>{HtmlUtils.HtmlEncode(colInfo.Title.Trim())}</th>");
            }

            sb.AppendLine("\t</tr>");
            sb.AppendLine("</thead>");

            sb.AppendLine("<tbody>");
            foreach (var row in tableData.Skip(1))
            {
                sb.AppendLine("\t<tr>");
                for (int i = 0; i < row.Count; i++)
                {
                    var col = row[i];
                    col.Text = col.Text.Replace("\n", "<br>").Replace("\r", "");                    
                    sb.AppendLine($"\t\t<td>{HtmlUtils.HtmlEncode(col.Text.Trim())}</td>");
                }

                sb.AppendLine("\t</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>\n");

            return sb.ToString();
        }


        /// <summary>
            /// Parses a table represented as Markdown into an Observable collection
            /// </summary>
            /// <param name="tableMarkdown"></param>
            /// <returns></returns>
            public ObservableCollection<ObservableCollection<CellContent>> ParseMarkdownToData(string tableMarkdown)
        {
            var data = new ObservableCollection<ObservableCollection<CellContent>>();
            if (string.IsNullOrEmpty(tableMarkdown))
                return data;

            var lines = StringUtils.GetLines(tableMarkdown.Trim());
            foreach (var row in lines)
            {
                if (row.Length == 0)
                    continue;
                if (row.StartsWith("|---"))
                    continue;

                var cols = row.Trim('|').Split('|');
                var columnData = new ObservableCollection<CellContent>();
                foreach (var col in cols)
                    columnData.Add(new CellContent(col.Trim()));

                data.Add(columnData);
            }


            return data;
        }


        /// <summary>
        /// Retrieves information about each of the columns in the table including
        /// max width and title. Looks at the first row of the table data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="tableHeaders"></param>
        /// <returns></returns>
        public List<ColumnInfo> GetColumnInfo(ObservableCollection<ObservableCollection<CellContent>> data)
        {
            var headers = new List<ColumnInfo>();
            if (data == null || data.Count < 1)
                return headers;

            var cols = data[0];

            for (int i = 0; i < cols.Count; i++)
            {
                var header = cols[i].Text;
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

                headers.Add(colInfo);
            }

            return headers;
        }
    }

    [DebuggerDisplay("{Title} - {MaxWidth}")]
    public class ColumnInfo
    {
        public string Title;
        public int MaxWidth;        
    }



}
