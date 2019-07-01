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
    }
}
