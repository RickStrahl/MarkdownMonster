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
    public class TableEditorHtmlTests
    {

        [TestMethod]
        public void ColumnInfoTest()
        {
            var data = GetTableData();

            var parser = new TableParserHtml();
            var colInfo = parser.GetColumnInfo(data);

            Assert.IsNotNull(colInfo);
            Console.WriteLine(JsonSerializationUtils.Serialize(colInfo));
        }

        #region DataTable Conversion to Markdown/Html

        [TestMethod]
        public void DataToMarkdownPipeTableTest()
        {
            var data = GetTableData();

            var parser = new TableParserHtml();
            string html = parser.ToPipeTableMarkdown(data);

            Console.WriteLine(html);

            Assert.IsTrue(html.Contains("| Row 8 Column 1  |"));
        }


        [TestMethod]
        public void DataToMarkdownPipeTableMultiLineTest()
        {
            var data = GetTableMultiLineData();

            var parser = new TableParserHtml();
            string html = parser.ToPipeTableMarkdown(data);

            Console.WriteLine(html);

            Assert.IsTrue(html.Contains("| Row 8 Column 1  |"));
        }



        [TestMethod]
        public void DataToMarkdownGridTableTest()
        {
            var data = GetTableData();

            var parser = new TableParserHtml();
            string html = parser.ToGridTableMarkdown(data);

            Console.WriteLine(html);

            Assert.IsTrue(html.Contains("| Row 8 Column 1  |"));
        }

        [TestMethod]
        public void DataToMarkdownGridTableMultiLineTest()
        {
            var data = GetTableMultiLineData();

            var parser = new TableParserHtml();
            string html = parser.ToGridTableMarkdown(data);

            Console.WriteLine(html);

            Assert.IsTrue(html.Contains("| Row 8 Column 1  |"));
        }

        
        [TestMethod]
        public void DataToHtmlTableTest()
        {
            var data = GetTableData();

            var parser = new TableParserHtml();
            string html = parser.ToTableHtml(data);

            Console.WriteLine(html);

            Assert.IsTrue(html.Contains("<td style=\"text-align: center\">Row 7 Column 2</td>"));
        }

        [TestMethod]
        public void DataToHtmlTableMultiLineTest()
        {
            var data = GetTableMultiLineData();

            var parser = new TableParserHtml();
            string html = parser.ToTableHtml(data);

            Console.WriteLine(html);

            Assert.IsTrue(html.Contains("<td style=\"text-align: center\">Row 7 Column 2</td>"));
        }

        #endregion

        #region Parse Markdown/HTML To TableData


        [TestMethod]
        public void ParseMarkdownPipeTableToDataTest()
        {
            string md = @"
        | Header1              | Header 2 | Header 3 |
        |--------------------:|----------|----------|
        | column 1             | Column 2 | Column 3 |
        | Custom Table Content | Column 5 | Column 6 |
        | Column 7             | Column 8 | Column 9 |
        ";

            var parser = new TableParserHtml();
            var data = parser.ParseMarkdownToData(md);

            Console.WriteLine(data.Rows.Count);
            Console.WriteLine(parser.ToPipeTableMarkdown(data));

            Assert.IsTrue(data.Rows.Count == 3, "Table should have returned 3 rows");
            Assert.IsTrue(data.Rows[0][1] == "Column 2");
        }

        [TestMethod]
        public void ParseMarkdownPipeTableToDataTest2()
        {
            string md = @"
        a  | b 
        --|--
        0  | 1 
        3  | 4
        ";

            var parser = new TableParserHtml();
            var data = parser.ParseMarkdownToData(md);

            Console.WriteLine(data.Rows.Count);
            Console.WriteLine(parser.ToPipeTableMarkdown(data));

            Assert.IsTrue(data.Rows.Count == 2, "Table should have returned 2 rows");
            Assert.IsTrue(data.Rows[0][1] == "1");
        }


        [TestMethod]
        public void UnbalancedPipeTableParsingTest()
        {
            var md = @"| Header 1 | Header 2 |
        |------------|------------|------------|
        | Column 1   | Column 2   | More Stuff |
        | Column 1.1 | Column 2.1 | More Stuff |";


            var parser = new TableParserHtml();
            var data = parser.ParseMarkdownToData(md);

            Console.WriteLine(data.Rows.Count);
            Console.WriteLine(parser.ToPipeTableMarkdown(data));

            Assert.IsTrue(data.Rows.Count == 2, "Table should have returned 2 rows");
            Assert.IsTrue(data.Rows[0][1] == "Column 2");
        }

        [TestMethod]
        public void ExtraUnbalancedPipeTableParsingTest()
        {
            var md = @"| Header 1 | Header 2 |
        |------------|------------|------------|
        | Column 1   | Column 2   |
        | Column 1.1 | Column 2.1 | More Stuff | Even More Stuff |";

            Console.WriteLine($"Original: \n{md}\n\n");

            var parser = new TableParserHtml();
            var data = parser.ParseMarkdownToData(md);

            Console.WriteLine(data.Rows.Count + " Rows Parsed:");
            Console.WriteLine(parser.ToPipeTableMarkdown(data));

            Assert.IsTrue(data.Rows.Count == 2, "Table should have returned 2 rows");
            Assert.IsTrue(string.IsNullOrWhiteSpace(data.Rows[0][2]));  // header
            Assert.IsTrue(!string.IsNullOrWhiteSpace(data.Rows[1][2]));
            Assert.IsTrue(data.Rows[0][1] == "Column 2");
            Assert.IsTrue(data.Rows[1][2] == "More Stuff");
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
    | Column 1 Row 2                           | Column 5 Text  | Column 5.5 |
    | and a bottle of Russian rum              |                |            |
    | with broken glass                        |                |            |
    +------------------------------------------+----------------+------------+
    | Column 6                                 | Column 7 Text  | Column 8   |
    +------------------------------------------+----------------+------------+
    | Column 9                                 | Column 10 Text | Column 11  |
    | ho ho and a bottle of rum                |                |            |
    +------------------------------------------+----------------+------------+
        ";

            var parser = new TableParserHtml();
            var data = parser.ParseMarkdownToData(md);

            Console.WriteLine(data.Rows.Count + " Rows Parsed:");
            Console.WriteLine(parser.ToGridTableMarkdown(data));

            Assert.IsTrue(data.Rows.Count == 4, "Table should have returned 4 rows");
            Assert.IsTrue(data.Rows[1][0].Contains("\nand a bottle of"));
        }

        [TestMethod]
        public void ParseUnbalancedMarkdownGridTableToDataTest()
        {
            string md = @"
        +------------------------------------------+----------------+------------+
        | Header 1                                 | Header 2       |
        +==========================================+================+============+
        | Column 1                                 | Column 2 Text  | Column 3   |
        +------------------------------------------+----------------+------------+
        | Column 4                                 | Column 5 Text  | 
        | and a bottle of Russian rum              |                |            
        | with broken glass                        |                |           
        +------------------------------------------+----------------+------------+
        | Column 6                                 | Column 7 Text  | Column 8   |
        +------------------------------------------+----------------+------------+
        | Column 9                                 | Column 10 Text | Column 11  |
        | ho ho and a bottle of rum                |                |            |
        +------------------------------------------+----------------+------------+
        ";

            var parser = new TableParserHtml();
            var data = parser.ParseMarkdownToData(md);

            Console.WriteLine(data.Rows.Count);
            Console.WriteLine(parser.ToGridTableMarkdown(data));

            Assert.IsTrue(data.Rows.Count == 4, "Table should have returned 4 rows");

            Assert.IsTrue(data.Headers[0] == "Header 1");  // header filled
            Assert.IsTrue(data.Rows[1][1] == "Column 5 Text", data.Rows[1][1] + "!");  // 2nd row filled

            Assert.IsTrue(data.Rows[1][0].Contains("\nand a bottle of"));

        }


        [TestMethod]
        public void ParseComplexMarkdownGridTableToDataTest()
        {
            var md = @"
        +-----------------------------------+-----------------------------------+
        | Attribute                         | Function                          |
        +===================================+===================================+
        | **display: flex**                 | Top level attribute that enables  |
        |                                   | Flexbox formatting on the         |
        |                                   | container it is applied to.       |
        |                                   |                                   |
        |                                   | **display:flex**                  |
        +-----------------------------------+-----------------------------------+
        | **flex-direction**                | Determines horizontal (row) or    |
        |                                   | vertical (column) flow direction  |
        |                                   | elements in the container.        |
        |                                   |                                   |
        |                                   | **row,column**                    |
        +-----------------------------------+-----------------------------------+
        | **flex-wrap**                     | Determines how content wraps when |
        |                                   | the content overflows the         |
        |                                   | container.                        |
        |                                   |                                   |
        |                                   | **wrap, nowrap, wrap-reverse**    |
        +-----------------------------------+-----------------------------------+
        | **flex-flow**                     | Combination of flex-direction and |
        |                                   | flex-wrap as a single attribute.  |
        |                                   |                                   |
        |                                   | **flex-flow: row nowrap**         |
        +-----------------------------------+-----------------------------------+
        | **justify-content**               | Aligns content along the flex     |
        |                                   | flow direction.                   |
        |                                   |                                   |
        |                                   | **flex-start, flex-end, center,   |
        |                                   | space-between, space-around**     |
        +-----------------------------------+-----------------------------------+
        | **align-items**                   | Like align-content but aligns     |
        |                                   | content along the perpendicular   |
        |                                   | axis.                             |
        |                                   |                                   |
        |                                   | **flex-start, flex-end, center,   |
        |                                   | stretch, baseline**               |
        +-----------------------------------+-----------------------------------+
        | **align-content**                 | Aligns multi-line content so that |
        |                                   | multiple lines of content line up |
        |                                   | when wrapping.                    |
        |                                   |                                   |
        |                                   | **flex-start, flex-end, center,   |
        |                                   | space-between, space-around,      |
        |                                   | stretch**                         |
        +-----------------------------------+-----------------------------------+
        ";

            var parser = new TableParserHtml();
            var data = parser.ParseMarkdownToData(md);

            Console.WriteLine(data.Rows.Count + " Rows Parsed:");
            Console.WriteLine(parser.ToGridTableMarkdown(data));

            Assert.IsTrue(data.Rows.Count == 7, "Table should have returned 7 rows");
            Assert.IsTrue(data.Rows[1][0].Contains("**flex-direction**"));


        }

        [TestMethod]
        public void SimpleHtmlTableToData()
        {
            string html = @"
        <table>
        <thead>
        <tr>
            <th>Column1</th>
            <th>Column2</th>
            <th>Column3</th>
        </tr>
        </thead>
        <tbody>
        <tr>
            <td>Column1 Row 1</td>
            <td>Column2 Row 1</td>
            <td>Column3 Row 1</td>
        </tr>
        <tr>
            <td>Column1 Row 2</td>
            <td>Column2 Row 2</td>
            <td>Column3 Row 2</td>
        </tr>
        <tr>
            <td>Column1 Row 3</td>
            <td>Column2 Row 3</td>
            <td>Column3 Row 3</td>
        </tr>
        </tbody>
        </html>
        ";

            var parser = new TableParserHtml();
            var data = parser.ParseHtmlToData(html);

            Assert.IsNotNull(data);

            Console.WriteLine(data.Rows.Count + " Rows Parsed:");
            Console.WriteLine(parser.ToGridTableMarkdown(data));

        }


        [TestMethod]
        public void SimpleHtmlTableNoHeaderToData()
        {
            string html = @"
        <table>
        <tbody>
        <tr>
            <td>Column1</td>
            <td>Column2</td>
            <td>Column3</td>
        </tr>
        <tr>
            <td>Column1 Row 1</td>
            <td>Column2 Row 1</td>
            <td>Column3 Row 1</td>
        </tr>
        <tr>
            <td>Column1 Row 2</td>
            <td>Column2 Row 2</td>
            <td>Column3 Row 2</td>
        </tr>
        <tr>
            <td>Column1 Row 3</td>
            <td>Column2 Row 3</td>
            <td>Column3 Row 3</td>
        </tr>
        </tbody>
        </html>
        ";

            var parser = new TableParserHtml();
            var data = parser.ParseHtmlToData(html);

            Assert.IsNotNull(data);

            Console.WriteLine(data.Rows.Count + " Rows Parsed:");
            Console.WriteLine(parser.ToGridTableMarkdown(data));

        }

        [TestMethod]
        public void SimpleHtmlTableWithBasicMarkupToData()
        {
            string html = @"
        <table>
        <tr>
            <th>Column1</th>
            <th>Column2</th>
            <th>Column3</th>
        </tr>
        </thead>
        <tbody>
        <tr>
            <td>Column1 Row 1</td>
            <td>Column2 Row 1</td>
            <td>Column3 Row 1</td>
        </tr>
        <tr>
            <td><img src=""https://markdownmonster.west-wind.com/Images/MarkdownMonster_Icon_32.png"" />Column1 <b>RowBold</b></td>
            <td><a href=""https://west-wind.com"">Column2</a> <a href=""http://google.com"">Row 2</a></td>
            <td>Column3 Row 2</td>
        </tr>
        <tr>
            <td>Column1 Row 3</td>
            <td>Column2 Row 3</td>
            <td>Column3 Row 3</td>
        </tr>
        </table>
        ";

            var parser = new TableParserHtml();
            var data = parser.ParseHtmlToData(html);

            Assert.IsNotNull(data);

            Console.WriteLine(data.Rows.Count + " Rows Parsed:");
            Console.WriteLine(parser.ToGridTableMarkdown(data));

        }
        #endregion

        #region Format Markdown Table


        [TestMethod]
        public void FormatPipeTableTest()
        {
            string md = @"
        | Header1 | Header 2   | Header 3    |
        |--------------------------------------------|
        | column 1 | Column 2 | Column 3 |
        | Custom Table Content | Column 5 | Column 6 |
        | Column 7  | Column 8 | Column 9 |
        ";

            var parser = new TableParserHtml();
            var niceMd = parser.FormatMarkdownTable(md);
            Console.WriteLine(niceMd);
            Assert.IsTrue(niceMd.Contains("| Column 7             |"), "Pipe is not formatted as expected");
        }


        [TestMethod]
        public void FormatGridTableTest()
        {
            string md = @"
        +------------------------+----------------+------------+
        | Header 1 | Header 2       | Header 3   |
        +==========================================+================+============+
        | Column 1  | Column 2 Text  | Column 3   |
        +------------------------------------------+----------------+------------+
        | Column 4                     | Column 5 Text  | Column 5.5 |
        | and a bottle of Russian rum              |                |            |
        | with broken glass   |                |            |
        +------------------------------------------+----------------+------------+
        | Column 6                      | Column 7 Text  | Column 8   |
        +------------------------------------------+----------------+------------+
        | Column 9      | Column 10 Text | Column 11  |
        | ho ho and a bottle of rum                |                |            |
        +------------------------------------------+----------------+------------+
        ";
            var parser = new TableParserHtml();
            var niceMd = parser.FormatMarkdownTable(md);

            Console.WriteLine(niceMd);
            Assert.IsTrue(niceMd.Contains("| Column 1                    |"), "Grid is not formatted as expected");
        }

        [TestMethod]
        public void FormatHtmlTableTest()
        {
            string md = @"
        <table>
        <tbody>
        <tr>    <td>Column1</td>     <td style='text-align: right'>Column2</td>     <td>Column3</td> </tr>
        <tr>
            <td>Column1 Row 1</td><td>Column2 Row 1</td><td>Column3 Row 1</td>
        </tr>
        <tr>    <td>Column1 Row 2</td>
            <td>Column2 Row 2</td><td>Column3 Row 2</td>
        </tr>
        <tr>
            <td>Column1 Row 3</td>
            <td>Column2 Row 3</td>
            <td>Column3 Row 3</td>
        </tr></tbody></table></html>
        ";

            var parser = new TableParserHtml();
            var niceMd = parser.FormatMarkdownTable(md);

            Console.WriteLine(niceMd);
            Assert.IsTrue(niceMd.Contains("	<td>Column3 Row 3</td>"), "Pipe is not formatted as expected");
        }

        #endregion

        [TestMethod]
        public void CsvTableParserFromStringTest()
        {

            string data = @"Name,Company,city,Test

    Rick,West Wind,Paia,4
    Markus,EPS,Kihei,20,11
    Kevin,Oak Leaf,Bumstuck VA,4
";

            var parser = new TableParserHtml();
            var tableData = parser.ParseCsvStringToData(data, ",");

            Console.WriteLine(parser.ToGridTableMarkdown(tableData));
        }

        //[TestMethod]
        //public void CsvTableParserFromFileTest()
        //{
        //    var parser = new TableParser();
        //    var tableData = parser.ParseCsvFileToData(@"c:\temp\Names.csv", ",");
        //    Console.WriteLine(parser.ToGridTableMarkdown(tableData));
        //}



        [TestMethod]
        public void DetectTableTypeTest()
        {
            string mdPipe = @"
        | Header1              | Header 2 | Header 3 |
        |--------------------------------------------|
        | column 1             | Column 2 | Column 3 |
        | Custom Table Content | Column 5 | Column 6 |
        | Column 7             | Column 8 | Column 9 |
        ";

            var parser = new TableParserHtml();
            var type = parser.DetectTableType(mdPipe);

            Assert.IsTrue(type == MarkdownTableType.Pipe, "Not a Pipe Table");


            string mdHtml = @"
        <table>
        <tbody>
        <tr>
            <td>Column1</td>
            <td>Column2</td>
            <td>Column3</td>
        </tr>
        <tr>
            <td>Column1 Row 1</td>
            <td>Column2 Row 1</td>
            <td>Column3 Row 1</td>
        </tr>
        <tr>
            <td>Column1 Row 2</td>
            <td>Column2 Row 2</td>
            <td>Column3 Row 2</td>
        </tr>
        <tr>
            <td>Column1 Row 3</td>
            <td>Column2 Row 3</td>
            <td>Column3 Row 3</td>
        </tr>
        </tbody>
        </table>
        ";

            type = parser.DetectTableType(mdHtml);
            Assert.IsTrue(type == MarkdownTableType.Html, "Not an HTML table");

            string mdGrid = @"
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

            type = parser.DetectTableType(mdGrid);
            Assert.IsTrue(type == MarkdownTableType.Grid, "Not a Grid Table");
        }




        #region Sample Data Setup
        TableData GetTableData()
        {
            var data = new TableData();
            data.Headers.AddRange(new[] { "Header 1", "Header 2", ":Header 3:", "Header 4:" });


            for (int i = 0; i < 11; i++)  // rows
            {
                var cols = new List<string>();
                for (int j = 0; j < 5; j++)   // cols
                {
                    cols.Add($"Row {i} Column {j}");
                }
                data.Rows.Add(cols);
            }

            return data;
        }


        TableData GetTableMultiLineData()
        {
            var data = new TableData();
            data.Headers.AddRange(new[] { "Header 1", "Header 2", ":Header 3:", "Header 4:" });


            for (int i = 0; i < 11; i++)  // rows
            {
                var cols = new List<string>();
                for (int j = 0; j < 5; j++)   // cols
                {
                    var text = $"Row {i} Column {j}";
                    if (j == 0 || j == 3)
                    {
                        text += "\nLine 2 of text";
                    }
                    cols.Add(text);
                }
                data.Rows.Add(cols);
            }

            return data;
        }



        #endregion

    }
}
