using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using LumenWorks.Framework.IO.Csv;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    public class TableParserHtml
    {
        /// <summary>
        /// Maximum column width that's to be padded.
        /// If greater values are rendering ragged
        /// </summary>
        public int MaxColumnWidth { get; set; } = 40;

        /// <summary>
        /// The data to process
        /// </summary>
        public TableData TableData {get; set; }


        #region Parsing Functions
         /// <summary>
        ///
        /// </summary>
        /// <param name="tableData"></param>
        /// <returns></returns>
        public string ToPipeTableMarkdown(TableData tableData = null)
        {
            if (tableData == null)
                tableData = TableData;

            if (tableData == null || tableData.Headers.Count < 1 && tableData.Rows.Count < 1)
                return string.Empty;

            var columnInfo = GetColumnInfo(tableData);

            var sb = new StringBuilder();
            sb.Clear();

            string line = $"| ";
            string separator = "|";
            for (int i = 0; i < columnInfo.Count; i++)
            {
                var colInfo = columnInfo[i];
                string title = colInfo.Title;
                title = title.Trim(':');
                line += $"{title.PadRight(colInfo.MaxWidth)} | ";

                if (colInfo.Title.StartsWith(":"))
                    separator += ":" + "-".PadRight(colInfo.MaxWidth, '-');
                else
                    separator += "-" + "-".PadRight(colInfo.MaxWidth, '-');

                if (colInfo.Title.EndsWith(":"))
                    separator += ":|";
                else
                    separator += "-|";
            }

            sb.AppendLine(line.TrimEnd());
            sb.AppendLine(separator);


            foreach (var row in tableData.Rows)
            {
                line = "| ";
                for (int i = 0; i < row.Count; i++)
                {
                    if (i >= columnInfo.Count)
                        break;

                    var col = row[i];
                    col = col.Replace("\n", "<br>").Replace("\r", "");

                    var colInfo = columnInfo[i];
                    line += col.PadRight(colInfo.MaxWidth) + " | ";
                }

                sb.AppendLine(line.Trim());
            }

            return sb.ToString();
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="tableData"></param>
        /// <returns></returns>
        public string ToGridTableMarkdown(TableData tableData = null)
        {
            if (tableData == null)
                tableData = TableData;

            if (tableData == null || tableData.Rows.Count < 1 && tableData.Headers.Count < 1)
                return string.Empty;


            var columnInfo = GetColumnInfo(tableData);

            var sb = new StringBuilder();
            sb.Clear();
            string separatorLine = "+-";
            string line = "| ";
            for (int i = 0; i < columnInfo.Count; i++)
            {
                var colInfo = columnInfo[i];
                line += $"{colInfo.Title.Trim(':').PadRight(colInfo.MaxWidth)} | ";
                separatorLine += "-".PadRight(colInfo.MaxWidth, '-') + "-+-";
            }

            separatorLine = separatorLine.TrimEnd('-');

            sb.AppendLine(separatorLine);
            sb.AppendLine(line.TrimEnd());
            sb.AppendLine(separatorLine.Replace("-", "="));

            for(var rowIndex = 0; rowIndex < tableData.Rows.Count; rowIndex++) // rows
            {
                var row = tableData.Rows[rowIndex];
                var maxLineCount = row.Max(s => s.Count(s2 => s2 == '\n')) + 1;

                for (int x = 0; x < maxLineCount; x++)    // text lines (line count)
                {
                    line = "| ";
                    for (int i = 0; i < columnInfo.Count; i++) // columns
                    {
                        var col = row[i];
                        var textLines = StringUtils.GetLines(col);

                        string content;
                        if (textLines.Length <= x)
                            content = string.Empty;
                        else
                            content = textLines[x];

                        line += content.PadRight(columnInfo[i].MaxWidth) + " | ";
                    }
                    sb.AppendLine(line.Trim());
                }
                sb.AppendLine(separatorLine);
            }


            return sb + mmApp.NewLine;
        }

        
        /// <summary>
        /// Takes the input collection and parses it into an HTML string. First row is considered to be the
        /// header of the table.
        /// </summary>
        /// <param name="tableData"></param>
        /// <returns></returns>
        public string ToTableHtml(TableData tableData = null)
        {
            if (tableData == null)
                tableData = TableData;

            if (tableData == null || tableData.Rows.Count < 1 && tableData.Headers.Count < 1)
                return string.Empty;

            var mdParser = MarkdownParserFactory.GetParser();

            var columnInfo = GetColumnInfo(tableData);

            StringBuilder sb = new StringBuilder();
            sb.Clear();

            sb.AppendLine("\n<table>");
            sb.AppendLine("<thead>");
            sb.AppendLine("\t<tr>");

            for (int i = 0; i < columnInfo.Count; i++)
            {
                var colInfo = columnInfo[i];

                var align= string.Empty;
                if (columnInfo[i].Justification == ColumnJustifications.Right)
                    align = " style=\"text-align:right\"";
                else if (columnInfo[i].Justification == ColumnJustifications.Center)
                    align = " style=\"text-align:center\"";

                sb.AppendLine($"\t\t<th{align}>{WebUtility.HtmlEncode(colInfo.Title.Trim(' ',':','\n','\r'))}</th>");
            }
            sb.AppendLine("\t</tr>");
            sb.AppendLine("</thead>");

            sb.AppendLine("<tbody>");
            foreach (var row in tableData.Rows)
            {
                sb.AppendLine("\t<tr>");
                for (int i = 0; i < columnInfo.Count; i++)
                {
                    var col = row[i];
                    if (string.IsNullOrEmpty(col))
                        col = string.Empty;
                    else 
                        col = col.Replace("\n", "<br>").Replace("\r", "");

                    var align= string.Empty;
                    if (columnInfo[i].Justification == ColumnJustifications.Right)
                        align = " style=\"text-align: right\"";
                    else if (columnInfo[i].Justification == ColumnJustifications.Center)
                        align = " style=\"text-align: center\"";

                    var text = mdParser.Parse(col.Trim()).Replace("<p>", "").Replace("</p>", "").Trim();
                    sb.AppendLine($"\t\t<td{align}>{text}</td>");
                }

                sb.AppendLine("\t</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine($"</table>{mmApp.NewLine}");

            return sb.ToString();
        }

        #endregion

        #region Parse Markdown to TableData

        /// <summary>
        /// Parses a table represented as Markdown or HTML into an Observable collection
        /// </summary>
        /// <param name="tableMarkdown"></param>
        /// <returns>TableData object or null if string is not a markdown table format recognized</returns>
        public TableData ParseMarkdownToData(string tableMarkdown)
        {
            var data = new TableData();
            if (string.IsNullOrEmpty(tableMarkdown))
                return data;

            var type = DetectTableType(tableMarkdown);
            if (type == MarkdownTableType.None)
                return null;

            if (type == MarkdownTableType.Grid)
                return ParseMarkdownGridTableToData(tableMarkdown);
            if (type == MarkdownTableType.Html)
                return ParseHtmlToData(tableMarkdown);

            return ParseMarkdownPipeTableToData(tableMarkdown);
        }


        /// <summary>
        /// Parses a Markdown Pipe Table to an Observable Data Collection
        /// </summary>
        /// <param name="tableMarkdown"></param>
        /// <returns></returns>
        TableData ParseMarkdownPipeTableToData(string tableMarkdown)
        {
            var data = new TableData();
            var lines = StringUtils.GetLines(tableMarkdown.Trim());
            for (var index = 0; index < lines.Length; index++)
            {
                var row = lines[index]?.Trim();
                if (string.IsNullOrEmpty(row))
                    continue;

                // check for aligned headers
                if (data.Headers.Count > 0 &&   // header row must exist
                    row.StartsWith("|--") || row.StartsWith("| --") ||
                    row.StartsWith("|:-") || row.StartsWith("| :-") ||
                    row.StartsWith("--"))
                {
                    if (!row.Contains(":"))
                        continue;

                    var headerCols = row.TrimEnd().Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < headerCols.Length; i++)
                    {
                        var sepLine = headerCols[i].Trim();
                        if (sepLine.StartsWith(":"))
                            data.Headers[i] = ":" + data.Headers[i];
                        if (sepLine.EndsWith(":"))
                            data.Headers[i] = data.Headers[i] + ":";
                    }

                    continue;
                }

                var cols = row.TrimEnd().Trim('|').Split('|');
                var columnData = new List<string>();
                foreach (var col in cols)
                {
                    var txt = col.Trim()
                        .Replace("<br>", "\n")
                        .Replace("</br>", "\n");
                    columnData.Add(txt);
                }

                if (index == 0)  // assume it's the header
                    data.Headers = columnData;
                else
                    data.Rows.Add(columnData);
            }

            BalanceTableColumns(data);

            return data;
        }


        
        /// <summary>
        /// Parses a Markdown Grid table to a Data Observable Collection
        /// </summary>
        /// <param name="tableMarkdown"></param>
        /// <returns></returns>
        public TableData ParseMarkdownGridTableToData(string tableMarkdown)
        {
            var data = new TableData();
            if (string.IsNullOrEmpty(tableMarkdown))
                return data;

            var lines = StringUtils.GetLines(tableMarkdown.Trim());

            // loop through rows
            for (var index = 0; index < lines.Length; index++)
            {
                var rowText = lines[index]?.Trim();
                if (rowText.Length == 0)
                    continue;

                // Skip over grid lines
                if (rowText.StartsWith("+--") || rowText.StartsWith("+=="))
                {
                    var columnData = new List<string>();

                    // goto next 'column line'
                    index++;
                    if (index >= lines.Length)
                        break;
                    rowText = lines[index]?.Trim();

                    var cellText = new List<StringBuilder>();
                    string[] cols = new string[0];

                    while (true)  // loop through multiple lines
                    {
                        cols = rowText.TrimEnd().Trim('|').Split(new[] { '|'}, StringSplitOptions.RemoveEmptyEntries);

                        if (cellText.Count < 1)
                        {
                            for (var i = 0; i < cols.Length; i++)
                                cellText.Add(new StringBuilder());
                        }

                        for (var i = 0; i < cols.Length; i++)
                        {
                            var col = cols[i]?.Trim();
                            if(!string.IsNullOrEmpty(col))
                                cellText[i].AppendLine(col.Trim());
                        }
                        
                        // get the next line of this column
                        if (lines[index + 1].Trim().StartsWith("|"))
                        {
                            index++;
                            rowText = lines[index]?.Trim();  // process next line
                        }
                        else
                            break;
                    }

                    if (cols.Length == 0)
                        continue;

                    // collect multiple lines per column
                    for (var i = 0; i < cols.Length; i++)
                    {
                        cellText[i].Length -= 2; // strip off trailing \r\n
                        var ctext = cellText[i].ToString().Replace("\r", "");
                        columnData.Add(ctext);
                    }

                    if (index < 2)
                        data.Headers = columnData;
                    else
                        data.Rows.Add(columnData);
                }
            }

            BalanceTableColumns(data);
            
            return data;
        }



        /// <summary>
        /// Parses and HTML table to a TableData object
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public TableData ParseHtmlToData(string html)
        {
            var data = new TableData();
            if (string.IsNullOrEmpty(html))
                return data;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var headerRow = doc.DocumentNode.SelectSingleNode("//tr");
            if (headerRow == null)
                return data;

            var headerColumns = new List<string>();
            var headerCols = headerRow.SelectNodes("th");
            if ( headerCols == null)
                headerCols = headerRow.SelectNodes("td");
            if (headerCols == null)
                return data;

            foreach (var node in headerCols)
            {
                var text = node.InnerText;
                var attrStyle = node.Attributes["style"];
                if (attrStyle != null)
                {
                    var style = attrStyle.Value;
                    if (style.Contains("text-align: right"))
                        text = text + ":";
                    else if(style.Contains("text-align: center"))
                        text = ":" + text + ":";
                }
                headerColumns.Add(text);
            }

            data.Headers = headerColumns;

            var nodes = doc.DocumentNode.SelectNodes("//tr");
            foreach (var trNode in nodes.Skip(1))
            {
                var rowColumns = new List<string>();
                var cols = trNode.SelectNodes("td");
                if (cols == null)
                    continue;
                
                foreach (var node in cols)
                {
                    string text;
                    var nodeHtml = node.InnerHtml;

                    // check for common replacements
                    if (!nodeHtml.Contains("<"))
                        text  = node.InnerText;
                    else
                    {
                        text = nodeHtml
                            .Replace("<b>", "**")
                            .Replace("</b>", "**")
                            .Replace("<i>", "*")
                            .Replace("</i>", "*")
                            .Replace("<br>", "\n");

                        // convert links and images
                        if (text.Contains("<"))
                            text = ParseLinkAndImage(text);
                    }

                    rowColumns.Add(text);
                }

                data.Rows.Add(rowColumns);
            }

            BalanceTableColumns(data);

            return data;
        }
        #endregion

        #region Csv Processing

        public TableData ParseCsvFileToData(string filename, string delimiter = ",")
        {
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                return new TableData();

            FileStream fs = null;
            try
            {
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                return ParseCsvStreamToData(fs, delimiter);
            }
            catch
            {
                return new TableData();
            }
            finally
            {
                fs?.Dispose();
            }
        }

        public TableData ParseCsvStringToData(string csvContent, string delimiter = ",")
        {
            if (string.IsNullOrEmpty(csvContent))
                return new TableData();

            var bytes = Encoding.UTF8.GetBytes(csvContent);
            using (var fs = new MemoryStream(bytes, 0, bytes.Length))
            {
                return ParseCsvStreamToData(fs, delimiter);
            }
        }


        public TableData ParseCsvStreamToData(Stream stream, string delimiter = ",")
        {
            if (string.IsNullOrEmpty(delimiter))
                delimiter = ",";
            if (delimiter == "\\t")
                delimiter = "\t";

            char charDelimiter = delimiter[0];

            bool firstLine = true;
            using (var reader = new StreamReader(stream))
            {
                
                using (var csv = new CachedCsvReader(reader, true, charDelimiter))
                {
                    var list = new TableData();

                    var colCount = csv.Columns.Count;
                    var columnCollection = new List<string>();

                    if (!csv.ReadNextRecord())
                        return list;

                    for (var index = 0; index < csv.Columns.Count; index++)
                    {
                        Column column = csv.Columns[index];
                        columnCollection.Add(column.Name);
                    }

                    if (firstLine)
                    {
                        list.Headers = columnCollection;
                        firstLine = false;
                    }
                    else 
                        list.Rows.Add(columnCollection);

                    // Field headers will automatically be used as column names
                    while (true)
                    {
                        columnCollection = new List<string>();
                        for (int index = 0; index < csv.Columns.Count; index++)
                        {
                            var colValue = csv[index];
                            columnCollection.Add(colValue);
                        }
                        list.Rows.Add(columnCollection);

                        if (!csv.ReadNextRecord())
                            break;
                    }

                    return list;
                }
            }
        }

        #endregion


        #region Format Routines

        /// <summary>
        /// Re-Formats a Markdown table to nicely formatted output (size permitting)
        /// </summary>
        /// <param name="tableMarkdown"></param>
        /// <returns>formatted markdown, if it can't be formatted original is returned</returns>
        public string FormatMarkdownTable(string tableMarkdown)
        {
            var parser = new TableParserHtml();
            var type = parser.DetectTableType(tableMarkdown);
            if (type == MarkdownTableType.None)
                return null;

            var tableData = ParseMarkdownToData(tableMarkdown);
            if (tableData == null)
                return tableMarkdown;

            string output = null;
            switch (type)
            {
                case MarkdownTableType.Pipe:
                    output = parser.ToPipeTableMarkdown(tableData);
                    break;
                case MarkdownTableType.Grid:
                    output = parser.ToGridTableMarkdown(tableData);
                    break;
                case MarkdownTableType.Html:
                    output = parser.ToTableHtml(tableData);
                    break;
            }

            return output;
        }

        
        /// <summary>
        /// Fixes up table columns to match the widest row. Table header and rows
        /// are all fixed up to match the widest row of columns. Empty columns and
        /// headers are created with empty text.
        /// </summary>
        /// <param name="data"></param>
        private static void BalanceTableColumns(TableData data)
        {
            if (data == null || data.Rows.Count < 1)
                return;

            // Check to see if the header has less columns than max colums in any row
            var headerCols = data.Headers.Count;
            var maxCols = data.Rows.Max(d => d.Count);
            if (maxCols == headerCols)
                return;
            if (headerCols > maxCols)
                maxCols = headerCols;

            // add header cols
            if (data.Headers.Count < maxCols)
            {
                int add = maxCols - data.Headers.Count;
                for (int x = 0; x < add; x++)
                    data.Headers.Add(string.Empty);
            }

            // add row cols
            foreach (var row in data.Rows)
            {
                if (row.Count < maxCols)
                {
                    int add = maxCols - row.Count;
                    for (int x = 0; x < add; x++)
                        row.Add(string.Empty);
                }
            }
        }

        /// <summary>
        /// Parses out links and images and replaces them with Markdown text
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        string ParseLinkAndImage(string html)
        {
            if (!html.Contains("<"))
                return html;

            string img = "x";
            while (!string.IsNullOrEmpty(img))
            {
                img = StringUtils.ExtractString(html, "<img ", ">", returnDelimiters: true);
                if (string.IsNullOrEmpty(img))
                    break;

                var src = StringUtils.ExtractString(img, "src=\"", "\"");
                html = html.Replace(img, $"![]({src})");
            }

            return html;
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Retrieves information about each of the columns in the table including
        /// max width and title. Looks at the first row of the table data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<ColumnInfo> GetColumnInfo(TableData data)
        {
            var headers = new List<ColumnInfo>();
            if (data == null || data.Headers.Count < 1 && data.Rows.Count < 1)
                return headers;

            var cols = data.Headers;
            if(cols == null || cols.Count < 0)
                return headers;

            for (int i = 0; i < cols.Count; i++)
            {
                var header = cols[i]?.Trim() ?? string.Empty;
                var colInfo = new ColumnInfo
                {
                    Title = header,
                    MaxWidth = header.Length
                };

                if (header.EndsWith(":") && header.StartsWith(":"))
                    colInfo.Justification = ColumnJustifications.Center;
                else if(header.EndsWith(":"))
                    colInfo.Justification = ColumnJustifications.Right;

                // figure out max length in rows
                for (int x = 0; x < data.Rows.Count; x++)
                {
                    var row = data.Rows[x];

                    var colText = row[i];
                    if (colText.IndexOf('\n') < 0)
                    {
                        if (colText.Length > colInfo.MaxWidth)
                            colInfo.MaxWidth = colText.Length;
                    }
                    else
                    {
                        // max width for multiple lines
                        var tokens = colText.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        var maxLength = tokens.Max(t => t.Trim().Length);
                        if (maxLength > colInfo.MaxWidth)
                            colInfo.MaxWidth = maxLength;
                    }
                }
                headers.Add(colInfo);
            }

            return headers;
        }


        /// <summary>
        /// determines if a string contains a given type of Markdown Table
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public MarkdownTableType DetectTableType(string tableMarkdown)
        {
            if (tableMarkdown.Trim().StartsWith("+-") && tableMarkdown.Trim().EndsWith("-+"))
                return MarkdownTableType.Grid;

            if (tableMarkdown.IndexOf("<table", StringComparison.InvariantCultureIgnoreCase) > -1 &&
                tableMarkdown.IndexOf("</table>", StringComparison.InvariantCultureIgnoreCase) > -1)
                return MarkdownTableType.Html;

            if (tableMarkdown.Contains("|"))
                return MarkdownTableType.Pipe;

            return MarkdownTableType.None;
        }
        #endregion
    }



}
