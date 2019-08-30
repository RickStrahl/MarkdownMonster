using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using MarkdownMonster.Windows;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;

namespace MarkdownMonster.Controls.ContextMenus
{
    public class WindowSizesContextMenu
    {
        private MainWindow Window { get; }

        public WindowSizesContextMenu(MainWindow window)
        {
            Window = window;
        }

        public void OpenContextMenu()
        {
            var ctx = new ContextMenu();
            ctx.Items.Add(
                new MenuItem {Header = "Select Window Size", IsEnabled = false, Foreground = Brushes.DarkGray});
            ctx.Items.Add(new Separator());

            foreach (string size in Window.Model.Configuration.WindowPosition.WindowSizes)
            {
                var menuItem = new MenuItem() {Header = size};
                menuItem.Click += ButtonWindowResize_Click;
                ctx.Items.Add(menuItem);
            }

            ctx.IsOpen = true;
            ctx.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            ctx.VerticalOffset = 8;
            ctx.HorizontalOffset = 10;
            WindowUtilities.DoEvents();
        }

        private void ButtonWindowResize_Click(object sender, RoutedEventArgs e)
        {
            var model = Window.Model;

            var size = ((MenuItem) sender).Header as string;
            var tokens = size.Split('x');
            Window.Width = double.Parse(tokens[0].Trim());
            Window.Height = double.Parse(tokens[1].Trim());

            if (model.ActiveEditor?.EditorPreviewPane != null)
            {

                var width = model.ActiveEditor.EditorPreviewPane.ActualWidth;

                if (width < 2000 && width > 800)
                {
                    model.ActiveEditor.EditorPreviewPane.EditorWebBrowserEditorColumn.Width = GridLengthHelper.Star;
                    if (model.Configuration.IsPreviewVisible)
                        model.ActiveEditor.EditorPreviewPane.EditorWebBrowserPreviewColumn.Width =
                            new GridLength(width * 0.45);
                }
                else if (width > 2000)
                {
                    Window.ShowLeftSidebar();
                    if (model.ActiveEditor.EditorPreviewPane.EditorWebBrowserPreviewColumn.Width.Value < 750 &&
                        model.Configuration.IsPreviewVisible)
                        model.ActiveEditor.EditorPreviewPane.EditorWebBrowserPreviewColumn.Width = new GridLength(750);
                }
                else if (width <= 900)
                {
                    Window.ShowLeftSidebar(hide: true);
                    WindowUtilities.DoEvents();

                    width = model.ActiveEditor.EditorPreviewPane.ActualWidth;
                    model.ActiveEditor.EditorPreviewPane.EditorWebBrowserEditorColumn.Width = GridLengthHelper.Star;
                    if (model.Configuration.IsPreviewVisible)
                        model.ActiveEditor.EditorPreviewPane.EditorWebBrowserPreviewColumn.Width =
                            new GridLength(width * 0.45);
                }
            }

            var screen = Screen.FromHandle(Window.Hwnd);
            var ratio = (double) WindowUtilities.GetDpiRatio(Window.Hwnd);
            var windowWidth = screen.Bounds.Width * ratio;
            var windowHeight = screen.Bounds.Height * ratio;

            if (windowWidth < Window.Width || windowHeight < Window.Height)
            {
                Window.Top = screen.Bounds.Y * ratio;
                Window.Left = screen.Bounds.X * ratio;
                Window.Width = windowWidth - 20;
                Window.Height = windowHeight - 40;
            }

            if (windowWidth < Window.Width + Window.Left ||
                windowHeight < Window.Height + Window.Top)
                WindowUtilities.CenterWindow(Window);
        }
    }
}
