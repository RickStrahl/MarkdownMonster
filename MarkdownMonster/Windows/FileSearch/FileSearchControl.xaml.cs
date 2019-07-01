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

            Loaded += FileSearchControl_Loaded;
            

        }

        private void FileSearchControl_Loaded(object sender, RoutedEventArgs e)
        {

            Model = new FileSearchModel();
            DataContext = Model;
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            await Model.SearchAsync();
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var searchResult = border.DataContext as SearchFileResult;

            var tab = Model.Window.ActivateTab(searchResult.Filename, openIfNotFound: true);
            if (tab == null)
                return;

            Model.Window.Dispatcher.InvokeAsync(() =>
            {
                var editor = tab.Tag as MarkdownDocumentEditor;
                // TODO: Fire Search And Replace Logic

                editor?.AceEditor.OpenSearch(Model.SearchPhrase);

            },System.Windows.Threading.DispatcherPriority.ApplicationIdle);

        }
    }
}
