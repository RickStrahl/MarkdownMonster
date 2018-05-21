using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SnippetsAddin.Annotations;

namespace SnippetsAddin
{
    public class Snippet : INotifyPropertyChanged
    {
        public Snippet()
        {
            ScriptMode = ScriptModes.CSharpExpressions;
        }
        
        /// <summary>
        /// A display name for the snippet.
        /// </summary>
        public string Name
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


        /// <summary>
        /// The actual snippet text to embed and evaluate
        /// </summary>
        public string SnippetText
        {
            get { return _snippetText; }
            set
            {
                if (value == _snippetText) return;
                _snippetText = value;
                OnPropertyChanged();

                // clear compiled id - might need to recompile
                CompiledId = null;
            }
        }
        private string _snippetText;

        public string Shortcut
        {
            get { return _shortcut; }
            set
            {
                if (value == _shortcut) return;
                _shortcut = value;
                OnPropertyChanged();
            }
        }
        private string _shortcut;


        

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
        private string _keyboardShortcut;

        //public string ExpansionShortCut
        //{
        //    get { return _expansionShortCut; }
        //    set
        //    {
        //        if (value == _expansionShortCut) return;
        //        _expansionShortCut = value;
        //        OnPropertyChanged();
        //    }
        //}
        //private string _expansionShortCut;

        public ScriptModes ScriptMode
        {
            get { return _scriptMode; }
            set
            {
                if (value == _scriptMode) return;
                _scriptMode = value;
                OnPropertyChanged();
            }
        }
        private ScriptModes _scriptMode;

        /// <summary>
        /// Id assigned to a compiled bit of script
        /// </summary>
        public string CompiledId { get; set;  }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public enum ScriptModes
    {
        // {{ expr syntax}}         
        CSharpExpressions, 
        // @Razor sytnax 
        Razor     
    }
   
}
