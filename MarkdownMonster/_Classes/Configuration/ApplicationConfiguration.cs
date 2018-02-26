using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using MarkdownMonster.Configuration;
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
        public bool OpenInPresentationMode { get; set; }

        /// <summary>
        /// Determines whether Markdown Monster runs as a Singleton application.
        /// If true only a single instance runs and parameters are forwarded to
        /// open in the single instance.
        /// </summary>
        public bool UseSingleWindow { get; set; }

        #region Editor Settings
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
        /// Determines whether the internal or external window previewer are
        /// used
        /// </summary>
        public PreviewModes PreviewMode
        {
            get { return _PreviewMode; }
            set
            {
                if (value == _PreviewMode) return;
                _PreviewMode = value;
                OnPropertyChanged(nameof(PreviewMode));
            }
        }
        private PreviewModes _PreviewMode = PreviewModes.InternalPreview;

        /// <summary>
        /// If set to true causes Http links in the Previewer
        /// to be opened in the default system Web Browser
        /// </summary>
        public bool PreviewHttpLinksExternal
        {
            get { return _previewHttpLinksExternal; }
            set
            {
                if (value == _previewHttpLinksExternal) return;
                OnPropertyChanged();
                _previewHttpLinksExternal = value;
            }
        }
        private bool _previewHttpLinksExternal;

        
        /// <summary>
        /// String that holds any of the following as a comma delimited string
        /// in all lower case:
        /// "toolbar,statusbar,menu,preview,tabs";
        /// 
        /// Any of those are hidden in distraction free mode.
        /// </summary>
        public string DistractionFreeModeHideOptions
        {
            get
            {
                if (_DistractionFreeModeHideOptions == null)
                    return string.Empty;
                return _DistractionFreeModeHideOptions;
            }
            set
            {
                if (value == _DistractionFreeModeHideOptions) return;
                _DistractionFreeModeHideOptions = value;
                OnPropertyChanged(nameof(DistractionFreeModeHideOptions));
            }
        }
        private string _DistractionFreeModeHideOptions = "toolbar,statusbar,menu,preview,tabs,maximized";

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
        
        
        /// <summary>
        /// Determines whether the Preview browser always does a full 
        /// refresh when the preview is updated. Normally MM tries to
        /// update just the document content. Use this setting if you
        /// are rendering custom content that includes script tags that 
        /// need to execute in the page in the rendered content.
        /// </summary>
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
        /// Zoom level percentage on top of the EditorFontSize
        /// </summary>
        public int EditorZoomLevel
        {
            get { return _editorZoomLevel; }
            set
            {
                if (value == _editorZoomLevel) return;
                _editorZoomLevel = value;
                OnPropertyChanged(nameof(EditorZoomLevel));                
                _editorZoomLevel = value;
            }
        }
        private int _editorZoomLevel = 100;
        

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
        /// Determines whether the editor should show invisible characters.
        /// Default is <see langword="false" />.
        /// </summary>
        public bool EditorShowInvisibles
        {
            get { return _EditorShowInvisibles; }
            set
            {
                if (_EditorShowInvisibles == value) return;
                _EditorShowInvisibles = value;
                OnPropertyChanged(nameof(EditorShowInvisibles));
            }
        }
        private bool _EditorShowInvisibles = false;

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
        /// Default code syntax displayed in the Paste Code dialog
        /// </summary>
        public string DefaultCodeSyntax { get; set; }


        /// <summary>
        /// Image editor used to edit images. Empty uses system default editor
        /// </summary>
        public string ImageEditor { get; set; }

		/// <summary>
		/// Image viewer used to open images. Empty setting uses the default viewer
		/// </summary>
		public string ImageViewer { get; set;  }


        /// <summary>
        /// Jpeg Image Compression level from 50 to 100. Defaults 80.
        /// </summary>
        public int JpegImageCompressionLevel { get; set; } = 80;

        /// <summary>
        /// Web Browser Preview Executable - use this to force 
        /// a specific browser since Windows seems to not be able
        /// to maintain a proper association long term.
        /// 
        /// If not set or path doesn't exist, uses Windows default
        /// configuration.
        /// </summary>
        public string WebBrowserPreviewExecutable { get; set; }

        #endregion



        #region Document Operations

        /// <summary>
        /// Command Processing for OpenFolder
        /// </summary>
        public string OpenFolderCommand { get; set;  }

		/// <summary>
		/// Command Processing Executable to bring up a terminal window
		/// Command or Powershell, but could also be Console or ConEmu
		/// cmd.exe				/k \"cd {0}\"
		/// powershell.exe		-NoExit -Command  "&amp; cd 'c:\program files'"
		/// </summary>
		public string TerminalCommand { get; set; }

		/// <summary>
		/// Terminal executable arguments to pass to bring up terminal
		/// in a specific folder. {0} represents folder name.
		/// </summary>
	    public string TerminalCommandArgs { get; set; }

        /// <summary>
        /// Determines how Git Commits are handled - either just commit
        /// or Commit and Push
        /// </summary>
        public GitCommitBehaviors GitCommitBehavior { get; set; } = GitCommitBehaviors.CommitAndPush;


        /// <summary>
        /// A collection of the open Markdown documents.
        /// </summary>
        public List<MarkdownDocument> OpenDocuments { get; set; }


        /// <summary>
        /// List of recently opened files. Files opened and selected are
        /// added to the beginning of the list.
        /// </summary>
        public ObservableCollection<string> RecentDocuments
        {
            get { return _recentDocuments; }
            set
            {
                if (value == _recentDocuments) return;
                _recentDocuments = value;
                OnPropertyChanged(nameof(RecentDocuments));
                OnPropertyChanged(nameof(RecentDocumentList));
            }
        }
        private ObservableCollection<string> _recentDocuments = new ObservableCollection<string>();


        /// <summary>
        /// Internal property used to display the recent document list
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<RecentDocumentListItem> RecentDocumentList
        {
            get
            {
                var list = RecentDocuments.Take(5);
                var docs = new ObservableCollection<RecentDocumentListItem>();
                foreach (var doc in list)
                {
                    docs.Add(new RecentDocumentListItem
                    {
                        Filename = doc,
                        DisplayFilename = mmFileUtils.GetCompactPath(doc, 70)
                    });
                }
                return docs;
            }
        }


        /// <summary>
        /// Determines how many items to display in the recent documents list
        /// </summary>
        public int RecentDocumentsLength { get; set; }

        /// <summary>
        /// Determines how many of the last documents are remembered and 
        /// reopened at most
        /// </summary>
        public int RememberLastDocumentsLength { get; set; }
        #endregion

        #region Nested Objects

        public MarkdownOptionsConfiguration MarkdownOptions { get; set; }

	    /// <summary>
	    /// Editor to editor syntax mappings that maps file extensions to 
	    /// specific Ace Editor syntax formats. If a file with the given
	    /// extension is opened it uses the specified syntax highlighting 
	    /// in the editor.
	    /// </summary>
	    public Dictionary<string, string> EditorExtensionMappings { get; set; } = new Dictionary<string, string>
	    {
		    {"md", "markdown"},
            {"markdown", "markdown" },
	        {"mdcrypt", "markdown"},
	        {"mdown", "markdown" },
	        {"mkd", "markdown" },
	        {"mkdn", "markdown" },
	        
            {"json", "json"},
		    {"html", "html"},
			{"htm", "html" },

		    {"cs", "csharp"},
		    {"js", "javascript"},
		    {"ts", "typescript"},
		    {"css", "css"},
		    {"less", "less"},
		    {"sass", "sass"},
		    {"sql", "sqlserver"},
		    {"prg", "foxpro"},
		    {"vb", "vb"},
		    {"py", "python"},
		    {"c", "c_cpp"},
		    {"cpp", "c_cpp"},
		    {"ps1", "powershell"},
		    {"ini", "ini"},
		    {"sh", "bash"},
		    {"bat", "batchfile"},
		    {"cmd", "batchfile"},
			
		    {"asp", "html"},
		    {"aspx", "html"},
		    {"jsx", "jsx"},
		    {"php", "php"},
		    {"go", "golang"},
		    {"cshtml", "razor"},
            {"vbhtml", "razor" },
		    {"r", "r"},
		    {"mak", "makefile"},

		    {"xml", "xml"},
		    {"xaml", "xml"},
		    {"wsdl", "xml"},
		    {"config", "xml"},
		    {"csproj", "xml"},
            {"sln", "xml" },
		    {"nuspec", "xml"},

		    {"yaml", "yaml"},
	        {"yml", "yaml"},
            {"txt", "text"},
		    {"log", "text"}	        
	    };

		/// <summary>
		/// Configuration object that olds info about how applications are updated
		/// </summary>
		public ApplicationUpdatesConfiguration ApplicationUpdates { get; set; }

	    /// <summary>
	    /// Hold last window position
	    /// </summary>
	    public WindowPositionConfiguration WindowPosition { get; set; }


	    /// <summary>
	    /// Configuration Setting for the Folder Browser
	    /// </summary>
	    public FolderBrowserConfiguration FolderBrowser { get; set; }

		#endregion



		#region Bug Reporting and Telemetry

		/// <summary>
		/// Determines whether errors are reported anonymously
		/// </summary>
		public bool ReportErrors { get; set; }


		/// <summary>
		/// Custom Bug Reporting url. Obsolete - in lieu of ApplicationInsights
		/// </summary>
		[JsonIgnore]
		public string BugReportUrl { get; set; }



		/// <summary>
		/// Custom Bug Reporting url. Obsolete - in lieu of ApplicationInsights
		/// </summary>
		[JsonIgnore]
		public string TelemetryUrl { get; set; }


		/// <summary>
		/// Flag to determine whether telemetry is sent
		/// </summary>
		public bool SendTelemetry { get; set; }

        #endregion

        #region Miscellaneous Settings
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


        /// <summary>
        /// Remembers last Is link External setting when embedding links
        /// </summary>
        public bool LastLinkExternal { get; set; }

        /// <summary>
        /// Disables all addins from loading
        /// </summary>
        public bool DisableAddins { get; set; }


        /// <summary>
        /// If set makes the application not use GPU accelleration.
        /// Set this setting if you have problems with MM starting up
        /// with a black screen. A very few  video drivers are known to
        /// have render problems and this setting allows getting around
        /// this driver issue.
        /// </summary>
        public bool DisableHardwareAcceleration { get; set; }

        /// <summary>
        /// By default passwords in addins are encrypted with machine encryption
        /// keys which means they are not portable. When false a fixed password
        /// is used that is portable which is not as secure.
        /// 
        /// Changing this scheme will cause Registration Keys to require
        /// re-entering passwords.
        /// </summary>
        public bool UseMachineEncryptionKeyForPasswords { get; set; }

        /// <summary>
        /// Timeout used on Statusbar messages
        /// </summary>
        public int StatusMessageTimeout { get; set; } = 6000;

        #endregion


        #region Folder Locations
        /// <summary>
        /// Last folder used when opening a document
        /// </summary>
        public string LastFolder { get; set; }

        /// <summary>
        /// Last location where an image was opened.
        /// </summary>
        public string LastImageFolder { get; set; }

	    

	    /// <summary>
	    /// Common folder where configuration files are stored. Can be moved
	    /// to an alternate location to allow sharing.
	    /// </summary>        
	    public string CommonFolder
	    {
		    get
		    {
			    if (_commonFolder == null)
				    _commonFolder = InternalCommonFolder;
				return _commonFolder;
		    }
		    set { _commonFolder = value; }
	    }
	    private string _commonFolder;

		internal string InternalCommonFolder { get; set; }

        internal string AddinsFolder => Path.Combine(CommonFolder, "Addins");

        /// <summary>
        /// Statusbar timeout length
        /// </summary>
        internal int StatusTimeout { get; set; } = 7000;

        #endregion

        public ApplicationConfiguration()
        {
            MarkdownOptions = new MarkdownOptionsConfiguration();
            WindowPosition = new WindowPositionConfiguration();
	        FolderBrowser = new FolderBrowserConfiguration();	        
            ApplicationUpdates = new ApplicationUpdatesConfiguration();
            OpenDocuments = new List<MarkdownDocument>();

            InternalCommonFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Markdown Monster");
            CommonFolder = InternalCommonFolder;
            LastFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            PreviewSyncMode = PreviewSyncMode.EditorToPreview;

            AutoSaveBackups = true;
            AutoSaveDocuments = false;

            RecentDocumentsLength = 12;
            RememberLastDocumentsLength = 3;
            
            
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
            
            EditorEnableSpellcheck = true;
            EditorDictionary = "EN_US";
            EditorKeyboardHandler = "default";  // vim,emacs

            DefaultCodeSyntax = "csharp";

            PreviewHttpLinksExternal = true;

            UseMachineEncryptionKeyForPasswords = true;
	        
			TerminalCommand = "powershell.exe";
			TerminalCommandArgs = "-noexit -command \"cd '{0}'\"";
            OpenFolderCommand = "explorer.exe";

            WebBrowserPreviewExecutable = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),@"Google\Chrome\Application\chrome.exe");
            
           ReportErrors = true;

            UseSingleWindow = true;

            IsPreviewVisible = true;
            OpenInPresentationMode = false;
            AlwaysUsePreviewRefresh = false;		
        }

        public void AddRecentFile(string filename)
        {
            if (string.IsNullOrEmpty(filename) || filename.ToLower() == "untitled")
                return;

            if (RecentDocuments.Contains(filename))
                RecentDocuments.Remove(filename);

            RecentDocuments.Insert(0,filename);
            OnPropertyChanged(nameof(RecentDocuments));

            if (RecentDocuments.Count > RecentDocumentsLength)
            {
                RecentDocuments.Clear();
                foreach (var recent in RecentDocuments.Take(RecentDocumentsLength))
                    RecentDocuments.Add(recent);
            }
        }


        
        #region Configuration Settings

        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var commonFolder = CommonFolder;
            var cfFile = Path.Combine(InternalCommonFolder, "CommonFolderLocation.txt");
            if (File.Exists(cfFile))
            {
                commonFolder = File.ReadAllText(cfFile);
                if (!Directory.Exists(commonFolder))
                {
                    commonFolder = InternalCommonFolder;
                    File.Delete(cfFile);
                }
            }
            if (string.IsNullOrWhiteSpace(commonFolder))
	        {
		        commonFolder = CommonFolder;
		        File.Delete(cfFile);
	        }

	        var provider = new JsonFileConfigurationProvider<ApplicationConfiguration>()
            {
                JsonConfigurationFile = Path.Combine(commonFolder,"MarkdownMonster.json")
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

        public override bool Write()
        {
            var commonFolderFile = Path.Combine(InternalCommonFolder, "CommonFolderLocation.txt");
            if (CommonFolder != InternalCommonFolder)
                File.WriteAllText(commonFolderFile, CommonFolder);
            else
                File.Delete(commonFolderFile);

            return base.Write();
        }

        /// <summary>
        /// Backs up configuration data to a backup file in the CommonFolder.
        /// Filename includes backup date and time
        /// </summary>
        /// <returns>
        /// Backup file name
        /// </returns>        
        public string Backup()
        {
            var filename = Path.Combine(CommonFolder, $"markdownmonster-backup-{DateTime.Now:MM-dd-yyyy-HH-mm-ss}.json");
            Write(filename);
            return filename;
        }

        /// <summary>
        /// Resets configuration settings by deleting the configuration file and
        /// then exits the application.
        /// </summary>
        public void Reset(bool noExit = false)
        {
            // now create new default config and write out then delete
            // file will be recreated on next restart
            File.Delete(Path.Combine(mmApp.Configuration.CommonFolder, "MarkdownMonster.json"));
            File.Delete(Path.Combine(mmApp.Configuration.CommonFolder, "CommonFolderLocation.txt"));

            try
            {
                Directory.Delete(mmApp.Configuration.AddinsFolder, true);
            }
            catch { }

            if (!noExit)
                Environment.Exit(0);
        }

        /// <summary>
        /// Writes the configuration data to the specified filename.
        /// </summary>
        /// <param name="filename"></param>
        public void Write(string filename)
        {
            string configData = WriteAsString();
            File.WriteAllText(filename, configData);
        }

        public override string WriteAsString()
        {
            return JsonConvert.SerializeObject(this,Formatting.Indented);
        }
        #endregion

        #region  INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;        
        
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

         
        #endregion

        
    }
    

      
    

    public enum MarkdownStyles
    {
        Common,
        GitHub
    }

    public enum PreviewModes
    {
        InternalPreview,
        ExternalPreviewWindow,
        None
    }

    public enum PreviewSyncMode
    {
        EditorToPreview,
        PreviewToEditor,
        EditorAndPreview,        
        None
    }

    public enum GitCommitBehaviors
    {
        Commit,
        CommitAndPush
    }
}
