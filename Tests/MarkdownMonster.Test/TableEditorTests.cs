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

        ObservableCollection<ObservableCollection<string>> GetTableData()
        {
            var parser = new TableParser();
            var data = parser.TableData;
            data.Add(new ObservableCollection<string> { "Column 1", "Column 2 Text", "Column 3" });
            data.Add(new ObservableCollection<string> { "Column 4.1111 item", "Column 5 Text", "Column 5.5" });
            data.Add(new ObservableCollection<string> { "Column 6", "Column 7 Text", "Column 8" });

            return data;
        }



    }
}
