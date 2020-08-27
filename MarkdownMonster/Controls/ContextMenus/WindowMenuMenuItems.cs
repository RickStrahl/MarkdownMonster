using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MarkdownMonster.Controls.ContextMenus
{
    public class WindowMenuContextMenu
    {
        private bool _firstWindowMenu = true;

        public void CreateWindowMenuContextMenu()
        {
            var mi = mmApp.Model.Window.MainMenuWindow;
            if (_firstWindowMenu)
            {
                // force the Window closed
                _firstWindowMenu = false;
                mi.ContextMenuClosing += (m, e) => { mi.Visibility = Visibility.Collapsed; };
            }

            var wmMenuContextMenu = new WindowMenuContextMenu();
            wmMenuContextMenu.CreateItems(mi.Items);

            mi.IsSubmenuOpen = true;
            mi.Focus();
        }

        public void CreateItems(ItemCollection items)
        {
            if (items == null)
                return;

            var model = mmApp.Model;
            items.Clear();


            items.Add(new MenuItem
            {
                Header = "_Close Document", Command = model.Commands.CloseActiveDocumentCommand
            });

            items.Add(new MenuItem
            {
                Header = "C_lose All Documents",
                Command = model.Commands.CloseAllDocumentsCommand,
                CommandParameter = "All"
            });

            items.Add(new MenuItem
            {
                Header = "Close _Other Documents",
                Command = model.Commands.CloseAllDocumentsCommand,
                CommandParameter = "AllBut"
            });

            items.Add(new MenuItem
            {
                Header = "Close Documents To _Right ",
                Command = model.Commands.CloseDocumentsToRightCommand,
            });

            items.Add(new Separator());

            items.Add(new MenuItem
            {
                Header = "Open Document in _New Window", Command = model.Commands.OpenInNewWindowCommand,
            });

            items.Add(new MenuItem
            {
                Header = "Show Document in _Folder Browser",
                Command = model.Commands.OpenFolderBrowserCommand,
                InputGestureText = "Alt-W-F"
            });



            var menuItems = model.Window.GenerateContextMenuItemsFromOpenTabs();
            if (menuItems.Count < 1)
                return;

            items.Add(new Separator());
            foreach (var menu in menuItems)
            {
                items.Add(menu);
            }

            return;
        }
    }
}
