using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.Annotations;

namespace MarkdownMonster.Windows
{
    public class GitCommitModel : INotifyPropertyChanged
    {
        public string Filename
        {
            get { return _Filename; }
            set
            {
                if (value == _Filename) return;
                _Filename = value;
                OnPropertyChanged(nameof(Filename));
            }
        }
        private string _Filename;

        public string CommitMessage
        {
            get { return _CommitMessage; }
            set
            {
                if (value == _CommitMessage) return;
                _CommitMessage = value;
                OnPropertyChanged(nameof(CommitMessage));
            }
        }
        private string _CommitMessage;


        public bool CommitAndPush
        {
            get { return _CommitAndPush; }
            set
            {
                if (value == _CommitAndPush) return;
                _CommitAndPush = value;
                OnPropertyChanged(nameof(CommitAndPush));
            }
        }
        private bool _CommitAndPush;




        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
