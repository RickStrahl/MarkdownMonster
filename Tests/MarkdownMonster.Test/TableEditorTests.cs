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
        public void DataToMarkdownPipeTableTest()
        {
            var data = GetTableData();
            
            var parser = new TableParser();
            string html = parser.ParseDataToPipeTableMarkdown(data);

            Console.WriteLine(html);

            Assert.IsTrue(html.Contains("| Column 8   |"));
        }

        [TestMethod]
        public void DataToMarkdownGridTableTest()
        {
            var data = GetTableData();

            var parser = new TableParser();
            string html = parser.ParseDataToGridTableMarkdown(data);

            Console.WriteLine(html);

            Assert.IsTrue(html.Contains("| ho ho and"));
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
        public void ParseMarkdownPipeTableToDataTest()
        {
            string md = @"
| Header1              | Header 2 | Header 3 |
|--------------------------------------------|
| column 1             | Column 2 | Column 3 |
| Custom Table Content | Column 5 | Column 6 |
| Column 7             | Column 8 | Column 9 |
";

            var parser = new TableParser();
            var data = parser.ParseMarkdownToData(md);

            Assert.IsTrue(data.Count == 4, "Table should have returned 4 rows");
            Assert.IsTrue(data[1][1].Text == "Header 2");
        }

        [TestMethod]
        public void ParseMarkdownGridTableToDataTest()
        {
            string md = @"
+------------------------------------------+----------------+------------+
| Header 1                                 | Header 2       | Header 3   |
+==========================================+================+============+
| Column 1                                 | Column 2 Text  | Column 3   |
+------------------------------------------+----------------+------------+
| Column 4                                 | Column 5 Text  | Column 5.5 |
| and a bottle of Russian rum              |                |            |
| with broken glass                        |                |            |
+------------------------------------------+----------------+------------+
| Column 6                                 | Column 7 Text  | Column 8   |
+------------------------------------------+----------------+------------+
| Column 9                                 | Column 10 Text | Column 11  |
| ho ho and a bottle of rum                |                |            |
+------------------------------------------+----------------+------------+
";

            var parser = new TableParser();
            var data = parser.ParseMarkdownToData(md);

            Console.WriteLine(data.Count);
            Console.WriteLine(parser.ParseDataToGridTableMarkdown(data));

            Assert.IsTrue(data.Count == 5, "Table should have returned 5 rows");
            Assert.IsTrue(data[2][0].Text.Contains("\nand a bottle of"));

            
        }

        ObservableCollection<ObservableCollection<CellContent>> GetTableData()
        {
            var parser = new TableParser();
            var data = parser.TableData;
            data.Add(new ObservableCollection<CellContent> { new CellContent("Header 1"), new CellContent("Header 2"), new CellContent("Header 3") });
            data.Add(new ObservableCollection<CellContent> { new CellContent("Column 1"), new CellContent("Column 2 Text"), new CellContent("Column 3") });
            data.Add(new ObservableCollection<CellContent> { new CellContent("Column 4\nand a bottle of Russian rum\nwith broken glass"), new CellContent("Column 5 Text"), new CellContent("Column 5.5") });
            data.Add(new ObservableCollection<CellContent> { new CellContent("Column 6"), new CellContent("Column 7 Text"), new CellContent("Column 8") });
            data.Add(new ObservableCollection<CellContent> { new CellContent("Column 9\nho ho and a bottle of rum"), new CellContent("Column 10 Text"), new CellContent("Column 11") });

            return data;
        }



    }
}
