using System.ComponentModel;
using System.Runtime.CompilerServices;
using WeblogAddin.Annotations;

namespace WeblogAddin
{
    public class WeblogPostMetadata : INotifyPropertyChanged
    {
        private string _title;
        private string _abstract;
        public string PostId { get; set; }

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary>
        /// This should hold the sanitized markdown text
        /// stripped of the config data.
        /// </summary>
        public string MarkdownBody { get; set; }

        /// <summary>
        /// This should hold the raw markdown text retrieved
        /// from the editor which will contain the meta post data
        /// </summary>
        public string RawMarkdownBody { get; set; }

        public string Abstract
        {
            get { return _abstract; }
            set
            {
                if (value == _abstract) return;
                _abstract = value;
                OnPropertyChanged(nameof(Abstract));
            }
        }

        public string Keywords { get; set; }

        public string Categories { get; set; }

        public string WeblogName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}