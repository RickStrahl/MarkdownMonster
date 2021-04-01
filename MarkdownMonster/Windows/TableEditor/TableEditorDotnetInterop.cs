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
    }

    /// <summary>
    /// Class that is called back to from JavaScript - this is passed into the
    /// page by calling `InitializeInterop()` in script (WebBrowser.InvokeScript())
    /// </summary>
    public class TableEditorJavaScriptCallbacks 
    {
        WebBrowser WebBrowser;

        public TableEditorJavaScriptCallbacks(WebBrowser browser)
        {
            WebBrowser = browser;    
        }

        public void ShowContextMenu(object mousePosition)
        {
            int x = (int) Convert.ToInt32(ReflectionUtils.GetPropertyCom(mousePosition, "x"));
            int y = (int) Convert.ToInt32(ReflectionUtils.GetPropertyCom(mousePosition, "y"));

            var cm = new ContextMenu();
            cm.Items.Add( new MenuItem() {Header = x + ":" + y});

            cm.PlacementTarget = WebBrowser;
            cm.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;

            cm.Focus();
            cm.IsOpen = true;

            var item = cm.Items[0] as MenuItem;
            item.Focus();
        }

    }
}
