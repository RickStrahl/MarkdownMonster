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
            int x = (int) Convert.ToInt32(ReflectionUtils.GetPropertyCom(mousePosition, "x"));
            int y = (int) Convert.ToInt32(ReflectionUtils.GetPropertyCom(mousePosition, "y"));

            var cm = new ContextMenu();
            cm.Items.Add( new MenuItem() {Header = x + ":" + y});

            cm.PlacementTarget = Window.WebBrowser;
            cm.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;

            cm.Focus();
            cm.IsOpen = true;

            var item = cm.Items[0] as MenuItem;
            item.Focus();
        }




    }
}
