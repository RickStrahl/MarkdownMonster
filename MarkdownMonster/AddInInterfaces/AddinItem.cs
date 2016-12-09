using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MarkdownMonster.Annotations;

namespace MarkdownMonster.AddIns
{
    public class AddinItem : INotifyPropertyChanged
    {        
        public string id
        {
            get { return _id; }
            set
            {
                if (value == _id) return;
                _id = value;
                OnPropertyChanged();
            }
        }
        private string _id;

        public string gitUrl
        {
            get { return _gitUrl; }
            set
            {
                if (value == _gitUrl) return;
                _gitUrl = value;
                OnPropertyChanged();
            }
        }
        private string _gitUrl;

        public string gitVersionUrl
        {
            get { return _gitVersionUrl; }
            set
            {
                if (value == _gitVersionUrl) return;
                _gitVersionUrl = value;
                OnPropertyChanged();
            }
        }
        private string _gitVersionUrl;
        


        public string name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }
        private string _name;

        public string summary
        {
            get { return _summary; }
            set
            {
                if (value == _summary) return;
                _summary = value;
                OnPropertyChanged();
            }
        }
        private string _summary;

        
        public string description
        {
            get { return _description; }
            set
            {
                if (value == _description) return;
                _description = value;
                OnPropertyChanged();
            }
        }
        private string _description;


        public string icon => gitVersionUrl.Replace("version.json", "icon.png");

        public string version
        {
            get { return _version; }
            set
            {
                if (value == _version) return;
                _version = value;
                OnPropertyChanged();
            }
        }
        private string _version;

        public string author
        {
            get { return _author; }
            set
            {
                if (value == _author) return;
                _author = value;
                OnPropertyChanged();
            }
        }
        private string _author;

        
        public DateTime updated
        {
            get { return _updated; }
            set
            {
                if (value.Equals(_updated)) return;
                _updated = value;
                OnPropertyChanged();
            }
        }
        private DateTime _updated;

        

        public bool isInstalled
        {
            get { return _isInstalled; }
            set
            {
                if (value == _isInstalled) return;
                _isInstalled = value;
                OnPropertyChanged();
            }
        }
        private bool _isInstalled;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}