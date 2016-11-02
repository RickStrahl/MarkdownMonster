using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Westwind.Utilities;
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

        /// <summary>
        /// The name of the application
        /// </summary>
        [JsonIgnore]
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


        /// <summary>
        /// The theme used for the editor. Can be any of AceEditor themes
        /// twilight, visualstudio, github, monokai etc.
        /// </summary>
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

        /// <summary>
        /// Themes used to render the Preview. Preview themes are
        /// located in the .\PreviewThemes folder and you can add
        /// custom themes to this folder.
        /// </summary>
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


        /// <summary>
        /// Determines whether the editor wraps text or extends lines
        /// out. Default is false.
        /// </summary>
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


        /// <summary>
        /// Font size for the editor.
        /// </summary>
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


        /// <summary>
        /// Determines if spell checking is used. This value maps to the
        /// spell check button in the window header.
        /// </summary>
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


        /// <summary>
        /// Dictionary used by the editor. Defaults to 'en_US'.
        /// Others shipped: de_DE, es_ES, fr_FR
        /// Any OpenOffice style dictionary can be used by copying into
        /// the .\Editor folder providing .dic and .aff files.
        /// </summary>
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
      

        /// <summary>
        /// If true re-opens files that were open when last closed.
        /// </summary>
        public bool RememberOpenFiles { get; set; }


        /// <summary>
        /// Determines whether Markdown Monster runs as a Singleton application.
        /// If true only a single instance runs and parameters are forwarded to
        /// open in the single instance.
        /// </summary>
        public bool UseSingleWindow { get; set; }


        /// <summary>
        /// Determines whether errors are reported anonymously
        /// </summary>
        public bool ReportErrors { get; set; }


        [JsonIgnore]
        public string BugReportUrl { get; set; }

        [JsonIgnore]
        public string TelemetryUrl { get; set; }


        /// <summary>
        /// Flag to determine whether telemetry is sent
        /// </summary>
        public bool SendTelemetry { get; set; }

        /// <summary>
        /// Last folder used when opening a document
        /// </summary>
        public string LastFolder { get; set; }

        /// <summary>
        /// Last location where an image was opened.
        /// </summary>
        public string LastImageFolder { get; set; }


        /// <summary>
        /// Common folder where configuration files are stored.
        /// </summary>
        [JsonIgnore]
        public string CommonFolder { get; set;  }


        /// <summary>
        /// Command Processing for OpenFolder
        /// </summary>
        public string OpenFolderCommand { get; set;  }

        /// <summary>
        /// Command Processing for open command window
        /// </summary>
        public string OpenCommandLine { get; set; }

        /// <summary>
        /// A collection of the open Markdown documents.
        /// </summary>
        public List<MarkdownDocument> OpenDocuments { get; set; }


        /// <summary>
        /// List of recently opened files. Files opened and selected are
        /// added to the beginning of the list.
        /// </summary>
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

        /// <summary>
        /// Configuration object that olds info about how applications are updated
        /// </summary>
        public ApplicationUpdates ApplicationUpdates { get; set; }

        /// <summary>
        /// Hold last window position
        /// </summary>
        public WindowPosition WindowPosition { get; set; }


        /// <summary>
        /// Determines whether the preview browser is visible
        /// </summary>
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

        public bool FirstRun { get; set; }

        //internal string FileWatcherOpenFilePath;

        public ApplicationConfiguration()
        {
            WindowPosition = new WindowPosition();
            ApplicationUpdates = new ApplicationUpdates();

            OpenDocuments = new List<MarkdownDocument>();

            LastFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            CommonFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Markdown Monster");

            // TODO: REMOVE THIS AFTER A WHILE            
            try
            {
                string oldFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "West Wind Markdown Monster");
                if (Directory.Exists(oldFolder))
                {
                    if (!Directory.Exists(CommonFolder))
                        Directory.CreateDirectory(CommonFolder);

                    var dir = new DirectoryInfo(oldFolder);
                    foreach (var file in dir.EnumerateFiles("*.*"))
                    {
                        file.CopyTo(Path.Combine(CommonFolder, file.Name), true);
                        file.Delete();
                    }
                    dir.Delete(true);
                }
            }
            catch(Exception ex)
            {
                mmApp.Log(ex);
            }
            // TODO: END REMOVE THIS AFTER A WHILE

            BugReportUrl = "https://markdownmonster.west-wind.com/bugreport/bugreport.ashx?method=ReportBug";
            //BugReportUrl = "http://localhost/MarkdownMonster/bugreport.ashx?method=ReportBug";
            TelemetryUrl = "https://markdownmonster.west-wind.com/bugreport/bugreport.ashx?method=Telemetry";
            SendTelemetry = true;

            ApplicationTheme = Themes.Dark;
            RenderTheme = "Github";
            EditorTheme = "twilight";
            EditorFontSize = 19;
            EditorWrapText = true;
            EditorEnableSpellcheck = true;
            EditorDictionary = "EN_US";

            OpenCommandLine = "cmd.exe";
            OpenFolderCommand = "explorer.exe";

            RememberOpenFiles = true;
            UseSingleWindow = true;
            ReportErrors = true;            
            IsPreviewVisible = true;

            FirstRun = true;
        }

        public void AddRecentFile(string filename)
        {
            if (string.IsNullOrEmpty(filename) || filename.ToLower() == "untitled")
                return;

            if (RecentDocuments.Contains(filename))
                RecentDocuments.Remove(filename);

            RecentDocuments.Insert(0,filename);
            OnPropertyChanged(nameof(RecentDocuments));

            if (RecentDocuments.Count > 12)
                RecentDocuments = RecentDocuments.Take(12).ToList();            
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
