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
using MarkdownMonster.Favorites;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for Favorites.xaml
    /// </summary>
    public partial class FavoritesControl : UserControl
    {

        public FavoritesModel FavoritesModel { get; set; }

        public FavoritesControl()
        {   
            InitializeComponent();
            Loaded += Favorites_Loaded;
            
        }

        private void Favorites_Loaded(object sender, RoutedEventArgs e)
        {
            FavoritesModel = new FavoritesModel()
            {
                AppModel = mmApp.Model,
                Window = mmApp.Model.Window
            };

            FavoritesModel.LoadFavorites();

            DataContext = FavoritesModel;            
        }


        private void ButtonFavorite_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var favorite = button.DataContext as FavoriteItem;
            if (favorite == null)
                return;

            if (favorite.IsFolder)
            {
                favorite.IsExpanded = !favorite.IsExpanded;
                return;
            }

            FavoritesModel.AppModel.Commands.OpenRecentDocumentCommand.Execute(favorite.File);

            var window = WindowUtilities.FindAnchestor<FavoritesWindow>(this);
            window?.Close();
        }
    }
}
