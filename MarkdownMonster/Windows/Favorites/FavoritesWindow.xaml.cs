using System;
using System.Collections.Generic;
using System.Globalization;
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
using MarkdownMonster.Favorites;
using MarkdownMonster.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for FavoritesWindow.xaml
    /// </summary>
    public partial class FavoritesWindow : Window
    {
        public FavoritesWindow()
        {
            InitializeComponent();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }


}
