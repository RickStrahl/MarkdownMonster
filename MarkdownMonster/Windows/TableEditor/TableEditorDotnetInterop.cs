using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    public class TableEditorDotnetInterop : BaseBrowserInterop
    {
        private object Page;

       

        public TableEditorDotnetInterop(object instance) : base(instance)
        {


            
           

        }

        #region Call into JavaScript from .NET

        public TableData GetJsonTableData()
        {
            string tdata = Invoke("parseTable", true) as string;  // asJson
            if(string.IsNullOrEmpty(tdata))
                return null;

            var td = JsonSerializationUtils.Deserialize<TableData>(tdata);
            return td;
        }


        public void UpdateHtmlTable(TableData data, TableLocation location)
        {
            Invoke("renderTable",
                 BaseBrowserInterop.SerializeObject(data),
                BaseBrowserInterop.SerializeObject(location));
        }
            
        #endregion
    }

    /// <summary>
    /// Class that is called back to from JavaScript - this is passed into the
    /// page by calling `InitializeInterop()` in script (WebBrowser.InvokeScript())
    /// </summary>
    public class TableEditorJavaScriptCallbacks 
    {
        TableEditorHtml Window;

        public TableEditorJavaScriptCallbacks(TableEditorHtml window)
        {
            Window = window;
        }

        public void UpdateTableData(string jsonTable)
        {
            var td = JsonSerializationUtils.Deserialize<TableData>(jsonTable);
            if (td != null)
                Window.TableData = td;
        }

        public void ShowContextMenu(object mousePosition)
        {
            // get the latest editor table data
            Window.Interop.GetJsonTableData();

            // incoming row data is: row 0 = header, actual rows 1 based 
            var loc = new TableLocation();
            loc.Row = Convert.ToInt32( ReflectionUtils.GetPropertyCom(mousePosition, "row") );
            loc.Column = Convert.ToInt32( ReflectionUtils.GetPropertyCom(mousePosition, "col") );
            loc.IsHeader = loc.Row < 1;

            // Fix up row number to 0 based
            if (!loc.IsHeader)
                loc.Row--;
            
            var ctx  = new TableEditorContextMenu(Window, loc);
            ctx.ShowContextMenu();
        }

    }

    
}
