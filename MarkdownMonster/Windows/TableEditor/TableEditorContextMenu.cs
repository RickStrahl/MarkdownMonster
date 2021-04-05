using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MarkdownMonster.Windows
{
    public class TableEditorContextMenu
    {
        private TableEditorHtml Window { get; }
        private TableLocation TableLocation { get; }

        private ContextMenu ContextMenu = new ContextMenu();

        private AppModel Model;


        /// <summary>
        /// This allows adding/removing items on the context menu from a plug in
        /// </summary>
        public static event EventHandler<ContextMenu> ContextMenuOpening;

        public TableEditorContextMenu(TableEditorHtml window, TableLocation tableLocation)
        {
            Window = window;
            TableLocation = tableLocation;
            Model = mmApp.Model;
        }


        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            ContextMenu.IsOpen = false;

            //Model.ActiveEditor?.SetEditorFocus();
            ClearMenu();
        }

        /// <summary>
        /// Clears all items off the menu
        /// </summary>
        public void ClearMenu()
        {
            if (ContextMenu == null)
            {
                ContextMenu = new ContextMenu();
                ContextMenu.Closed += ContextMenu_Closed;
            }
            else
                ContextMenu?.Items.Clear();
        }

        public void Show()
        {
            ContextMenuOpening?.Invoke(this, ContextMenu);

            if (ContextMenu != null)
            {
                ContextMenu.PlacementTarget = Window.WebBrowser;
                ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                ContextMenu.VerticalOffset = 8;

                ContextMenu.Focus();
                ContextMenu.IsOpen = true;
                Window.WebBrowser.ContextMenu = ContextMenu;

                var item = ContextMenu.Items[0] as MenuItem;

                Window.Activate();
                ContextMenu.Dispatcher.InvokeAsync(() => item.Focus(),
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
            
        }

        public void ShowContextMenu()
        {
            ClearMenu();

            var cm = ContextMenu;
            cm.Items.Clear();

            MenuItem ci;

            var sub = new MenuItem() {Header = "Align Column..."};

            ci = new MenuItem() {Header = "Align Right"};
            ci.Click += (s, e) =>
            {
                var text = Window.TableData.Headers[TableLocation.Column];
                text = text.Trim(':') + ":";
                Window.TableData.Headers[TableLocation.Column] = text;

                Window.Interop.UpdateHtmlTable(Window.TableData, TableLocation);
            };
            sub.Items.Add(ci);

            ci = new MenuItem() {Header = "Align Center"};
            ci.Click += (s, e) =>
            {
                var text = Window.TableData.Headers[TableLocation.Column];
                text = ":" + text.Trim(':') + ":";
                Window.TableData.Headers[TableLocation.Column] = text;

                Window.Interop.UpdateHtmlTable(Window.TableData, TableLocation);
            };
            sub.Items.Add(ci);

            ci = new MenuItem() {Header = "Align Left"};
            ci.Click += (s, e) =>
            {
                var text = Window.TableData.Headers[TableLocation.Column];
                text = text.Trim(':');
                Window.TableData.Headers[TableLocation.Column] = text;

                Window.Interop.UpdateHtmlTable(Window.TableData, TableLocation);
            };
            sub.Items.Add(ci);
            cm.Items.Add(sub);


            ci = new MenuItem();
            ci.Header = "Insert Row Below";
            ci.Click += (o, ev) =>
            {
                List<string> row = Window.TableData.GetEmptyRow();

                TableLocation.Row++;
                if (TableLocation.IsHeader)
                {
                    TableLocation.Row = 0;
                    TableLocation.IsHeader = false;
                    Window.TableData.Rows.Insert(0, row );
                }
                else if (TableLocation.Row < Window.TableData.Rows.Count)
                    Window.TableData.Rows.Insert(TableLocation.Row, row );
                else
                    Window.TableData.Rows.Add(row);

                
                Window.Interop.UpdateHtmlTable(Window.TableData, TableLocation);
            };
            cm.Items.Add(ci);


            if (!TableLocation.IsHeader)
            {
                ci = new MenuItem();
                ci.Header = "Insert Row Above";
                ci.Click += (o, ev) =>
                {
                    List<string> row = Window.TableData.GetEmptyRow();
                    Window.TableData.Rows.Insert(TableLocation.Row, row);
                    Window.Interop.UpdateHtmlTable(Window.TableData, TableLocation);
                };
                cm.Items.Add(ci);
            }

            if (!TableLocation.IsHeader)
            {
                ci = new MenuItem();
                ci.Header = "Delete Row";
                ci.Click += (o, ev) =>
                {
                    Window.TableData.Rows.RemoveAt(TableLocation.Row);
                    Window.Interop.UpdateHtmlTable(Window.TableData, TableLocation);
                };
                cm.Items.Add(ci);
            }


            cm.Items.Add(new Separator());

            ci = new MenuItem();
            ci.Header = "Insert Column Right";
            ci.Click += (o, ev) => { };
            cm.Items.Add(ci);

            ci = new MenuItem();
            ci.Header = "Insert Column Left";
            ci.Click += (o, ev) => { };
            cm.Items.Add(ci);

            ci = new MenuItem();
            ci.Header = "Delete Column";
            ci.Click += (o, ev) => { };
            cm.Items.Add(ci);

            Show();
        }

    }
}
