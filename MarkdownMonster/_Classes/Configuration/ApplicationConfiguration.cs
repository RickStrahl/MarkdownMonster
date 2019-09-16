using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using MarkdownMonster.Configuration;
using Newtonsoft.Json;
using Westwind.Utilities.Configuration;
using MarkdownMonster.Annotations;
using Westwind.Utilities;


namespace MarkdownMonster
{

    /// <summary>
    /// Application level configuration for Markdown Monster
    /// </summary>
    public class ApplicationConfiguration : AppConfiguration,
                                            INotifyPropertyChanged
    {
        /// <summary>
        /// Dark or Light overall application theme selection for Markdown Monster
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


        /// <summary>
        /// If set this property controls whether the editor is opened in
        /// presentation mode which shows only the preview pane with the editor
        /// collapsed.
        /// </summary>
        public bool OpenInPresentationMode { get; set; }

        /// <summary>
        /// Determines whether Markdown Monster runs as a Singleton application.
        /// If true only a single instance runs and parameters are forwarded to
        /// open in the single instance.
        /// </summary>
        public bool UseSingleWindow { get; set; }


        /// <summary>
        /// The theme used for the editor. Can be any of the available AceEditor
        /// themes which include twilight, vscodedark, vscodelight, visualstudio,
        /// github, monokai etc. Themes available based on files in:
        /// 
        /// \Editor\scripts\Ace\theme-XXXX.js
        ///
        /// You can create additional editor themes by copying and modifying existing
        /// editor themes and using the same naming convention.
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
        /// Editor Configuration Sub-Settings
        /// </summary>
        public EditorConfiguration Editor { get; set; }


        /// <summary>
        /// Themes used to render the Preview. Preview themes are
        /// located in the following folder:
        ///
        /// \PreviewThemes\XXXX
        ///
        /// You can create additional Preview Themes by copying an existing
        /// theme folder and modifying the CSS and HTML templates as needed.
        /// </summary>
        public string PreviewTheme
        {
            get { return _previewTheme; }
            set
            {
                if (value == _previewTheme) return;
                _previewTheme = value;
                OnPropertyChanged(nameof(PreviewTheme));
            }
        }
        private string _previewTheme;

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
        /// Determines whether links are embedded as reference links
        /// at the bottom of the current document rather than explicit links
        /// </summary>
        public bool UseReferenceLinks
        {
            get => _useReferenceLinks;
            set
            {
                if (value == _useReferenceLinks) return;
                _useReferenceLinks = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Determines whether the full path for the open
        /// document is displayed in the Main Window's
        /// title bar.
        /// </summary>
        public bool ShowFullDocPathInTitlebar { get; set; }

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
        private string _DistractionFreeModeHideOptions = "toolbar,statusbar,menu,preview,tabs,maximized,maxwidth:1000";

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
        /// Default code syntax displayed in the Paste Code dialog
        /// </summary>
        public string DefaultCodeSyntax { get; set; }

        /// <summary>
        /// Web Browser Preview Executable - use this to force
        /// a specific browser since Windows seems to not be able
        /// to maintain a proper association long term.
        ///
        /// If not set or path doesn't exist, uses Windows default
        /// configuration.
        /// </summary>
        public string WebBrowserPreviewExecutable { get; set; }

        public ApplicationConfiguration()
        {
            Editor = new EditorConfiguration();
            Git = new GitConfiguration();
            Images = new ImagesConfiguration();

            MarkdownOptions = new MarkdownOptionsConfiguration();
            WindowPosition = new WindowPositionConfiguration();
            FolderBrowser = new FolderBrowserConfiguration();
            ApplicationUpdates = new ApplicationUpdatesConfiguration();
            OpenDocuments = new List<OpenFileDocument>();


            InternalCommonFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Markdown Monster");
            CommonFolder = InternalCommonFolder;
            LastFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            PreviewSyncMode = PreviewSyncMode.EditorToPreview;

            AutoSaveBackups = true;
            AutoSaveDocuments = false;

            RecentDocumentsLength = 10;
            RememberLastDocumentsLength = 5;


            //BugReportUrl = "https://markdownmonster.west-wind.com/bugreport/bugreport.ashx?method=ReportBug";
            //BugReportUrl = "http://localhost.fiddler/MarkdownMonster/bugreport/bugreport.ashx?method=ReportBug";
            //TelemetryUrl = "https://markdownmonster.west-wind.com/bugreport/bugreport.ashx?method=Telemetry";
            SendTelemetry = true;

            ApplicationTheme = Themes.Dark;
            PreviewTheme = "Dharkan";
            EditorTheme = "vscodedark";

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
            IsDocumentOutlineVisible = true;
            OpenInPresentationMode = false;
            AlwaysUsePreviewRefresh = false;

            // Disable for better stability and compatibility
            // We're not doing anything that pushes the hardware to bring benefits
            DisableHardwareAcceleration = true;
        }


        #region Document Operations

        /// <summary>
        /// Command Processing for OpenFolder
        /// </summary>
        public string OpenFolderCommand { get; set;  }

        /// <summary>
        /// Command Processing Executable to bring up a terminal window
        /// Command or Powershell, but could also be Console or ConEmu
        /// cmd.exe            /k \"cd {0}\"
        /// powershell.exe     -NoExit -Command  "&amp; cd 'c:\program files'"
        /// wt.exe             (leave blank) &amp; set Profile "startingDirectory" : "%__CD__%",
        /// </summary>
        public string TerminalCommand { get; set; }

		/// <summary>
		/// Terminal executable arguments to pass to bring up terminal
		/// in a specific folder. {0} represents folder name.
		/// </summary>
	    public string TerminalCommandArgs { get; set; }


        /// <summary>
        /// A collection of the open Markdown documents.
        /// </summary>
        public List<OpenFileDocument> OpenDocuments { get; set; }


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
                //OnPropertyChanged(nameof(RecentDocumentList));
            }
        }
        private ObservableCollection<string> _recentDocuments = new ObservableCollection<string>();


        ///// <summary>
        ///// Internal property used to display the recent document list
        ///// </summary>
        //[JsonIgnore]
        //public ObservableCollection<RecentDocumentListItem> RecentDocumentList
        //{
        //    get
        //    {
        //        var list = RecentDocuments.Take(10);
        //        var docs = new ObservableCollection<RecentDocumentListItem>();
        //        foreach (var doc in list)
        //        {
        //            docs.Add(new RecentDocumentListItem
        //            {
        //                Filename = doc,
        //                DisplayFilename = FileUtils.GetCompactPath(doc, 70)
        //            });
        //        }
        //        return docs;
        //    }
        //}


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

        public ImagesConfiguration Images { get; set; }

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
	        { "kavadocs", "json"},
            { "mdproj", "json" },

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
	        {"user", "xml" },
	        {"dotsettings", "xml" },
            {"csproj", "xml"},
            {"sln", "xml" },
		    {"nuspec", "xml"},

		    {"yaml", "yaml"},
	        {"yml", "yaml"},
	        {"diff", "diff" },
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


        /// <summary>
        /// Configuration Settings for Git Integration
        /// </summary>
        public GitConfiguration Git { get; set; }


        #endregion


        #region Bug Reporting and Telemetry

        /// <summary>
        /// Determines whether errors are reported anonymously
        /// </summary>
        public bool ReportErrors { get; set; }


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
        /// Determines if the Document Outline sidebar is visible.
        /// </summary>
        public bool IsDocumentOutlineVisible
        {
            get { return _IsDocumentOutlineVisible; }
            set
            {
                if (value == _IsDocumentOutlineVisible) return;
                _IsDocumentOutlineVisible = value;
                OnPropertyChanged(nameof(IsDocumentOutlineVisible));
            }
        }
        private bool _IsDocumentOutlineVisible;


        /// <summary>
        /// Maximum outline level that is rendered based on H1,H2,H3 etc. tags. Default is 4.
        /// </summary>
        public int MaxDocumentOutlineLevel
        {
            get => _maxOutlineLevel;
            set
            {
                if (value == _maxOutlineLevel) return;
                if (value > 6)
                    value = 6;
                if (value < 1)
                    value = 1;
                _maxOutlineLevel = value;
                OnPropertyChanged(nameof(_maxOutlineLevel));
            }
        }
        private int _maxOutlineLevel = 4;


        /// <summary>
        /// Remembers last Is link External setting when embedding links
        /// </summary>
        public bool LastLinkExternal { get; set; }

        /// <summary>
        /// Disables all addins from loading
        /// </summary>
        public bool DisableAddins { get; set; }

        /// <summary>
        /// Starts up the application without showing the Splash screen.
        /// </summary>
        public bool DisableSplashScreen { get; set; }

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
        public int StatusMessageTimeout { get; set; } = 8000;

        #endregion


        #region Folder Locations
        /// <summary>
        /// Last folder used when opening a document
        /// </summary>
        public string LastFolder { get; set; }



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
        private bool _useReferenceLinks;

        internal string InternalCommonFolder { get; set; }

        internal string AddinsFolder => Path.Combine(CommonFolder, "Addins");
    

        #endregion


        #region RecentFiles

        public void AddRecentFile(string filename)
        {
            if (string.IsNullOrEmpty(filename) )   return;
            
            var justFile = Path.GetFileName(filename);

            if (filename.Equals("untitled", StringComparison.InvariantCultureIgnoreCase) ||
                justFile.StartsWith("__") )
                return;

            if (!File.Exists(filename))
                return;

            if (RecentDocuments.Contains(filename))
                RecentDocuments.Remove(filename);

            RecentDocuments.Insert(0,filename);
            OnPropertyChanged(nameof(RecentDocuments));

            if (RecentDocuments.Count > RecentDocumentsLength)
            {
                // the hard way to force collection to properly refresh so bindings work properly
                var recents = RecentDocuments.Take(RecentDocumentsLength).ToList();
                RecentDocuments.Clear();
                foreach (var recent in recents)
                    RecentDocuments.Add(recent);
            }
        }

        /// <summary>
        /// Removes missing files and folders from the recent lists
        /// </summary>
        public void CleanupRecentFilesAndFolders()
        {
            var missing = new List<string>();
            foreach (var file in RecentDocuments)
            {
                if (!File.Exists(file))
                    missing.Add(file);
            }
            foreach (var file in missing)
                RecentDocuments.Remove(file);

            missing.Clear();
            foreach (var dir in FolderBrowser.RecentFolders)
            {
                if (!Directory.Exists(dir))
                    missing.Add(dir);
            }

            foreach (var dir in missing)
                FolderBrowser.RecentFolders.Remove(dir);
        }
        #endregion


        #region Configuration Settings

        protected override void OnInitialize(IConfigurationProvider provider, string sectionName, object configData)
        {
            base.OnInitialize(provider, sectionName, configData);
            mmApp.Configuration = this;

            // Make sure we have a valid config directory!!!
            if (!Directory.Exists(CommonFolder))
                CommonFolder = FindCommonFolder();

            if (string.IsNullOrEmpty(Git.GitClientExecutable))
                Git.GitClientExecutable = mmFileUtils.FindGitClient();
            if (string.IsNullOrEmpty(Git.GitDiffExecutable))
                Git.GitDiffExecutable = mmFileUtils.FindGitDiffTool();

            if (string.IsNullOrEmpty(Images.ImageEditor))
                Images.ImageEditor = mmFileUtils.FindImageEditor();


            CleanupRecentFilesAndFolders();

            // TODO: Remove in future version - added in 1.11
            // fix up new dictionary files
            var dict = Editor.Dictionary.ToLower();
            switch (dict)
            {
                case "en_us":
                    Editor.Dictionary = "en-US";
                    break;
                case "de_de":
                    Editor.Dictionary = "de";
                    break;
                case "fr_fr":
                    Editor.Dictionary = "fr";
                    break;
                case "es_es":
                    Editor.Dictionary = "es";
                    break;
            }
        }

        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            CommonFolder = FindCommonFolder();

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

        public override bool Write()
        {
            if (!App.IsPortableMode)
            {
                var commonFolderFile = Path.Combine(InternalCommonFolder, "CommonFolderLocation.txt");
                if (CommonFolder != InternalCommonFolder)
                    File.WriteAllText(commonFolderFile, CommonFolder);
                else
                {
                    if (File.Exists(commonFolderFile))
                        File.Delete(commonFolderFile);
                }
            }

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
        public void Reset(bool noExit = false, bool restart = false)
        {
            // now create new default config and write out then delete
            // file will be recreated on next restart
            File.Delete(Path.Combine(mmApp.Configuration.CommonFolder, "MarkdownMonster.json"));
            File.Delete(Path.Combine(mmApp.Configuration.CommonFolder, "CommonFolderLocation.txt"));

            try
            {
                // remove addins
                Directory.Delete(mmApp.Configuration.AddinsFolder, true);
            }
            catch { }



            if (!noExit)
            {
                if (mmApp.Model?.Window != null)
                    mmApp.Model.Window.Close();
                else
                    Environment.Exit(0);
            }

            if (restart)
                ShellUtils.ExecuteProcess(Path.Combine(App.InitialStartDirectory, "MarkdownMonster.exe"));


        }

        /// <summary>
        /// Writes the configuration data to the specified filename.
        /// </summary>
        /// <param name="filename"></param>
        public void Write(string filename)
        {
            string configData = WriteAsString();
            var path = Path.GetDirectoryName(filename);

            if (Directory.Exists(path))
                Directory.CreateDirectory(path);

            File.WriteAllText(filename, configData);
        }

        public override string WriteAsString()
        {
            return JsonConvert.SerializeObject(this,Formatting.Indented);
        }
        #endregion


        #region  INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Sets the CommonFolder for finding configuration settings and
        /// Addins etc.
        /// </summary>
        /// <returns></returns>
        protected string FindCommonFolder()
        {
            string commonFolderToSet = null;
            string workFolder = null;

            // Check for Portable Installation
            var cfFile = Path.Combine(App.InitialStartDirectory, "_IsPortable");
            if (File.Exists(cfFile))
            {
                // Use PortableSettings in Install Folder
                workFolder = Path.Combine(App.InitialStartDirectory, "PortableSettings");

                if (Directory.Exists(workFolder))
                {
                    InternalCommonFolder = workFolder;
                    App.IsPortableMode = true;
                    return workFolder;
                }

                if (LanguageUtils.IgnoreErrors(() => { Directory.CreateDirectory(workFolder); }))
                {
                    InternalCommonFolder = workFolder;
                    App.IsPortableMode = true;

                    // try to unblock DLLs downloaded from Internet via Zip file
                    UnblockDlls();

                    return workFolder;
                }
            }

            // Check for Common Folder override
            cfFile = Path.Combine(InternalCommonFolder, "CommonFolderLocation.txt");
            bool cflExists = File.Exists(cfFile);

            if (cflExists)
            {
                commonFolderToSet = File.ReadAllText(cfFile);
                if (!Directory.Exists(commonFolderToSet))
                {
                    commonFolderToSet = InternalCommonFolder;
                    File.Delete(cfFile);
                }
            }
            else
                commonFolderToSet = CommonFolder;

            if (!Directory.Exists(commonFolderToSet))
                commonFolderToSet = InternalCommonFolder;

            return commonFolderToSet;
        }
        #endregion

        #region UnblockDlls

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        /// <summary>
        /// Unblocks DLLS that are downloaded from the Internet either directly
        /// or directly installed from an Internet Downloaded Zip file
        /// </summary>
        private void UnblockDlls()
        {            
            foreach (var dir in Directory.GetDirectories(Path.Combine(App.InitialStartDirectory, "Addins")))
            {
                foreach (var filename in Directory.GetFiles(dir,"*.dll"))
                {
                    if (File.Exists(filename))
                        DeleteFile(filename + ":Zone.Identifier"); // API call never throws/only false result
                }
            }
        }

        #endregion
    }

    
    public class OpenFileDocument
    {

        public OpenFileDocument()
        {

        }

        public OpenFileDocument(MarkdownDocument doc)
        {
            if (doc != null)
            {
                Filename = doc.Filename;
                IsActive = doc.IsActive;
                LastEditorLineNumber = doc.LastEditorLineNumber;
                LastImageFolder = doc.LastImageFolder;
            };
            
        }

        public string Filename { get; set; }
        public bool IsActive { get; set; }

        public int LastEditorLineNumber { get; set; }
        public string LastImageFolder { get; set; }
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
