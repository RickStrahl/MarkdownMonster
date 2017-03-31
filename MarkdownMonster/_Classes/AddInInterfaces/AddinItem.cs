#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 04/28/2016
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using MarkdownMonster.Annotations;

namespace MarkdownMonster.AddIns
{
    public class AddinItem : INotifyPropertyChanged
    {        
        /// <summary>
        /// Unique ID for this addin. Prefer you use 
        /// a camel cased version of the Addin without
        /// the word Addin in it.
        /// 
        /// Example: Snippet, Commander, PasteImageToAzureBlob
        /// </summary>
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


        /// <summary>
        /// The base URL to the Git Repo where this add in lives.
        /// Repo must follow addin guidelines for layout with a
        /// Build folder that contains a Zip file of 
        /// </summary>
        public string gitUrl
        {
            get { return _gitUrl; }
            set
            {
                if (value == _gitUrl) return;
                _gitUrl = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(gitVersionUrl));
            }
        }
        private string _gitUrl;

        public string gitVersionUrl => gitUrl.ToLower()
                                           .Replace("https://github.com", "https://raw.githubusercontent.com") +
                                                    "/master/Build/version.json";
        
        public string gitIconUrl => gitUrl.ToLower()
                                           .Replace("https://github.com", "https://raw.githubusercontent.com") +
                                                    "/master/Build/icon.png";

        public string gitScreenShotUrl => gitUrl.ToLower()
                                           .Replace("https://github.com", "https://raw.githubusercontent.com") +
                                                    "/master/Build/screenshot.png";

        /// <summary>
        /// The display name for the addin
        /// </summary>
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


        /// <summary>
        /// A short one paragraph description of the addin. This is what
        /// displays in the Addin Manager's list display
        /// </summary>
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

        /// <summary>
        /// Detailed description of the Addin. Put as much detail as you want here,
        /// but you should shoot for roughly a page in the addin manager's detail
        /// view.
        /// </summary>
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


        /// <summary>
        /// Addin Version using 1.0.0.0 format.
        /// </summary>
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

        /// <summary>
        /// The author or company that authored this addin. 
        /// Typically: © Rick Strahl - West Wind Technologies, 2017
        /// </summary>
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

        /// <summary>
        /// Minimum required version of Markdown Monster to run this
        /// addin.
        /// </summary>
        public string minVersion
        {
            get { return _minVersion; }
            set
            {
                if (value == _minVersion) return;
                _minVersion = value;
                OnPropertyChanged();
            }
        }
        private string _minVersion = "1.0";


        /// <summary>
        /// Date when this addin was updated. When making this change in the JSON file use
        /// 12:00 as time.
        /// Example: "updated": "2017-2-15T12:00:00Z"
        /// </summary>
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

        
        /// <summary>
        /// Internally used value that determines whether this addin is installed.
        /// set after initial download of addin list and checking for installed
        /// addins.
        /// </summary>
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


        /// <summary>
        /// Determines whether a newer version of the addin is available
        /// if installed. Available only after initial list has completely loaded.
        /// </summary>
        public bool updateAvailable
        { 

            get { return _updateAvailable; }
            set
            {
                if (value == _updateAvailable) return;
                _updateAvailable = value;
                OnPropertyChanged();
            }
        }
        

        /// <summary>
        /// Installed version if any. null if not installed.
        /// </summary>
        public string installedVersion
        {
            get { return _installedVersion; }
            set
            {
                if (value == _installedVersion) return;
                _installedVersion = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Determines whether the addin is enabled.
        /// </summary>
        public bool isEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                OnPropertyChanged();
            }
        }
        private bool _isEnabled;

        private string _installedVersion;

        private bool _updateAvailable;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}