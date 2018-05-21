using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarkdownMonster;
using Newtonsoft.Json;
using SnippetsAddin.Annotations;


namespace SnippetsAddin
{
    public class SnippetsAddinModel : INotifyPropertyChanged
    {

        public SnippetsAddinModel()
        {
            ScriptModeItems = new List<ScriptModeItem>
            {
                new ScriptModeItem
                {
                    Name = "C# Expressions",
                    ScriptMode = ScriptModes.CSharpExpressions
                },
                new ScriptModeItem
                {
                    Name = "C# Razor Template",
                    ScriptMode = ScriptModes.Razor
                },
            };
        }


        public SnippetsAddin Addin { get; set;  }               

        public MainWindow Window { get; set; }
                
        public AppModel AppModel
        {
            get { return _appModel; }
            set
            {
                if (Equals(value, _appModel)) return;
                _appModel = value;
                OnPropertyChanged();
            }
        }
        private AppModel _appModel;

        
        public Snippet ActiveSnippet
        {
            get { return _activeSnippet; }
            set
            {
                if (Equals(value, _activeSnippet)) return;
                _activeSnippet = value;
                OnPropertyChanged();
            }
        }
        private Snippet _activeSnippet;
        

        public List<ScriptModeItem> ScriptModeItems
        {
            get { return _scriptModeItems; }
            set
            {
                if (Equals(value, _scriptModeItems)) return;
                _scriptModeItems = value;
                OnPropertyChanged();
            }
        }
        private List<ScriptModeItem> _scriptModeItems;

        public SnippetsAddinConfiguration Configuration { get; set; }

        private ObservableCollection<Snippet> _snippets = new ObservableCollection<Snippet>();


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ScriptModeItem
    {
        public string Name {
            get;
            set; }
        public ScriptModes ScriptMode { get; set; } = ScriptModes.CSharpExpressions;
    }
}
