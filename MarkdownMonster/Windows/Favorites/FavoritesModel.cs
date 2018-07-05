using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.Annotations;
using MarkdownMonster.Favorites;
using Westwind.Utilities;

namespace MarkdownMonster.Favorites
{
    public class FavoritesModel : INotifyPropertyChanged
    {
        

        public ObservableCollection<FavoriteItem> Favorites
        {
            get => _favorites;
            set
            {
                if (Equals(value, _favorites)) return;
                _favorites = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<FavoriteItem> _favorites = new ObservableCollection<FavoriteItem>();

        public FavoriteItem EditedFavorite
        {
            get => _editedFavorite;
            set
            {
                if (Equals(value, _editedFavorite)) return;
                _editedFavorite = value;
                OnPropertyChanged();
            }
        }
        private FavoriteItem _editedFavorite;


        public AppModel AppModel { get; set; }
        public MainWindow Window { get; set; }

        public static string FavoritesFile { get; set;  }

        
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (value == _searchText) return;
                _searchText = value;
                OnPropertyChanged();
            }
        }
        private string _searchText;

        public FavoritesModel()
        {
            //Model = mmApp.Model;
            //Window = Model.Window;
        }

        static FavoritesModel()
        {
            FavoritesFile = Path.Combine(mmApp.Configuration.CommonFolder, "MarkdownMonster-Favorites.json");
        }


        /// <summary>
        /// Loads Favorites from Favorties file in Common Folder
        /// </summary>
        /// <returns></returns>
        public bool LoadFavorites()
        {
            if (!File.Exists(FavoritesFile))
            {
                Favorites = new ObservableCollection<FavoriteItem>();
                return true;
            }

            var favorites = JsonSerializationUtils.DeserializeFromFile(
                                    FavoritesFile,
                                    typeof(ObservableCollection<FavoriteItem>),
                                    false) as ObservableCollection<FavoriteItem>;

            
            if (favorites == null)
            {
                Favorites = new ObservableCollection<FavoriteItem>();
                return false;
            }

            UpdateParents(favorites, null);

            FilterList(favorites);
            
            Favorites = favorites;
            
            return true;
        }

        void FilterList(ObservableCollection<FavoriteItem> favorites)
        {
            foreach (var fav in favorites)
            {
                if (string.IsNullOrEmpty(SearchText))
                    fav.DisplayState.IsVisible = true;
                else if (!string.IsNullOrEmpty(fav.File) &&
                         fav.File.IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) > -1)
                    fav.DisplayState.IsVisible = true;
                else if(!string.IsNullOrEmpty(fav.Title) && fav.Title.IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) > -1)
                    fav.DisplayState.IsVisible = true;
                else
                    fav.DisplayState.IsVisible = false;

                // make parents visible
                if (fav.DisplayState.IsVisible)
                {
                    // make all the parents visible too
                    var parent = fav.Parent;
                    while (parent != null)
                    {
                        parent.DisplayState.IsVisible = true;
                        parent = parent.Parent;
                    }
                }

                if (fav.Items.Count > 0)
                    FilterList(fav.Items);
            }
        }

        void UpdateParents(ObservableCollection<FavoriteItem> favorites, FavoriteItem parent)
        {
            foreach (var fav in favorites)
            {
                fav.Parent = parent;
                if (fav.Items.Count > 0)
                    UpdateParents(fav.Items, fav);
            }
        }


        public FavoriteItem AddFavorite(FavoriteItem baseItem, FavoriteItem favoriteToAdd = null)
        {
            if (favoriteToAdd == null)
            {
                favoriteToAdd = new FavoriteItem()
                {
                    Title = "New Favorite",
                    File = "newFile.md"
                };
                favoriteToAdd.DisplayState.IsEditing = true;
            }

            if (baseItem == null)
                Favorites.Insert(0, favoriteToAdd);
            else
            {
                if (baseItem.IsFolder)
                    baseItem.Items.Insert(0, favoriteToAdd);
                else
                {
                    var parentItems = baseItem.Parent?.Items;
                    if (parentItems == null)
                        parentItems = Favorites;
                    var index = parentItems.IndexOf(baseItem);
                    parentItems.Insert(index + 1, favoriteToAdd);
                }
            }

            return favoriteToAdd;
        }

        public void DeleteFavorite(FavoriteItem favorite)
        {
            var parentList = favorite.Parent?.Items;
            if (parentList == null)
                parentList = Favorites;

            parentList.Remove(favorite);
            SaveFavorites();
        }

        /// <summary>
        /// Saves Favorites to the Favorites file in common folder
        /// </summary>
        /// <returns></returns>
        public bool SaveFavorites()
        {
            bool result = JsonSerializationUtils.SerializeToFile(Favorites, FavoritesFile, throwExceptions: false, formatJsonOutput: true);
            if (result)
                // if we have the editor open show the change of the JSON file
                AppModel?.Window?.CheckFileChangeInOpenDocuments();

            return result;
        }


        /// <summary>
        /// Searches the tree for a specific item
        /// </summary>
        /// <param name="parentList"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public FavoriteItem FindFavoriteByFilenameAndTitle(ObservableCollection<FavoriteItem> parentList, string file, string title)
        {
            string lowerFullName = (file + "|" + title).ToLowerInvariant();
            
            // Files first for perf
            foreach (var item in parentList.Where(pi => !pi.IsFolder))
            {               
                if ((item.File + "|" + item.Title).ToLowerInvariant()  == lowerFullName)
                    return item;
            }

            // then directories recursively
            foreach (var item in parentList.Where(pi => pi.IsFolder))
            {
                if ((item.File + "|" + item.Title).ToLowerInvariant() == lowerFullName)
                    return item;

                if (item.IsFolder && item.Items != null && item.Items.Count > 0)
                {
                    var childItem = FindFavoriteByFilenameAndTitle(item.Items, file, title);
                    if (childItem != null)
                        return childItem;
                }
            }

            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
