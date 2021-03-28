using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MarkdownMonster.Utilities;
using MarkdownMonster.Windows.FileSearch;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for FileSearchControl.xaml
    /// </summary>
    public partial class FileSearchControl : UserControl
    {
        public FileSearchModel Model { get; set; }

        public FileSearchControl()
        {
            InitializeComponent();

            Model = new FileSearchModel();
            DataContext = Model;
            Loaded += FileSearchControl_Loaded;
        }

        private void FileSearchControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            // clear list
            Model.SearchResults = new System.Collections.ObjectModel.ObservableCollection<SearchFileResult>();
            await Model.SearchAsync();
        }

        private DebounceDispatcher debounce = new DebounceDispatcher();

        private void SearchPhrase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var origText = Model.SearchPhrase;

            debounce.Debounce(400, (p) =>
            {
                Search_Click(this, null);
            });
        }


        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var searchResult = border.DataContext as SearchFileResult;

            var tab = Model.Window.ActivateTab(searchResult.Filename, openIfNotFound: true, isPreview: true);
            if (tab == null)
                return;

            Model.Window.Dispatcher.InvokeAsync(() =>
            {
                var editor = tab.Tag as MarkdownDocumentEditor;
                if (string.IsNullOrEmpty(Model.ReplaceText))
                    editor?.AceEditor.OpenSearch(Model.SearchPhrase);
                else
                    editor?.AceEditor.OpenSearchAndReplace(Model.SearchPhrase, Model.ReplaceText);
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private void ButtonProjectOrDocumentPath_Click(object sender, RoutedEventArgs e)
        {
            string folder = Model.AppModel.ActiveProject?.ActiveFolder;
            if (string.IsNullOrEmpty(folder))
            {
                folder = Model.AppModel.ActiveDocument?.Filename;
                if (string.IsNullOrEmpty(folder))
                    return;
                folder = System.IO.Path.GetDirectoryName(folder);
            }

            Model.SearchFolder = folder;
        }

        private void ButtonSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            string folder = Model.SearchFolder;

            if (string.IsNullOrEmpty(folder))
            {
                folder = Model.AppModel.ActiveDocument?.Filename;
                if (string.IsNullOrEmpty(folder))
                    folder = System.IO.Path.GetDirectoryName(folder);
                else
                    folder = KnownFolders.GetPath(KnownFolder.Libraries);
            }

            folder = mmWindowsUtils.ShowFolderDialog(folder, "Select Search Folder");
            if (folder == null)
                return;

            Model.SearchFolder = folder;

        }

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                Search_Click(this, null);
            }

            if (e.Key == Key.Tab && this.Model.SearchResults.Count > 0)
            {
                ListResults.Focus();

            }
        }

        private void ButtonCloseSearch_Click(object sender, RoutedEventArgs e)
        {
            Model.Window.SidebarContainer.Items.Remove(Model.Window.SearchTab);
            Model.Window.SearchTab = null;

            Model.AppModel.Commands.OpenFolderBrowserCommand.Execute(null);
        }

    }
}
