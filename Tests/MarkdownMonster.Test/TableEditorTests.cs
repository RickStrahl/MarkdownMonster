using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class TableEditorTests
    {

        [TestMethod]
        public void ColumnInfoTest()
        {
            var data = GetTableData();

            var parser = new TableParser();
            var colInfo = parser.GetColumnInfo(data);

            Assert.IsNotNull(colInfo);
            Console.WriteLine(JsonSerializationUtils.Serialize(colInfo));
        }

        [TestMethod]
        public void DataToMarkdownTest()
        {
            var data = GetTableData();
            
            var parser = new TableParser();
            string html = parser.ParseDataToMarkdown(data);

            Console.WriteLine(html);

            Assert.IsTrue(html.Contains("| Column 8   |"));
        }


        [TestMethod]
        public void DataToHtmlTest()
        {
            var data = GetTableData();

            var parser = new TableParser();
            string html = parser.ParseDataToHtml(data);

            Console.WriteLine(html);

            Assert.IsTrue(html.Contains("<td>Column 5 Text</td>"));
        }



        [TestMethod]
        public void ParseMarkdownToDataTest()
        {
            string md = @"
| Column1              | Column2  | Column3  |
|--------------------------------------------|
| Header 1             | Header 2 | Header 3 |
| Custom Table Content | Column 2 | Column 3 |
| Column 4             | Column 5 | Column 6 |
";

            var parser = new TableParser();
            var data = parser.ParseMarkdownToData(md);

            Assert.IsTrue(data.Count == 4, "Table should have returned 4 rows");
            Assert.IsTrue(data[1][1].Text == "Header 2");
        }

        ObservableCollection<ObservableCollection<CellContent>> GetTableData()
        {
            var parser = new TableParser();
            var data = parser.TableData;
            data.Add(new ObservableCollection<CellContent> { new CellContent("Column 1"), new CellContent("Column 2 Text"), new CellContent("Column 3") });
            data.Add(new ObservableCollection<CellContent> { new CellContent("Column 4 and a bottle of Russian rum"), new CellContent("Column 5 Text"), new CellContent("Column 5.5") });
            data.Add(new ObservableCollection<CellContent> { new CellContent("Column 6"), new CellContent("Column 7 Text"), new CellContent("Column 8") });

            return data;
        }



    }
}
