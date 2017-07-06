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
    /// Interaction logic for FilePasswordDialog.xaml
    /// </summary>
    public partial class FilePasswordDialog 
    {
        public MarkdownDocument document;

        public FilePasswordDialog(MarkdownDocument document, bool decrypt)
        {
            this.document = document;
            InitializeComponent();

            DataContext = this;
            mmApp.SetThemeWindowOverride(this);

            
            if (decrypt)
            {
                ButtonLabel.Text = "Decrypt";
                Title = "Decrypt file: " + document.FilenamePathWithIndicator;
            }
            else
                Title = "Encrypt file: " + document.FilenamePathWithIndicator;

            TextPassword.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            document.Password = TextPassword.SecurePassword;
            DialogResult = true;
            Close();

        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            document.Password = null;
            DialogResult = false;
            Close();
        }
    }
}
