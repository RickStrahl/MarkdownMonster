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
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for GitCommitDialog.xaml
    /// </summary>
    public partial class GitCommitDialog : MetroWindow
    {
        public GitCommitModel CommitModel { get; set; }

        public AppModel AppModel { get; set; }

        public GitCommitDialog()
        {
            InitializeComponent();
            AppModel = mmApp.Model;

            Owner = AppModel.Window;

            Loaded += GitCommitDialog_Loaded;
        }

        private void GitCommitDialog_Loaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ButtonCommit_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
