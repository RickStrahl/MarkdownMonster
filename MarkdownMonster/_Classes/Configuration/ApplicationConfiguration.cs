using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
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
        private Themes _applicationTheme;


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

        private string _editorTheme;

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
        /// Determines whether the preview attempts to sync to 
        /// the editor when previewing HTML.
        /// </summary>
        public PreviewSyncMode PreviewSyncMode
        {
            get { return _previewSyncMode; }
            set
            {
                if (value == _previewSyncMode) return;
                _previewSyncMode = value;
                OnPropertyChanged(nameof(PreviewSyncMode));
            }
        }
        private PreviewSyncMode _previewSyncMode;


        
        /// <summary>
        /// Determines whether documents are automatically saved
        /// whenever changes are made.
        /// 
        /// AutoSaveDocuments takes precedence over AutoSaveBackups
        /// </summary>
        public bool AutoSaveDocuments
        {
            get { return _autoSaveDocuments; }
            set
            {
                if (value == _autoSaveDocuments) return;
                _autoSaveDocuments = value;                                
                OnPropertyChanged(nameof(AutoSaveDocuments));
            }
        }
        private bool _autoSaveDocuments;

        /// <summary>
        /// If non-zero creates a backup in the number of minutes
        /// specified. 0 turns this feature off.
        /// </summary>
        public bool AutoSaveBackups { get; set; }

        public bool AlwaysUsePreviewRefresh
        {
            get { return _alwaysUsePreviewRefresh; }
            set
            {
                if (value == _alwaysUsePreviewRefresh) return;
                _alwaysUsePreviewRefresh = value;
                OnPropertyChanged(nameof(AlwaysUsePreviewRefresh));
            }
        }
        private bool _alwaysUsePreviewRefresh;

        /// <summary>
        /// The font used in the editor. Must be a proportional font
        /// </summary>
        public string EditorFont
        {
            get { return _editorFont; }
            set
            {
                if (_editorFont == value) return;
                _editorFont = value;
                OnPropertyChanged(nameof(EditorFont));
            }
        }

        private string _editorFont;

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
        /// Determines whether the active line is highlighted in the editor
        /// </summary>
        public bool EditorHighlightActiveLine
        {
            get { return _editorHighlightActiveLine; }
            set
            {
                if (_editorHighlightActiveLine == value) return;
                _editorHighlightActiveLine = value;
                OnPropertyChanged(nameof(EditorHighlightActiveLine));
            }
        }
        private bool _editorHighlightActiveLine;



        /// <summary>
        /// 
        /// </summary>
        public bool EditorShowLineNumbers
        {
            get { return _EditorShowLineNumbers; }
            set
            {
                if (_EditorShowLineNumbers == value) return;
                _EditorShowLineNumbers = value;
                OnPropertyChanged(nameof(EditorShowLineNumbers));
            }
        }
        private bool _EditorShowLineNumbers = false;


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
        private bool _editorEnableSpellcheck;


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
        private string _editorDictionary;


        /// <summary>
        /// Keyboard input hanlder type:
        /// default (ace/vs), vim, emacs
        /// </summary>
        public object EditorKeyboardHandler { get; set; }

        /// <summary>
        /// Determines whether the Markdown rendering allows script tags 
        /// in generated HTML output. Set this to true
        /// if you want to allow script tags to be rendered into
        /// HTML script tags and execute - such as embedding
        /// Gists or other Widgets that use scripts.        
        /// </summary>
        public bool AllowRenderScriptTags
        {
            get { return _allowRenderScriptTags; }
            set
            {
                if (value == _allowRenderScriptTags) return;
                _allowRenderScriptTags = value;
                OnPropertyChanged(nameof(AllowRenderScriptTags));
            }
        }
        private bool _allowRenderScriptTags;

        /// <summary>
        /// Determines whether links are always rendered with target='top'
        /// </summary>
        public bool RenderLinksExternal { get; set; }

        /// <summary>
        /// If greater than 0 re-opens up to number of files that were open when last closed.
        /// </summary>
        public int RememberLastDocuments { get; set; }


        public bool OpenInPresentationMode { get; set; }

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
        /// Remembers last Is link External setting when embedding links
        /// </summary>
        public bool LastLinkExternal { get; set; }

        public MarkdownOptions MarkdownOptions { get; set; }

        /// <summary>
        /// Common folder where configuration files are stored.
        /// </summary>
        [JsonIgnore]
        public string CommonFolder { get; set;  }

        [JsonIgnore]
        internal string AddinsFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_AddinsFolder))
                    _AddinsFolder = Path.Combine(CommonFolder, "Addins");
                return _AddinsFolder;
            }
        }
        private string _AddinsFolder;

        

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

        public int RecentDocumentLength { get; set; } = 12;

        /// <summary>
        /// Configuration object that olds info about how applications are updated
        /// </summary>
        public ApplicationUpdates ApplicationUpdates { get; set; }

        /// <summary>
        /// Hold last window position
        /// </summary>
        public WindowPosition WindowPosition { get; set; }

        /// <summary>
        /// Hold last window state.
        /// </summary>
        public WindowState WindowState { get; set; }

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

        private bool _isPreviewVisible;
        public bool FirstRun { get; set; }

        /// <summary>
        /// Disables all addins from loading
        /// </summary>
        public bool DisableAddins { get; set; }

        


        public ApplicationConfiguration()
        {
            MarkdownOptions = new MarkdownOptions();
            WindowPosition = new WindowPosition();
            ApplicationUpdates = new ApplicationUpdates();
            OpenDocuments = new List<MarkdownDocument>();

            LastFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            CommonFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Markdown Monster");

            PreviewSyncMode = PreviewSyncMode.EditorAndPreview;

            AutoSaveBackups = true;
            AutoSaveDocuments = false;
            
            
            BugReportUrl = "https://markdownmonster.west-wind.com/bugreport/bugreport.ashx?method=ReportBug";
            //BugReportUrl = "http://localhost.fiddler/MarkdownMonster/bugreport/bugreport.ashx?method=ReportBug";
            TelemetryUrl = "https://markdownmonster.west-wind.com/bugreport/bugreport.ashx?method=Telemetry";
            SendTelemetry = true;

            ApplicationTheme = Themes.Dark;
            RenderTheme = "Github";
            EditorTheme = "twilight";
            EditorFont = "Consolas";
            EditorFontSize = 17;
            EditorWrapText = true;
            EditorHighlightActiveLine = true;
            AllowRenderScriptTags = true;
            EditorEnableSpellcheck = true;
            EditorDictionary = "EN_US";
            EditorKeyboardHandler = "default";  // vim,emacs

            OpenCommandLine = "cmd.exe";
            OpenFolderCommand = "explorer.exe";

            RememberLastDocuments = 3;            
            ReportErrors = true;

            UseSingleWindow = false;

            IsPreviewVisible = true;
            OpenInPresentationMode = false;
            AlwaysUsePreviewRefresh = false;

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

            if (RecentDocuments.Count > RecentDocumentLength)
                RecentDocuments = RecentDocuments.Take(RecentDocumentLength).ToList();            
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
           if (!Directory.Exists(AddinsFolder))
                Directory.CreateDirectory(AddinsFolder);

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


    /// <summary>
    /// Holds the current Window position and splitter settings
    /// </summary>
    public class WindowPosition
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        

        public int SplitterPosition
        {
            get { return _splitterPosition; }
            set
            {
                _splitterPosition = value;
                //Debug.WriteLine(value);
            }   
        }
        private int _splitterPosition;
    }

    public class MarkdownOptions
    {
        /// <summary>
        /// Determines whether links are automatically expanded
        /// </summary>
        public bool AutoLinks { get; set; } = true;

        /// <summary>
        /// Determines if headers automatically generate 
        /// ids.
        /// </summary>
        public bool AutoHeaderIdentifiers { get; set; }

        /// <summary>
        /// If true strips Yaml FrontMatter headers
        /// </summary>
        public bool StripYamlFrontMatter { get; set; } = true;

        /// <summary>
        /// If true expand Emoji and Smileys 
        /// </summary>
        public bool EmojiAndSmiley { get; set; } = true;

        /// <summary>
        /// Creates playable media links from music and video files
        /// </summary>
        public bool MediaLinks { get; set; } = true;

        /// <summary>
        /// Adds additional list features like a. b.  and roman numerals i. ii. ix.
        /// </summary>
        public bool ListExtras { get; set; }
        

        public bool Figures { get; set; }

        /// <summary>
        /// Creates Github task lists like - [ ] Task 1
        /// </summary>
        public bool GithubTaskLists { get; set; } = true;

        /// <summary>
        /// Converts common typeographic options like -- to em dash
        /// quotes to curly quotes, triple dots to elipsis etc.
        /// </summary>
        public bool SmartyPants { get; set; }
    }

    public enum PreviewSyncMode
    {
        EditorToPreview,
        PreviewToEditor,
        EditorAndPreview,        
        None
    }
}
