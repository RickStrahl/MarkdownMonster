using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using MarkdownMonster.Controls;
using MarkdownMonster.Favorites;
using Microsoft.WindowsAPICodePack.Dialogs;

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

        private void ButtonFileSelection_Click(object sender, RoutedEventArgs e)
        {
            string folder = mmApp.Configuration.LastFolder;
            if (FavoritesModel.EditedFavorite?.File != null)
                folder = System.IO.Path.GetDirectoryName(FavoritesModel.EditedFavorite.File);
            
            var dlg = new CommonOpenFileDialog();

            dlg.Title = "Select a file for this Favorite";
            dlg.InitialDirectory = folder;
            dlg.RestoreDirectory = true;
            dlg.ShowHiddenItems = true;
            dlg.ShowPlacesList = true;
            dlg.EnsurePathExists = true;

            var result = dlg.ShowDialog();

            if (result != CommonFileDialogResult.Ok)
                return;

            FavoritesModel.EditedFavorite.File = dlg.FileName;
        }

        #region Drag Operations

        private System.Windows.Point startPoint;

        private void TreeFavorites_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = ElementHelper.FindVisualTreeParent<TreeViewItem>(e.OriginalSource as FrameworkElement);
            if (item != null)
                item.IsSelected = true;

            startPoint = e.GetPosition(null);
        }

    

        private void TreeFavorites_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var selected = TreeFavorites.SelectedItem as FavoriteItem;
                if (selected == null)
                    return;

                var mousePos = e.GetPosition(null);
                var diff = startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var treeView = sender as TreeView;
                    var treeViewItem = WindowUtilities.FindAnchestor<TreeViewItem>((DependencyObject)e.OriginalSource);
                    if (treeView == null || treeViewItem == null)
                        return;

                    var dragData = new DataObject(DataFormats.UnicodeText, selected.File + "|" + selected.Title);
                    DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.All);
                }
            }
        }

        private void TreeViewItem_Drop(object sender, DragEventArgs e)
        {

            Debug.WriteLine("TreeViewDrop...");

            FavoriteItem targetItem;

            if (sender is TreeView)
            {
                // dropped into treeview open space
                e.Handled = true;
                return; //targetItem = ActivePathItem;
            }
            else
            {
                targetItem = (e.OriginalSource as FrameworkElement)?.DataContext as FavoriteItem;
                if (targetItem == null)
                    return;
            }
            e.Handled = true;


            //  "path|title"
            var path = e.Data.GetData(DataFormats.UnicodeText) as string;
            if (string.IsNullOrEmpty(path))
                return;

            var tokens = path.Split('|');

            var sourceItem = FavoritesModel.FindFavoriteByFilename(FavoritesModel.Favorites, tokens[0], tokens[1]);
            if (sourceItem == null)
                return;

            WindowUtilities.DoEvents();

            var parentList = sourceItem.Parent?.Items;
            if (parentList == null)
                parentList = FavoritesModel.Favorites;

            if (!parentList.Remove(sourceItem))
            {
                int x = 0;
            }
            parentList = null;
            WindowUtilities.DoEvents();

            
            if (targetItem.IsFolder && !sourceItem.IsFolder)
            {
                // dropped on folder: Add below
                parentList = targetItem.Items;
                sourceItem.Parent = targetItem;
            }
            else
            {
                // Dropped on file: Add after
                parentList = targetItem.Parent?.Items;
                if (parentList == null)
                {
                    parentList = FavoritesModel.Favorites;
                    sourceItem.Parent = null;
                }
                else
                    sourceItem.Parent = targetItem.Parent;
            }

            var index = parentList.IndexOf(targetItem);
            if (index < 0)
                index = 0;
            else
                index++;

            if(index >= parentList.Count)
                parentList.Add(sourceItem);
            else
                parentList.Insert(index,sourceItem);
            
            WindowUtilities.DoEvents();
            FavoritesModel.SaveFavorites();            
        }

        #endregion

        private void TextSearch_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            FavoritesModel.LoadFavorites();            
        }
    }
}
