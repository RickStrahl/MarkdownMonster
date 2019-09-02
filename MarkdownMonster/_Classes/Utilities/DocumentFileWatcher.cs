using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FontAwesome.WPF;
using MarkdownMonster.Windows;
using Westwind.Utilities;

namespace MarkdownMonster.Utilities
{
    public class DocumentFileWatcher
    {
        private static FileSystemWatcher FileWatcher;
        private static readonly DebounceDispatcher debounce = new DebounceDispatcher();

        public static void AttachFilewatcher(MarkdownDocumentEditor document)
        {
            var file = document?.MarkdownDocument?.Filename;
            if (file == null || !File.Exists(file))
                return;

            if (FileWatcher == null)
            {
                FileWatcher = new FileSystemWatcher();
                FileWatcher.Path = Path.GetDirectoryName(file);
                FileWatcher.Filter = Path.GetFileName(file);
                FileWatcher.EnableRaisingEvents = true;
                FileWatcher.Changed += FileWatcher_Changed;
                
            }
            else
            {
                FileWatcher.Path = Path.GetDirectoryName(file);
                FileWatcher.Filter = Path.GetFileName(file);
            }
        }

        
        /// <summary>
        /// If the file open in the active tab has changed try to
        /// either update the document if not dirty, otherwise
        /// do nothing and wait for OnActivate to trigger a comparison.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            
            debounce.Debounce(220,(p) =>
            {
                //Debug.WriteLine("FileWatcher: " + e.FullPath);

                var tab = mmApp.Model.Window.TabControl.SelectedItem as TabItem;
                if (tab == null)
                    return;

                var editor = tab.Tag as MarkdownDocumentEditor;
                if (tab.Tag == null)
                    return;

                var document = editor.MarkdownDocument;
                if (document == null)
                    return;
                
                // if the file was saved in the last second - don't update because
                // the change event most likely is from the save op
                // Note: this can include auto-save operations
                if (document.LastSaveTime > DateTime.UtcNow.AddMilliseconds(-1000) ||
                    document.IsDirty ||
                    !document.Filename.Equals(e.FullPath,StringComparison.InvariantCultureIgnoreCase))
                    return;
                
                // load the document 
                if (!document.Load(document.Filename))
                    return;

                //Debug.WriteLine("FileWatcher Updating Actuall: " + e.FullPath);

                // if the file was saved in the last second - don't update because
                // the change event most likely is from the save op
                // Note: this can include auto-save operations
                if (document.LastSaveTime > DateTime.UtcNow.AddMilliseconds(-1000)) 
                    return;
                try
                {
                    int scroll = editor.GetScrollPosition();
                    editor.SetMarkdown(document.CurrentText);
                    editor.AceEditor?.UpdateDocumentStats();

                    if (scroll > -1)
                        editor.SetScrollPosition(scroll);
                }
                catch { }

                mmApp.Model.Window.PreviewBrowser?.PreviewMarkdown(editor, keepScrollPosition: true);

                //Debug.WriteLine("FileWatcher Updating DONE: " + e.FullPath);

            },null, DispatcherPriority.Normal, mmApp.Model.Window.Dispatcher);
        }


        /// <summary>
        /// Checks to see if 
        /// </summary>
        /// <param name="noPrompt"></param>
        public static void CheckFileChangeInOpenDocuments(bool noPrompt = false)
        {
            var window = mmApp.Model.Window;

            var selectedTab = window.TabControl.SelectedItem as TabItem;

            // check for external file changes
            for (int i = window.TabControl.Items.Count - 1; i > -1; i--)
            {
                var tab = window.TabControl.Items[i] as TabItem;

                if (tab == null)
                    continue;

                var editor = tab.Tag as MarkdownDocumentEditor;
                var doc = editor?.MarkdownDocument;
                if (doc == null)
                    continue;

                if (doc.HasFileCrcChanged())
                {
                    // force update to what's on disk so it doesn't fire again
                    // do here prior to dialogs so this code doesn't fire recursively
                    doc.UpdateCrc();

                    string filename = doc.FilenamePathWithIndicator.Replace("*", "");
                    string template = filename +
                                      Environment.NewLine + Environment.NewLine +
                                      "This file has been modified by another program." + Environment.NewLine +
                                      "Do you want to reload it?";

                    if (!doc.IsDirty || MessageBox.Show(window, template,
                            "Reload",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        if (!doc.Load(doc.Filename))
                        {
                            MessageBox.Show(window, "Unable to re-load current document.",
                                "Error re-loading file",
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            continue;
                        }

                        try
                        {
                            int scroll = editor.GetScrollPosition();
                            editor.SetMarkdown(doc.CurrentText);
                            editor.AceEditor?.UpdateDocumentStats();

                            if (scroll > -1)
                                editor.SetScrollPosition(scroll);
                        }
                        catch (Exception ex)
                        {
                            mmApp.Log("Changed File Notification failed.", ex);
                            MessageBox.Show(window, "Unable to re-load current document.",
                                "Error re-loading file",
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }

                        if (tab == selectedTab)
                            window.PreviewBrowser?.PreviewMarkdown(editor, keepScrollPosition: true);
                    }
                }
            }
        }
    }
}
