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
    /// <summary>
    /// Class that handles display and execution of the Tab Context Menu
    /// </summary>
    public class TabContextMenu
    {
        private ContextMenu ContextMenu;
        private AppModel Model;
        private bool firstAccess;

        public TabContextMenu()
        {
            Model = mmApp.Model;
            if (firstAccess)
            {
                ContextMenu.Closed += ContextMenu_Closed;
                firstAccess = false;
            }

            ClearMenu();
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

        public void ShowContextMenu()
        {
            ClearMenu();
            AddContextMenuItems();
            Show();
        }

        public void AddContextMenuItems(ContextMenu contextMenu = null)
        {
            if (contextMenu == null)
                contextMenu = ContextMenu;

            var model = Model;

            var mi = new MenuItem
            {
                Header = "_Close Document",
                Command = Model.Commands.CloseActiveDocumentCommand
            };
            contextMenu.Items.Add(mi);

            mi = new MenuItem
            {
                Header = "Close All Documents",
                Name= "MenuCloseAllTabs",
                Command = Model.Commands.CloseAllDocumentsCommand
            };
            contextMenu.Items.Add(mi);

            mi = new MenuItem
            {
                Header = "Close Other Documents",
                Name = "MenuCloseAllButThis",
                Command = Model.Commands.CloseAllDocumentsCommand,
                CommandParameter="AllBut"
            };
            contextMenu.Items.Add(mi);

            contextMenu.Items.Add(new Separator());


            mi = new MenuItem
            {
                Header = "Add to Favorites",
                Name = "AddFavorite",
                Command = Model.Commands.AddFavoriteCommand,
                CommandParameter = model.ActiveDocument?.Filename
            };
            contextMenu.Items.Add(mi);

            contextMenu.Items.Add(new Separator());

            // TERMINAL AND FOLDER BROWSING

            mi = new MenuItem
            {
                Header = "Open _Terminal",
                Name = "ContextOpenInCommandWindow",
                Command = Model.Commands.CommandWindowCommand,
            };
            contextMenu.Items.Add(mi);

            mi = new MenuItem
            {
                Header = "Open in Explorer",
                Name = "ContextOpenInFolder",
                Command = Model.Commands.OpenInExplorerCommand,
            };
            contextMenu.Items.Add(mi);

            mi = new MenuItem
            {
                Header = "Show in Folder _Browser",
                Name = "OpenInFolderBrowser",
                Command = Model.Commands.OpenFolderBrowserCommand,
            };
            contextMenu.Items.Add(mi);

            contextMenu.Items.Add(new Separator());

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
                contextMenu.Items.Add(mi);

                mi = new MenuItem
                {
                    Header = "Open in Git Client",
                    Name = "ContextOpenGitClient",
                    Command = Model.Commands.OpenGitClientCommand,
                };
                contextMenu.Items.Add(mi);

                if (gitRemoteUrl != null && gitRemoteUrl.Contains("github.com"))
                {
                    mi = new MenuItem
                    {
                        Header = "Open on Git_Hub",
                        Name = "ContextOpenOnGithub",
                        Command = Model.Commands.OpenOnGithubCommand,
                        CommandParameter=Model.ActiveTabFilename
                    };
                    contextMenu.Items.Add(mi);
                }

                contextMenu.Items.Add(new Separator());
            }

            // FOLDER PATH

            mi = new MenuItem
            {
                Header = "Copy File Path to Clipboard",
                Name = "ContextCopyFolderName",
                Command = Model.Commands.CopyFullPathToClipboardCommand
            };
            contextMenu.Items.Add(mi);
        }


    }
}
