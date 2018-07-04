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

            Favorites = favorites;
            return true;
        }



        /// <summary>
        /// Saves Favorites to the Favorites file in common folder
        /// </summary>
        /// <returns></returns>
        public bool SaveFavorites()
        {
            return JsonSerializationUtils.SerializeToFile(Favorites, FavoritesFile, throwExceptions: false, formatJsonOutput: true);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
