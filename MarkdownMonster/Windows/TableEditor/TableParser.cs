using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    public class TableParser
    {
        public string TableHeaders { get; set; }
        public DataTable TableData { get; set; }

        public DataTable CreateDataTable(string columnHeaders)
        {
            var cols = columnHeaders.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            var tableData = new DataTable();
            foreach (var col in cols)
            {
                tableData.Columns.Add(StringUtils.RandomString(5, false));
            }

            TableData = tableData;
            TableHeaders = columnHeaders;

            return tableData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public string ParseDataTableToHtml(DataTable table = null, string tableHeaders=null)
        {
            if (table == null)
                table = TableData;

            if (table == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(tableHeaders))
            {                
                foreach (DataColumn col in table.Columns)
                    sb.Append(col.ColumnName + ",");

                tableHeaders = sb.ToString().TrimEnd(new char[] {',', ' '});
            }

            var headers = tableHeaders.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            sb.Clear();

            string line = "| ";
            foreach (var header in headers)
            {
                line += $"{header} | ";
            }

            sb.AppendLine(line.Trim());

            sb.Append("|");
            for (int i = 0; i < line.Length + 4 * headers.Length; i++)
                sb.Append("-");
            sb.AppendLine("|");

            foreach (DataRow row in table.Rows)
            {
                line = "| ";
                foreach (DataColumn col in table.Columns)
                {
                    line += $"{row[col]} | ";
                }

                sb.AppendLine(line.Trim());
            }

            return sb.ToString();
        }


    }
}
