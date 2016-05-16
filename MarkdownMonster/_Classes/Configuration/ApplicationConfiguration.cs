using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Westwind.Utilities.Configuration;


namespace MarkdownMonster
{

    /// <summary>
    /// Application level configuration for Markdown Monster
    /// </summary>
    public class ApplicationConfiguration : AppConfiguration, 
                                            INotifyPropertyChanged
    {
        private bool _isPreviewVisible;
        private string _editorTheme;
        private Themes _applicationTheme;
        private bool _editorEnableSpellcheck;
        private string _editorDictionary;


        public Themes ApplicationTheme
        {
            get { return _applicationTheme; }
            set
            {
                if (value == _applicationTheme) return;
                _applicationTheme = value;
                OnPropertyChanged(nameof(ApplicationTheme));
            }
        }

        public string EditorTheme
        {
            get { return _editorTheme; }
            set
            {
                if (value == _editorTheme) return;
                _editorTheme = value;
                OnPropertyChanged(nameof(EditorTheme));
            }
        }


        public string RenderTheme
        {
            get { return _RenderTheme; }
            set
            {
                if (value == _RenderTheme) return;
                _RenderTheme = value;
                OnPropertyChanged(nameof(RenderTheme));
            }
        }
        private string _RenderTheme;


        public bool EditorWrapText
        {
            get { return _EditorWrapText; }
            set
            {
                if (value == _EditorWrapText) return;
                _EditorWrapText = value;
                OnPropertyChanged(nameof(EditorWrapText));
            }
        }
        private bool _EditorWrapText;

        public int EditorFontSize
        {
            get { return _EditorFontSize; }
            set
            {
                if (value == _EditorFontSize) return;
                _EditorFontSize = value;
                OnPropertyChanged(nameof(EditorFontSize));
            }
        }
        private int _EditorFontSize;

        public bool EditorEnableSpellcheck
        {
            get { return _editorEnableSpellcheck; }
            set
            {
                if (value == _editorEnableSpellcheck) return;
                _editorEnableSpellcheck = value;
                OnPropertyChanged(nameof(EditorEnableSpellcheck));
            }
        }

        public string EditorDictionary
        {
            get { return _editorDictionary; }
            set
            {
                if (value == _editorDictionary) return;
                _editorDictionary = value;
                OnPropertyChanged(nameof(EditorDictionary));
            }
        }
      
        public bool RememberOpenFiles { get; set; }

        public bool UseSingleWindow { get; set; }

        
        public string LastFolder { get; set; }

        public string LastImageFolder { get; set; }

        [JsonIgnore]
        public string CommonFolder { get; set;  }

        public string OpenFolderCommand { get; set;  }

        public string OpenCommandLine { get; set; }

        public List<MarkdownDocument> OpenDocuments { get; set; }

        public List<string> RecentDocuments
        {
            get { return _recentDocuments; }
            set
            {
                if (value == _recentDocuments) return;
                _recentDocuments = value;
                OnPropertyChanged(nameof(RecentDocuments));
            }
        }
        private List<string> _recentDocuments = new List<string>();

        public ApplicationUpdates ApplicationUpdates { get; set; }

        public WindowPosition WindowPosition { get; set; }


        public bool IsPreviewVisible
        {
            get { return _isPreviewVisible; }
            set
            {
                if (value == _isPreviewVisible) return;
                _isPreviewVisible = value;
                OnPropertyChanged(nameof(IsPreviewVisible));
            }
        }

        internal string FileWatcherOpenFilePath;

        public ApplicationConfiguration()
        {
            WindowPosition = new WindowPosition();
            ApplicationUpdates = new ApplicationUpdates();

            OpenDocuments = new List<MarkdownDocument>();

            LastFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            CommonFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"West Wind Markdown Monster");
            FileWatcherOpenFilePath =  Path.Combine(
                  Path.GetTempPath(),
                 "__mm_openfile.txt");

            ApplicationTheme = Themes.Dark;
            RenderTheme = "blackout";
            RememberOpenFiles = true;

            EditorTheme = "twilight";
            EditorFontSize = 19;
            EditorWrapText = true;
            EditorEnableSpellcheck = true;
            EditorDictionary = "EN_US";

            OpenCommandLine = "cmd.exe";
            OpenFolderCommand = "explorer.exe";

            UseSingleWindow = true;
        }

        public void AddRecentFile(string filename)
        {
            if (RecentDocuments.Contains(filename))
                RecentDocuments.Remove(filename);
            RecentDocuments.Insert(0,filename);

            if (RecentDocuments.Count > 12)
                RecentDocuments = RecentDocuments.Take(10).ToList();            
        }

        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var provider = new JsonFileConfigurationProvider<ApplicationConfiguration>()
            {
                JsonConfigurationFile = Path.Combine(CommonFolder,"MarkdownMonster.json")
            };

            if (!File.Exists(provider.JsonConfigurationFile))
            {
                if (!Directory.Exists(Path.GetDirectoryName(provider.JsonConfigurationFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(provider.JsonConfigurationFile));
            }

            return provider;
        }
        public event PropertyChangedEventHandler PropertyChanged;        
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum MarkdownStyles
    {
        Common,
        GitHub
    }

    public class ApplicationUpdates
    {
        /// <summary>
        /// Url where installer is downloaded from
        /// </summary>
        [JsonIgnore]
        public string InstallerDownloadUrl { get; }

        /// <summary>
        /// Url to check version info from
        /// </summary>
        [JsonIgnore]
        public string UpdateCheckUrl { get; }


        /// <summary>
        /// Last date and time when an update check was performed
        /// </summary>
        public DateTime LastUpdateCheck { get; set; }

        /// <summary>
        /// Frequency for update checks in days. Done on shutdown
        /// </summary>
        public int  UpdateFrequency { get; set; }

        public int AccessCount { get; set; }

        public ApplicationUpdates()
        {
            InstallerDownloadUrl = "http://west-wind.com/files/MarkdownMonsterSetup.exe";
            UpdateCheckUrl = "http://west-wind.com/files/MarkdownMonster_version.xml";
            UpdateFrequency = 7;
        }
    }

    public class WindowPosition
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int SplitterPosition { get; set; }
    }
}
