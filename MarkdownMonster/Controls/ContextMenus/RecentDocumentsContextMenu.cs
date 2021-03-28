using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            var contextMenu = new ContextMenu { FontSize = 13, Padding = new Thickness(0,8,8,8),  };

            if (mode == RecentFileDropdownModes.MenuDropDown)
                Window.ButtonRecentFiles.Items.Clear();
            else if (mode == RecentFileDropdownModes.ToolbarDropdown)
                Window.ToolbarButtonRecentFiles.ContextMenu = contextMenu;

            var icon = new AssociatedIcons();
            MenuItem mi = null;
            var lowlightColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aaa"));


            var headerContent = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            // image/textblock panel
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            headerPanel.Children.Add(new Image
            {
                Source = ImageAwesome.CreateImageSource(FontAwesomeIcon.Star, Brushes.Goldenrod, 17),
                Height = 16
            });
            headerPanel.Children.Add(new TextBlock
            {
                Text = "Favorites...",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(5, 2, 0, 0)
            });
            headerContent.Children.Add(headerPanel);

            mi = new MenuItem() { Header = headerContent, Padding = new Thickness(0, 2, 0, 3) };
            mi.Click += (o, args) => Window.OpenFavorites();
            contextMenu.Items.Add(mi);

            contextMenu.Items.Add(new Separator());



            mmApp.Configuration.CleanupRecentFilesAndFolders();

            foreach (string file in mmApp.Configuration.RecentDocuments)
            {
                var fileOnly = Path.GetFileName(file).Replace("_", "__");
                var path = Path.GetDirectoryName(file).Replace("_", "__");

                var content = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0, 2, 0, 2)
                };

                // image/textblock panel
                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                panel.Children.Add(new Image
                {
                    Source = icon.GetIconFromFile(file),
                    Height = 15
                });
                panel.Children.Add(new TextBlock
                {
                    Text = fileOnly,
                    FontWeight = FontWeights.Medium,
                    Margin = new Thickness(5, 0, 0, 0)
                });
                content.Children.Add(panel);

                var sp = new StackPanel {Orientation = Orientation.Horizontal};

                // folder
                sp.Children.Add(new TextBlock
                {
                    Text = path,
                    FontStyle = FontStyles.Italic,
                    FontSize = 10.35,
                    Margin = new Thickness(19, 0, 0, 0),
                    Foreground = Brushes.SteelBlue
                });

                var button = new Button
                {
                    Height=10.35,
                    FontSize = 10.35,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0),
                    Margin = new Thickness( 0 , 0, 0,   0),
                    Background = Brushes.Transparent,
                    Style =  Application.Current.TryFindResource(ToolBar.ButtonStyleKey) as Style
                };
                var folderButton = new FontAwesome.WPF.FontAwesome
                {
                    Icon = FontAwesomeIcon.FolderOpen,
                    FontSize = 11,
                    Margin = new Thickness(4, 1, 0, 0),
                    Padding = new Thickness(0),
                    Foreground = Brushes.DarkGoldenrod,
                    ToolTip = "Open folder in Folder Browser",
                };
                button.Click += (s, e) =>
                {
                    Window.Model.Commands.OpenFolderBrowserCommand.Execute(path);
                    e.Handled = true;
                    Window.Dispatcher.InvokeAsync(() => contextMenu.IsOpen = false,
                        System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                };
                button.Content = folderButton;
                sp.Children.Add(button);

                content.Children.Add(sp);

                mi = new MenuItem
                {
                    Header = content,
                    Command = Window.Model.Commands.OpenRecentDocumentCommand,
                    CommandParameter = file,
                    Padding = new Thickness(0, 1, 0, 3)
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
                    Header = "—————————  Recent Folders  —————————"
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
                        Orientation = Orientation.Vertical,
                        Margin = new Thickness(0, 2, 0, 2)
                    };

                    // image/textblock panel
                    var panel = new StackPanel { Orientation = Orientation.Horizontal };
                    panel.Children.Add(new Image
                    {
                        Source = icon.GetIconFromFile("folder.folder"),
                        Height = 15
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
                        FontSize = 10.35,
                        Margin = new Thickness(19, 1, 0, 0),
                        Foreground = Brushes.SteelBlue
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
