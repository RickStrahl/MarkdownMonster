using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MarkdownMonster.Windows;

namespace MarkdownMonster.Utilities
{

    /// <summary>
    /// File watcher used on the open editor document. Monitors the
    /// file for changes and if a change occurs either updates the
    /// document (if not dirty) or does nothing and defers to
    /// editor.SaveDocument() to compare changes.
    /// </summary>
    public class DocumentFileWatcher
    {
        private static FileSystemWatcher FileWatcher;

        private static readonly DebounceDispatcher debounce = new DebounceDispatcher();

        /// <summary>
        /// Attach an Editor instance to watch the related file
        /// </summary>
        /// <param name="editor">Editor instance on which to watch the file for changes</param>
        public static void AttachFilewatcher(MarkdownDocumentEditor editor)
        {
            var file = editor?.MarkdownDocument?.Filename;
            if (string.IsNullOrEmpty(file) ||
                file.Equals("untitled",StringComparison.InvariantCultureIgnoreCase) ||
                !File.Exists(file))
                return;

            var folder = Path.GetDirectoryName(file);
            var filename  = Path.GetFileName(file);
            try
            { 
                if (FileWatcher == null)
                {
                    FileWatcher = new FileSystemWatcher();
                    FileWatcher.Path = folder;
                    FileWatcher.Filter = filename;

                    FileWatcher.NotifyFilter = NotifyFilters.LastWrite;
                    FileWatcher.EnableRaisingEvents = true;
                    FileWatcher.Changed += FileWatcher_Changed;
                }
                else
                {
                    FileWatcher.Path = folder;
                    FileWatcher.Filter = filename;
                }
            }
            catch (Exception ex)
            {
                mmApp.Log($"Couldn't attach File Watcher to active document: {file}", ex, logLevel: LogLevels.Error);
            }
        }


        /// <summary>
        /// If the file open in the active tab has changed try to
        /// either update the document if not dirty, otherwise
        /// do nothing and wait for save operation to trigger
        /// comparison or choosing which file to pick (in SaveDocument())
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            debounce.Debounce(200,(objPath) =>
            {
                string fullPath = objPath as string;

                // Get the current editor and document
                var tab = mmApp.Model.Window.TabControl.SelectedItem as TabItem;
                var editor = tab?.Tag as MarkdownDocumentEditor;
                var document = editor?.MarkdownDocument;
                
                bool isActiveTabFile = document != null && document.Filename.Equals(fullPath, StringComparison.InvariantCultureIgnoreCase);

                if (!isActiveTabFile)
                {
                    tab = mmApp.Model.Window.GetTabFromFilename(fullPath);
                    editor = tab?.Tag as MarkdownDocumentEditor;
                    document = editor?.MarkdownDocument;
                }

                if (document == null)
                    return;


                // Only work changes on the active tab
                // if the file was saved in the last second - don't update because
                // the change event most likely is from the save op
                // Note: this can include auto-save operations
                if (document.LastSaveTime > DateTime.UtcNow.AddMilliseconds(-1000) ||
                    document.IsDirty)
                    return;
              
                // Otherwise reload from disk and refresh editor and preview

                // load the document 
                if (!document.Load(document.Filename))
                    return;

                try
                {
                    int scroll = editor.GetScrollPosition();
                    editor.SetMarkdown(document.CurrentText, keepUndoBuffer: true);
                    if (isActiveTabFile)
                        editor.AceEditor?.UpdateDocumentStats();
                    if (scroll > -1)
                        editor.SetScrollPosition(scroll);
                }
                catch { }

                if(isActiveTabFile &&
                   document.ToString() != MarkdownDocument.PREVIEW_HTML_FILENAME)
                    mmApp.Model.Window.PreviewBrowser?.PreviewMarkdown(editor, keepScrollPosition: true);

            },e.FullPath, DispatcherPriority.Normal, mmApp.Model.Window.Dispatcher);
        }


        /// <summary>
        /// Checks to see if a file has changed and updates the document in the editor
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
                    // if the file is not dirty - reload. Otherwise it won't be notified until
                    // you try and save the document.
                    if (!doc.IsDirty)
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
