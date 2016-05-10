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
    /// Interaction logic for PasteHref.xaml
    /// </summary>
    public partial class PasteHref : MetroWindow
    {

        public string Link { get; set; }        
        public string LinkText { get; set; }
        public bool IsExternal { get; set; }
        public string HtmlResult { get; set; }


        public PasteHref()
        {
            InitializeComponent();

            DataContext = this;
            mmApp.SetThemeWindowOverride(this);

            Loaded += PasteHref_Loaded;

        }

        private void PasteHref_Loaded(object sender, RoutedEventArgs e)
        {
            this.TextLink.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == ButtonCancel)
                DialogResult = false;
            else
            {
                DialogResult = true;                
            }

            Close();
        }
    }
}
