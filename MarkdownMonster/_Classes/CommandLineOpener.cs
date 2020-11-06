using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MarkdownMonster.Services;
using MarkdownMonster.Utilities;
using MarkdownMonster.Windows;
using Westwind.Utilities;

namespace MarkdownMonster._Classes
{
    /// <summary>
    /// This class handles opening files and processing a few non-CLI commands
    /// from the command line Arguments.
    /// </summary>
    public class CommandLineOpener
    {

        public CommandLineOpener(MainWindow window)
        {
            Window = window;
            Model = window.Model;
        }


        /// <summary>
        /// Window instance
        /// </summary>
        public MainWindow Window { get; }

        public AppModel Model {get; }


        
        /// <summary>
        /// Opens files from the command line or from an array of strings
        /// </summary>
        /// <param name="args">
        /// Array of file names and command line arguments.
        /// If null Command Line Args are used.
        /// </param>
        public void OpenFilesFromCommandLine(string[] args = null)
        {
            if (args == null)
            {
                // read fixed up command line args
                args = App.CommandArgs;

                if (args == null || args.Length == 0) // no args, only command line
                    return;
            }

            var autoSave = App.CommandArgs.Any(a => a.Equals("-autosave", StringComparison.InvariantCultureIgnoreCase));
            bool isFirstFile = true;
            TabItem tabToActivate = null;

            bool closeNextFile = false;
            foreach (var fileArgs in args)
            {
                var file = fileArgs;
                if (string.IsNullOrEmpty(file) )
                    continue;

                // handle file closing
                if (file == "-close")
                {
                    closeNextFile = true;
                    continue;
                }

                if (closeNextFile)
                {
                    closeNextFile = false;
                    var tab = Window.GetTabFromFilename(file);
                    if (tab != null)
                    {
                        if (tab.Tag is MarkdownDocumentEditor editor)
                        {
                            if (editor.IsDirty())
                                editor.SaveDocument();
                            Window.CloseTab(tab, dontPromptForSave: true);

                            if (Window.TabControl.Items.Count < 1)
                            {
                                WindowUtilities.DoEvents();
                                Window.Close();
                                return;
                            }
                        }
                    }

                    continue;
                }


                string ext = string.Empty;
                file = file.TrimEnd('\\');

                // file monikers - just strip first
                if (file.StartsWith("markdownmonster:webserver"))
                {
                    if (Window.WebServer == null)
                        WebServerLauncher.StartMarkdownMonsterWebServer();
                    else
                        WebServerLauncher.StopMarkdownMonsterWebServer();
                    
                    continue;
                }
                if (file.StartsWith("markdownmonster:"))
                    file = WebUtility.UrlDecode(file.Replace("markdownmonster:",String.Empty));
                else if (file.StartsWith("markdown:"))
                    file = WebUtility.UrlDecode(file.Replace("markdown:",String.Empty));
            
                bool isUntitled = file.Equals("untitled", StringComparison.OrdinalIgnoreCase);
                if (file.StartsWith("untitled."))
                    isUntitled = true;


                if (!isUntitled)
                {
                    if (FileUtils.HasInvalidPathCharacters(file))
                    {
                        Window.ShowStatusError($"Can't open file: {file}");
                        continue;
                    }

                    try
                    {
                        // FAIL: This fails at runtime not in debugger when value is .\ trimmed to . VERY WEIRD
                        file = Path.GetFullPath(file);
                        ext = Path.GetExtension(file);
                    }
                    catch(Exception ex)
                    {
                        Window.ShowStatusError($"Can't open file: {file}");
                        mmApp.Log($"CommandLine file path resolution failed: {file}", ex);
                    }
                }

                // open an empty doc or new doc with preset text from base64 (untitled.base64text)
                if (isUntitled)
                {
                    string docText = null;

                    // untitled.base64text will decode the base64 text
                    if (file.StartsWith("untitled."))
                    {
                        docText = CommandLineTextEncoder.ParseUntitledString(file);
                    }

                    // open empty document, or fill with App.StartupText is set
                    Window.Model.Commands.NewDocumentCommand.Execute(docText);
                }
              
                else if (File.Exists(file))
                {
                    // open file which may or may not open a tab (like a project)
                    var tab = Window.OpenFile(filename: file, batchOpen: true,
                                        noShellNavigation: true,
                                        initialLineNumber: MarkdownMonster.App.LineToOpen, noFocus: true);
                    App.LineToOpen = 0;
                    
                    //var tab = OpenTab(mdFile: file, batchOpen: true);
                    if (tab?.Tag is MarkdownDocumentEditor editor)
                    {
                        editor.MarkdownDocument.AutoSaveBackup = Model.Configuration.AutoSaveBackups;
                        editor.MarkdownDocument.AutoSaveDocument = autoSave || Model.Configuration.AutoSaveDocuments;
                    }

                    if (isFirstFile)
                    {
                        tabToActivate = tab;
                        isFirstFile = false;
                    }
                }
                else if (Directory.Exists(file))
                {
                    Window.ShowFolderBrowser(false, file);
                }
                // file is an .md file but doesn't exist but folder exists - create it
                else if ((ext.Equals(".md", StringComparison.InvariantCultureIgnoreCase) ||
                          ext.Equals(".mkdown", StringComparison.InvariantCultureIgnoreCase) ||
                          ext.Equals(".markdown", StringComparison.InvariantCultureIgnoreCase) ||
                          ext.Equals(".mdcrypt", StringComparison.InvariantCultureIgnoreCase)
                         ) &&
                         Directory.Exists(Path.GetDirectoryName(file)))
                {
                    File.WriteAllText(file, "");
                    var tab = Window.OpenTab(mdFile: file, batchOpen: true, noFocus:true);
                    if (tab?.Tag is MarkdownDocumentEditor editor)
                    {
                        editor.MarkdownDocument.AutoSaveBackup = Model.Configuration.AutoSaveBackups;
                        editor.MarkdownDocument.AutoSaveDocument = autoSave || Model.Configuration.AutoSaveDocuments;
                    }

                    // delete the file if we abort
                    Window.Dispatcher.Delay(1000, p => File.Delete(file), null);

                    if (isFirstFile)
                    {
                        tabToActivate = tab;
                        isFirstFile = false;
                    }
                }
                else
                {
                    file = Path.Combine(App.InitialStartDirectory, file);
                    file = Path.GetFullPath(file);
                    if (File.Exists(file))
                    {
                        var tab = Window.OpenTab(mdFile: file, batchOpen: true);
                        if (tab.Tag is MarkdownDocumentEditor editor)
                        {
                            editor.MarkdownDocument.AutoSaveBackup = Model.Configuration.AutoSaveBackups;
                            editor.MarkdownDocument.AutoSaveDocument =
                                autoSave || Model.Configuration.AutoSaveDocuments;
                        }
                    }
                    else if (Directory.Exists(file))
                        Window.ShowFolderBrowser(false, file);
                }


                if (tabToActivate != null)
                {
                    Window.Dispatcher.InvokeAsync(() =>
                    {
                        // Forces the Window to be active - note simple Activate()/Focus() doesn't
                        // work here due to cross thread activation when called from named pipe - even with Dispatcher
                        WindowUtilities.ActivateWindow(Window);
                        Window.ActivateTab(tabToActivate, true);
                    }, DispatcherPriority.ApplicationIdle);
                }
            }

        }
    }
}
