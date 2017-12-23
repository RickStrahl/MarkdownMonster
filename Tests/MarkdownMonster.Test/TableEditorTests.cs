using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    class TableEditorTests
    {
        DataTable TableData;
        private string TableHeaders;

        [TestMethod]
        public void DataTableToHtmlTest()
        {
            var colHeaders = "Column 1,Column 2,Column 3";
            var dt = TableParser.CreateDataTable("Column 1,Column 2,Column 3");

            //TableParser.ParseDataTableToHtml(dt,)

        }
        



    }
}
