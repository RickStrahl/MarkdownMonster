using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Markdig.Parsers;
using MarkdownMonster.Utilities;
using MarkdownMonster.Windows;
using MarkdownMonster.Windows.DocumentOutlineSidebar;
using Westwind.Utilities;

namespace MarkdownMonster.Controls.ContextMenus
{

      //<!--<ContextMenu x:Key="TabItemContextMenu" Name="TabItemContextMenu">
      //      <MenuItem Header="_Close Document" Command="{Binding DataContext.Commands.CloseActiveDocumentCommand}" />
      //      <MenuItem Name="MenuCloseAllTabs" Header="Close All Documents" Command="{Binding DataContext.Commands.CloseAllDocumentsCommand}"/>
      //      <MenuItem Name="MenuCloseAllButThisTab" Header="Close All But This Document"  Command="{Binding DataContext.Commands.CloseAllDocumentsCommand}" CommandParameter="AllBut" />
      //      <Separator/>
      //      <MenuItem Name="MenuAddFavoriteTab" Header="Add to Favorites"  Command="{Binding DataContext.Commands.AddFavoriteCommand}" CommandParameter="{Binding DataContext.ActiveDocument.Filename}" />
      //      <Separator />
      //      <MenuItem Name="ContextOpenInCommandWindow" Header="Open _Terminal" Command="{Binding DataContext.Commands.CommandWindowCommand}"  />
      //      <MenuItem Name="ContextOpenInFolder" Header="Show in Explorer" Command="{Binding DataContext.Commands.OpenInExplorerCommand}"  />
      //      <MenuItem Name="ContextOpenInFolderBrowser" Header="Show in Folder _Browser" Command="{Binding DataContext.Commands.OpenFolderBrowserCommand}" />
      //      <Separator/>
      //      <MenuItem Name="ContextCommitToGit" Header="Commit to _Git..."
      //                Command="{Binding DataContext.Commands.CommitToGitCommand}" />
      //      <MenuItem Name="ContextOpenGitClient" Header="Open in Git Client"
      //                Command="{Binding DataContext.Commands.OpenGitClientCommand}"   
      //                IsEnabled="{Binding DataContext.Configuration.GitClientExecutable, Converter={StaticResource NotEmptyStringToBooleanConverter}}" />
      //      <MenuItem Name="ContextOpenOnGithub" Header="Open on Github" 
      //                IsEnabled="{Binding DataContext.Configuration.GitClientExecutable, Converter={StaticResource NotEmptyStringToBooleanConverter}}" />
      //                />
      //      <Separator/>
      //      <MenuItem Name="ContextCopyFoldername" Header="Copy Full Path" Command="{Binding DataContext.Commands.CopyFullPathToClipboardCommand}" />
      //  </ContextMenu>-->


    /// <summary>
    /// Class that handles display and execution of the editors
    /// context menu.
    /// </summary>
    public class TabContextMenu
    {
        private ContextMenu ContextMenu;
        private AppModel Model;

        public TabContextMenu()
        {
            Model = mmApp.Model;

            if (Model.Window.TabControl.ContextMenu == null)
                Model.Window.TabControl.ContextMenu = new ContextMenu();
            ContextMenu = Model.Window.TabControl.ContextMenu;
            ContextMenu.Closed += ContextMenu_Closed;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            Model.ActiveEditor?.SetEditorFocus();
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
                Model.Window.TabControl.ContextMenu = ContextMenu;
            }
            else
                ContextMenu?.Items.Clear();
        }

        public void Show()
        {
            if (ContextMenu != null)
            {
                Model.ActiveEditor?.SetMarkdownMonsterWindowFocus();

                ContextMenu.PlacementTarget = Model.Window.TabControl;
                ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                ContextMenu.VerticalOffset = 8;

                ContextMenu.Focus();
                ContextMenu.IsOpen = true;

                var item = ContextMenu.Items[0] as MenuItem;
                item.Focus();
            }         
        }

        public void ShowContextMenuAll()
        {
            ClearMenu();
            var model = Model;

            // CLOSE DOCUMENTS

            var mi = new MenuItem
            {
                Header = "_Close Document",
                Command = Model.Commands.CloseActiveDocumentCommand
            };            
            ContextMenu.Items.Add(mi);

            mi = new MenuItem
            {
                Header = "Close All Documents",
                Name= "MenuCloseAllTabs",
                Command = Model.Commands.CloseAllDocumentsCommand
            };
            ContextMenu.Items.Add(mi);

            mi = new MenuItem
            {
                Header = "Close Other Documents",
                Name = "MenuCloseAllButThis",
                Command = Model.Commands.CloseAllDocumentsCommand,
                CommandParameter="AllBut"
            };
            ContextMenu.Items.Add(mi);

            ContextMenu.Items.Add(new Separator());

            // TERMINAL AND FOLDER BROWSING

            mi = new MenuItem
            {
                Header = "Open _Terminal",
                Name = "ContextOpenInCommandWindow",
                Command = Model.Commands.CommandWindowCommand,                
            };
            ContextMenu.Items.Add(mi);

            mi = new MenuItem
            {
                Header = "Open in Explorer",
                Name = "ContextOpenInFolder",
                Command = Model.Commands.OpenInExplorerCommand,                
            };
            ContextMenu.Items.Add(mi);

            mi = new MenuItem
            {
                Header = "Show in Folder _Browser",
                Name = "OpenInFolderBrowser",
                Command = Model.Commands.OpenFolderBrowserCommand,                
            };
            ContextMenu.Items.Add(mi);

            ContextMenu.Items.Add(new Separator());

            // GIT OPERATIONS
            bool showGitOperations = false;
            string gitRemoteUrl = null;
            if (Model.ActiveEditor?.MarkdownDocument != null)
            {
                var git = new GitHelper();
                using (var repo = git.OpenRepository(Model.ActiveDocument.Filename))
                {
                    showGitOperations = repo != null;
                    if (showGitOperations)
                        gitRemoteUrl = git.GetActiveRemoteUrl();
                }               
            }

            if (showGitOperations)
            {
                mi = new MenuItem
                {
                    Header = "Commit to _Git",
                    Name = "ContextContextCommitToGit",
                    Command = Model.Commands.CommitToGitCommand,
                };
                ContextMenu.Items.Add(mi);

                mi = new MenuItem
                {
                    Header = "Open in Git Client",
                    Name = "ContextOpenGitClient",
                    Command = Model.Commands.OpenGitClientCommand,
                };
                ContextMenu.Items.Add(mi);

                if (gitRemoteUrl != null && gitRemoteUrl.Contains("github.com"))
                {
                    mi = new MenuItem
                    {
                        Header = "Open on Git_Hub",
                        Name = "ContextOpenOnGithub",
                        Command = Model.Commands.OpenOnGithubCommand,
                        CommandParameter=Model.ActiveDocument?.Filename
                    };
                    ContextMenu.Items.Add(mi);
                }

                ContextMenu.Items.Add(new Separator());
            }


            // FOLDER PATH

            mi = new MenuItem
            {
                Header = "Copy File Path to Clipboard",
                Name = "ContextCopyFolderName",
                Command = Model.Commands.CopyFullPathToClipboardCommand
            };
            ContextMenu.Items.Add(mi);

            Show();
        }


    }
}
