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
        public ObservableCollection<FavoriteItem> Favorites = new ObservableCollection<FavoriteItem>();

        public AppModel Model { get;  }
        public MainWindow Window { get; }

        public FavoritesModel()
        {
            //Model = mmApp.Model;
            //Window = Model.Window;
        }

        public bool LoadFavorites()
        {
            var file = Path.Combine(mmApp.Configuration.CommonFolder, "MarkdownMonster-Favorites.json");
            if (!File.Exists(file))
            {
                Favorites = new ObservableCollection<FavoriteItem>();
                return true;
            }

            var favorites = JsonSerializationUtils.DeserializeFromFile(file, typeof(ObservableCollection<FavoriteItem>), false) as
                ObservableCollection<FavoriteItem>;

            if (favorites == null)
            {
                Favorites = new ObservableCollection<FavoriteItem>();                
                return false;
            }

            Favorites = favorites;
            return true;
        }

        


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
