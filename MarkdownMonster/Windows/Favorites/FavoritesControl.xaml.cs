using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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


        void StartEditing(FavoriteItem favorite)
        {
            FavoritesModel.EditedFavorite = favorite;
            favorite.DisplayState.IsEditing = true;
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

            if (Directory.Exists(favorite.File))
                FavoritesModel.AppModel.Commands.OpenFolderBrowserCommand.Execute(favorite.File);
            else
                FavoritesModel.AppModel.Commands.OpenRecentDocumentCommand.Execute(favorite.File);
        }


        private void ButtonAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            var favorite = button?.DataContext as FavoriteItem;
            
            favorite = FavoritesModel.AddFavorite(favorite);
            FavoritesModel.SaveFavorites();

            StartEditing(favorite);
        }

        private void ButtonAddFolder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            var favorite = button?.DataContext as FavoriteItem;

            favorite = FavoritesModel.AddFavorite(favorite, new FavoriteItem { Title = "Grouping",  IsFolder = true});
            FavoritesModel.SaveFavorites();

            StartEditing(favorite);
        }

        private void ButtonDeleteFavorite_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            var favorite = button?.DataContext as FavoriteItem;
            if (favorite == null)
                return;

            FavoritesModel.DeleteFavorite(favorite);
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

            StartEditing(favorite);
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
        private bool IsDragging = false;

        private void TreeFavorites_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = ElementHelper.FindVisualTreeParent<TreeViewItem>(e.OriginalSource as FrameworkElement);
            if (item != null)
                item.IsSelected = true;

            startPoint = e.GetPosition(null);
            IsDragging = false;
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
                    IsDragging = true;
                }
            }
        }

        private void TreeViewItem_Drop(object sender, DragEventArgs e)
        {
            // avoid double drop events?
            if (!IsDragging)
                return;
            IsDragging = false;

            FavoriteItem targetItem;            
            e.Handled = true;

            if (sender is TreeView)
            {
                // dropped into treeview open space
                return; //targetItem = ActivePathItem;
            }

            targetItem = (e.OriginalSource as FrameworkElement)?.DataContext as FavoriteItem;
            if (targetItem == null)
                return;

            //  "path|title"
            var path = e.Data.GetData(DataFormats.UnicodeText) as string;
            if (string.IsNullOrEmpty(path))
                return;

            var tokens = path.Split('|');

            var sourceItem = FavoritesModel.FindFavoriteByFilenameAndTitle(FavoritesModel.Favorites, tokens[0], tokens[1]);
            if (sourceItem == null)
                return;
            
            var parentList = sourceItem.Parent?.Items;
            if (parentList == null)
                parentList = FavoritesModel.Favorites;

            parentList.Remove(sourceItem);
            parentList = null;
            

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

            FavoritesModel.SaveFavorites();
            WindowUtilities.DoEvents();
        }

        #endregion

        private void TextSearch_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            FavoritesModel.LoadFavorites();            
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Dispatcher.InvokeAsync(() =>FavoritesModel.SaveFavoritesQuick(),System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
    }
}
