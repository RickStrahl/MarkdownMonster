using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using MarkdownMonster;
using SnippetsAddin.Annotations;
using Westwind.Utilities.Configuration;

namespace SnippetsAddin
{
    public class SnippetsAddinConfiguration : AppConfiguration, INotifyPropertyChanged
    {

        public static SnippetsAddinConfiguration Current;

        static SnippetsAddinConfiguration()
        {
            Current = new SnippetsAddinConfiguration();
            Current.Initialize();
        }


        public ObservableCollection<Snippet> Snippets
        {
            get { return _snippets; }
            set
            {
                if (Equals(value, _snippets)) return;
                _snippets = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Snippet> _snippets = new ObservableCollection<Snippet>();

        
        /// <summary>
        /// Optional Keyboard shortcut used to open the Snippets Window
        /// </summary>
        public string KeyboardShortcut
        {
            get { return _keyboardShortcut; }
            set
            {
                if (value == _keyboardShortcut) return;
                _keyboardShortcut = value;
                OnPropertyChanged();
            }
        }
        private string _keyboardShortcut = string.Empty;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
        #endregion


        #region AppConfiguration
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var provider = new JsonFileConfigurationProvider<SnippetsAddinConfiguration>()
            {
                JsonConfigurationFile = Path.Combine(mmApp.Configuration.CommonFolder, "SnippetsAddin.json")
            };

            if (!File.Exists(provider.JsonConfigurationFile))
            {
                if (!Directory.Exists(Path.GetDirectoryName(provider.JsonConfigurationFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(provider.JsonConfigurationFile));
            }

            return provider;
        }
        #endregion
    }
}
