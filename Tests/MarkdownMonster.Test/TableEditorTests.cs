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
            var colHeaders = "Column 1,Column 2,Column 3";
            var data = GetTableData();

            var parser = new TableParser();
            var colInfo = parser.GetColumnInfo(data, colHeaders);

            Assert.IsNotNull(colInfo);
            Console.WriteLine(JsonSerializationUtils.Serialize(colInfo));
        }

        [TestMethod]
        public void DataToHtmlTest()
        {
            var colHeaders = "Column 1,Column 2,Column 3";            
            var data = GetTableData();
            
            var parser = new TableParser();
            string html = parser.ParseDataToHtml(data,colHeaders);

            Console.WriteLine(html);
        }

        ObservableCollection<ObservableCollection<ColumnText>> GetTableData()
        {
            var parser = new TableParser();
            var data = parser.TableData;
            data.Add(new ObservableCollection<ColumnText> { new ColumnText("Column 1"), new ColumnText("Column 2 Text"), new ColumnText("Column 3") });
            data.Add(new ObservableCollection<ColumnText> { new ColumnText("Column 4 and a bottle of Russian rum"), new ColumnText("Column 5 Text"), new ColumnText("Column 5.5") });
            data.Add(new ObservableCollection<ColumnText> { new ColumnText("Column 6"), new ColumnText("Column 7 Text"), new ColumnText("Column 8") });

            return data;
        }



    }
}
