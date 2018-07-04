using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using MarkdownMonster.Annotations;
using Newtonsoft.Json;

namespace MarkdownMonster.Favorites
{
    /// <summary>
    /// An individual Favorite item
    /// </summary>
    [DebuggerDisplay("{Title} - {File}")]
    public class FavoriteItem : INotifyPropertyChanged
    {
        [JsonIgnore]
        public string Filename
        {
            get { return Path.GetFileName(_file); }
        }


        public string File
        {
            get => _file;
            set
            {
                if (value == _file) return;
                _file = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Filename));
            }
        }
        private string _file;

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }
        private string _title;

        

        public string Description
        {
            get => _description;
            set
            {
                if (value == _description) return;
                _description = value;
                OnPropertyChanged();
            }
        }
        private string _description;



        public bool IsFolder
        {
            get => _isFolder;
            set
            {
                if (value == _isFolder) return;
                _isFolder = value;
                OnPropertyChanged();
            }
        }
        private bool _isFolder;


        [JsonIgnore]
        public bool IsExpanded
        {
            get => _IsExpanded;
            set
            {
                if (value == _IsExpanded) return;
                _IsExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }
        private bool _IsExpanded;


        public ObservableCollection<FavoriteItem> Items { get; set; } = new ObservableCollection<FavoriteItem>();
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
