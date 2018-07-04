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
        
            //Dispatcher.InvokeAsync(() =>
            //{
                FavoritesModel = new FavoritesModel()
                {
                    AppModel = mmApp.Model,
                    Window = mmApp.Model.Window
                };

                FavoritesModel.LoadFavorites();

                DataContext = FavoritesModel;

                EditPanel.Visibility = Visibility.Collapsed;


            //},System.Windows.Threading.DispatcherPriority.ApplicationIdle);
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
        }


        private void AddFavorite(FavoriteItem baseItem, FavoriteItem favoriteToAdd = null)
        {
            if (favoriteToAdd == null)
            {
                favoriteToAdd = new FavoriteItem()
                {
                    Title = "New Favorite",
                    File = "newFile.md"
                };
            }

            if (baseItem == null)
                FavoritesModel.Favorites.Insert(0,favoriteToAdd);
            else
            {
                if(baseItem.IsFolder)
                    baseItem.Items.Insert(0, favoriteToAdd);
                else
                {
                    var parentItems = baseItem.Parent?.Items;
                    if (parentItems == null)
                        parentItems = FavoritesModel.Favorites;
                    var index = parentItems.IndexOf(baseItem);
                    parentItems.Insert(index + 1, favoriteToAdd);
                }
            }
        }

        private void DeleteSelectedFavorite(FavoriteItem favorite)
        {
            var parentList = favorite.Parent?.Items;
            if (parentList == null)
                parentList = FavoritesModel.Favorites;

            parentList.Remove(favorite);
            FavoritesModel.SaveFavorites();
        }

        private void ButtonAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            var favorite = button?.DataContext as FavoriteItem;

            AddFavorite(favorite);
            FavoritesModel.SaveFavorites();
        }

        private void ButtonAddFolder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            var favorite = button?.DataContext as FavoriteItem;

            AddFavorite(favorite, new FavoriteItem { Title = "Grouping",  IsFolder = true});
            FavoritesModel.SaveFavorites();
        }

        private void ButtonDeleteFavorite_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            var favorite = button?.DataContext as FavoriteItem;
            if (favorite == null)
                return;

            DeleteSelectedFavorite(favorite);
        }

        private void ButtonEditComplete_Click(object sender, RoutedEventArgs e)
        {
            var favorite = FavoritesModel.EditedFavorite;
            if (favorite == null)
                return;

            favorite.DisplayState.IsEditing = false;

            var parentList = favorite.Parent?.Items;
            if (parentList == null)
                parentList = FavoritesModel.Favorites;

            // convoluted 
            var idx = parentList.IndexOf(favorite);
            if (idx == -1)
                idx = 0;
            parentList.Remove(favorite);
            parentList.Insert(idx,favorite);

            FavoritesModel.SaveFavorites();

            FavoritesModel.EditedFavorite = null;
        }

        private void ButtonStartEditing_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            var favorite = button?.DataContext as FavoriteItem;
            if (favorite == null)
                return;

            FavoritesModel.EditedFavorite = favorite;
            favorite.DisplayState.IsEditing = true;
        }

        private void ButtonRefreshList_Click(object sender, RoutedEventArgs e)
        {
            FavoritesModel.LoadFavorites();
        }

        private void ButtonEditList_Click(object sender, RoutedEventArgs e)
        {
            FavoritesModel.AppModel.Window.OpenTab(FavoritesModel.FavoritesFile);
        }
    }
}
