using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FontAwesome.WPF;
using MarkdownMonster.Utilities;

namespace MarkdownMonster.Controls.ContextMenus
{
    public class RecentDocumentsContextMenu
    {
        private MainWindow Window { get; }

        public RecentDocumentsContextMenu(MainWindow window)
        {
            Window = window;
        }


        /// <summary>
        /// Creates/Updates the Recent Items Context list
        /// from recent file and recent folder configuration
        /// </summary>
        public void UpdateRecentDocumentsContextMenu(RecentFileDropdownModes mode)
        {
            var contextMenu = new ContextMenu { FontSize = 12.5, Padding = new Thickness(8) };

            if (mode == RecentFileDropdownModes.MenuDropDown)
                Window.ButtonRecentFiles.Items.Clear();
            else if (mode == RecentFileDropdownModes.ToolbarDropdown)
                Window.ToolbarButtonRecentFiles.ContextMenu = contextMenu;

            var icon = new AssociatedIcons();
            MenuItem mi = null;
            var lowlightColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aaa"));


            if (true)
            {
                var content = new StackPanel
                {
                    Orientation = Orientation.Vertical
                };

                // image/textblock panel
                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                panel.Children.Add(new Image
                {
                    Source = ImageAwesome.CreateImageSource(FontAwesomeIcon.Star, Brushes.Goldenrod, 17),
                    Height = 16
                });
                panel.Children.Add(new TextBlock
                {
                    Text = "Favorites...",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(5, 2, 0, 0)
                });
                content.Children.Add(panel);

                mi = new MenuItem() { Header = content, Padding = new Thickness(0, 2, 0, 3) };
                mi.Click += (o, args) => Window.OpenFavorites();
                contextMenu.Items.Add(mi);
                contextMenu.Items.Add(new Separator());
            }


            mmApp.Configuration.CleanupRecentFilesAndFolders();

            foreach (string file in mmApp.Configuration.RecentDocuments)
            {
                var fileOnly = Path.GetFileName(file).Replace("_", "__");
                var path = Path.GetDirectoryName(file).Replace("_", "__");

                var content = new StackPanel
                {
                    Orientation = Orientation.Vertical
                };

                // image/textblock panel
                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                panel.Children.Add(new Image
                {
                    Source = icon.GetIconFromFile(file),
                    Height = 14
                });
                panel.Children.Add(new TextBlock
                {
                    Text = fileOnly,
                    FontWeight = FontWeights.Medium,
                    Margin = new Thickness(5, 0, 0, 0)
                });
                content.Children.Add(panel);

                // folder
                content.Children.Add(new TextBlock
                {
                    Text = path,
                    FontStyle = FontStyles.Italic,
                    FontSize = 10.25,
                    //Margin = new Thickness(0, 2, 0, 0),
                    Foreground = lowlightColor
                });


                mi = new MenuItem
                {
                    Header = content,
                    Command = Window.Model.Commands.OpenRecentDocumentCommand,
                    CommandParameter = file,
                    Padding = new Thickness(0, 2, 0, 3)
                };

                if (mode == RecentFileDropdownModes.ToolbarDropdown)
                    contextMenu.Items.Add(mi);
                else
                    Window.ButtonRecentFiles.Items.Add(mi);
            }


            if (mmApp.Configuration.FolderBrowser.RecentFolders.Count > 0)
            {

                mi = new MenuItem
                {
                    IsEnabled = false,
                    Header = "————————— Recent Folders —————————"
                };

                if (mode == RecentFileDropdownModes.ToolbarDropdown)
                    contextMenu.Items.Add(mi);
                else
                    Window.ButtonRecentFiles.Items.Add(mi);

                foreach (var folder in mmApp.Configuration.FolderBrowser.RecentFolders.Take(7))
                {
                    var pathOnly = Path.GetFileName(folder).Replace("_", "__");
                    var path = folder.Replace("_", "__");

                    var content = new StackPanel()
                    {
                        Orientation = Orientation.Vertical
                    };

                    // image/textblock panel
                    var panel = new StackPanel { Orientation = Orientation.Horizontal };
                    panel.Children.Add(new Image
                    {
                        Source = icon.GetIconFromFile("folder.folder"),
                        Height = 14
                    });
                    panel.Children.Add(new TextBlock
                    {
                        Text = pathOnly,
                        FontWeight = FontWeights.Medium,
                        Margin = new Thickness(5, 0, 0, 0)
                    });
                    content.Children.Add(panel);

                    content.Children.Add(new TextBlock
                    {
                        Text = path,
                        FontStyle = FontStyles.Italic,
                        FontSize = 10.25,
                        Margin = new Thickness(0, 2, 0, 0),
                        Opacity = 0.8
                    });

                    mi = new MenuItem()
                    {
                        Header = content,
                        Command = Window.Model.Commands.OpenRecentDocumentCommand,
                        CommandParameter = folder,
                        Padding = new Thickness(0, 2, 0, 3)
                    };

                    if (mode == RecentFileDropdownModes.ToolbarDropdown)
                        contextMenu.Items.Add(mi);
                    else
                        Window.ButtonRecentFiles.Items.Add(mi);
                }
            }

        }
    }
}
