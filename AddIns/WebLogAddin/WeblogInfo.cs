using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using MarkdownMonster.Annotations;
using Westwind.Utilities;

namespace WeblogAddin
{
    public class WeblogInfo : INotifyPropertyChanged
    {
       
        public WeblogInfo()
        {
            Id = DataUtils.GenerateUniqueId(8);
            BlogId = "1";
        }

        public string Id { get; set; }

        

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

        

        public string Username
        {
            get { return _username; }
            set
            {
                if (value == _username) return;
                _username = value;
                OnPropertyChanged();
            }
        }
        private string _username;

        public string Password
        {
            get { return _password; }
            set
            {
                _password = EncryptPassword(value);
                OnPropertyChanged();
            }
        }

        

        public string ApiUrl
        {
            get { return _apiUrl; }
            set
            {
                if (value == _apiUrl) return;
                _apiUrl = value;
                OnPropertyChanged();
            }
        }
        private string _apiUrl;

        

        public object BlogId
        {
            get { return _blogId; }
            set
            {
                if (Equals(value, _blogId)) return;
                _blogId = value;
                OnPropertyChanged();
            }
        }
        private object _blogId;

        

        public WeblogTypes Type
        {
            get { return _type; }
            set
            {
                if (value == _type) return;
                _type = value;
                OnPropertyChanged();
            }
        }
        private WeblogTypes _type = WeblogTypes.MetaWeblogApi;


        

        /// <summary>
        /// Url used to preview the post. The postId can be embedded into 
        /// the value by using {0}.
        /// </summary>
        public string PreviewUrl
        {
            get { return _previewUrl; }
            set
            {
                if (value == _previewUrl) return;
                _previewUrl = value;
                OnPropertyChanged();
            }
        }
        private string _previewUrl;

        string pb = Encoding.Default.GetString( new byte[] {  44, 33, 29,233, 255, 78, 33, 89, 88, 235, 121, 187});
        private string postFix = "*~~*";
        private string _password;

        public string EncryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            if (password.EndsWith(postFix))
                return password;

            return Encryption.EncryptString(password,pb) + postFix;
        }

        public string DecryptPassword(string encrypted)
        {
            if (string.IsNullOrEmpty(encrypted))
                return string.Empty;

            if (!encrypted.EndsWith(postFix))
                return encrypted;

            encrypted = encrypted.Replace(postFix, "");

            return Encryption.DecryptString(encrypted, pb);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum WeblogTypes
    {
        MetaWeblogApi,
        Wordpress,
        Unknown
    }
}