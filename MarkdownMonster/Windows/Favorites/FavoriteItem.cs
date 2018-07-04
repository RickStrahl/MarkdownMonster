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
        public FavoriteItem Parent
        {
            get { return _Parent; }
            set
            {
                if (value == _Parent) return;
                _Parent = value;
                OnPropertyChanged(nameof(Parent));
            }
        }
        private FavoriteItem _Parent;
        
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

        [JsonIgnore]
        public FavoriteDisplayState DisplayState
        {
            get { return _DisplayState; }
            set { _DisplayState = value; }
        }
        private FavoriteDisplayState _DisplayState = new FavoriteDisplayState();


        public ObservableCollection<FavoriteItem> Items { get; set; } = new ObservableCollection<FavoriteItem>();
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class FavoriteDisplayState : INotifyPropertyChanged
    {
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value == _IsSelected) return;
                _IsSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
        private bool _IsSelected;


        public bool IsEditing
        {
            get { return _IsEditing; }
            set
            {
                if (value == _IsEditing) return;
                _IsEditing = value;
                OnPropertyChanged(nameof(IsEditing));
            }
        }
        private bool _IsEditing;


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
