using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MarkdownMonster.AddIns;
using MarkdownMonster.Windows;
using Microsoft.Win32;

namespace MarkdownMonster
{
    public class AppCommands
    {
        AppModel Model;

        public AppCommands(AppModel model)
        {
            Model = model;

            // File Operations
            OpenDocument();
            NewWeblogPost();


            // Configuration 
            OpenAddinManager();

            // Misc
            OpenSampleMarkdown();
            OpenRecentDocument();
            PreviewModes();

        }

        public CommandBase OpenDocumentCommand { get; set; }

        void OpenDocument()
        {
            // OPEN DOCUMENT COMMAND
            OpenDocumentCommand = new CommandBase((s, e) =>
            {
                var fd = new OpenFileDialog
                {
                    DefaultExt = ".md",
                    Filter = "Markdown files (*.md,*.markdown,*.mdcrypt)|*.md;*.markdown;*.mdcrypt|" +
                             "Html files (*.htm,*.html)|*.htm;*.html|" +
                             "Javascript files (*.js)|*.js|" +
                             "Typescript files (*.ts)|*.ts|" +
                             "Json files (*.json)|*.json|" +
                             "Css files (*.css)|*.css|" +
                             "Xml files (*.xml,*.config)|*.xml;*.config|" +
                             "C# files (*.cs)|*.cs|" +
                             "C# Razor files (*.cshtml)|*.cshtml|" +
                             "Foxpro files (*.prg)|*.prg|" +
                             "Powershell files (*.ps1)|*.ps1|" +
                             "Php files (*.php)|*.php|" +
                             "Python files (*.py)|*.py|" +
                             "All files (*.*)|*.*",
                    CheckFileExists = true,
                    RestoreDirectory = true,
                    Multiselect = true,
                    Title = "Open Markdown File"
                };

                if (!string.IsNullOrEmpty(mmApp.Configuration.LastFolder))
                    fd.InitialDirectory = mmApp.Configuration.LastFolder;

                bool? res = null;
                try
                {
                    res = fd.ShowDialog();
                }
                catch (Exception ex)
                {
                    mmApp.Log("Unable to open file.", ex);
                    MessageBox.Show(
                        $@"Unable to open file:\r\n\r\n" + ex.Message,
                        "An error occurred trying to open a file",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                if (res == null || !res.Value)
                    return;

                foreach (var file in fd.FileNames)
                {
                    // TODO: Check AddRecentFile and make sure Tab Selection works
                    Model.Window.OpenTab(file, rebindTabHeaders: true);
                    //Window.AddRecentFile(file);
                }
                
            });
        }



        public CommandBase NewWeblogPostCommand { get; set; }

        void NewWeblogPost()
        {
            NewWeblogPostCommand = new CommandBase((parameter, command) =>
            {
                
                AddinManager.Current.RaiseOnNotifyAddin("newweblogpost", null);
            });
        }





        public CommandBase AddinManagerCommand { get; set; }

        void OpenAddinManager()
        {
            AddinManagerCommand = new CommandBase((parameter, command) =>
            {
                var form = new AddinManagerWindow
                {
                    Owner = Model.Window
                };
                form.Show();
            });
        }


        public CommandBase OpenSampleMarkdownCommand { get; set; }

        void OpenSampleMarkdown()
        {
            OpenSampleMarkdownCommand = new CommandBase((parameter, command) =>
            {
                string tempFile = Path.Combine(Path.GetTempPath(), "SampleMarkdown.md");
                File.Copy(Path.Combine(Environment.CurrentDirectory, "SampleMarkdown.md"), tempFile, true);
                Model.Window.OpenTab(tempFile, rebindTabHeaders: true);
            });
        }


        public CommandBase OpenRecentDocumentCommand { get; set; }

        void OpenRecentDocument()
        {
            OpenRecentDocumentCommand = new CommandBase((parameter, command) =>
            {
                // hide to avoid weird fade behavior
                var context = Model.Window.Resources["ContextMenuRecentFiles"] as ContextMenu;
                if (context != null)
                    context.Visibility = Visibility.Hidden;

                WindowUtilities.DoEvents();

                var parm = parameter as string;
                if (parm == null)
                    return;

                Model.Window.OpenTab(parm, rebindTabHeaders: true);
                if (context != null)
                {
                    WindowUtilities.DoEvents();
                    context.Visibility = Visibility.Visible;
                }

            }, (p, c) => true);
        }


        public CommandBase PreviewModesCommand { get; set; }

        void PreviewModes()
        {
            PreviewModesCommand = new CommandBase((parameter, command) =>
            {
                string action = parameter as string;
                if (string.IsNullOrEmpty(action))
                    return;

                if (action == "ExternalPreviewWindow")
                    Model.Configuration.PreviewMode = MarkdownMonster.PreviewModes.ExternalPreviewWindow;
                else
                    Model.Configuration.PreviewMode = MarkdownMonster.PreviewModes.InternalPreview;

                Model.IsPreviewBrowserVisible = true;

                Model.Window.ShowPreviewBrowser();
            }, (p, c) => true);
        }


    }
}
