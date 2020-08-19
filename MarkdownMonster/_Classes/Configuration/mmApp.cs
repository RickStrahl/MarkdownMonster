using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Westwind.Utilities;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using ControlzEx.Theming;
using MarkdownMonster.Utilities;
using MarkdownMonster.Windows;
using MarkdownMonster.Windows.ConfigurationEditor;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Win32;

namespace MarkdownMonster
{

    /// <summary>
    /// Application class for Markdown Monster that provides
    /// a global static placeholder for configuration and some
    /// utility functions
    /// </summary>
    public class mmApp
    {
        private const string mkbase = "a4dx23513TY69dE+533#1Ae@rTo*dO&-002";

        /// <summary>
        /// Holds a static instance of the application's configuration settings
        /// </summary>
        public static ApplicationConfiguration Configuration { get; set; }

        /// <summary>
        /// Holds a static instance of the Application Model
        /// </summary>
        public static AppModel Model { get; set; }


        /// <summary>
        /// A static class that holds singleton window references
        /// </summary>
        public static OpenWindows OpenWindows { get; set; } = new OpenWindows();

        /// <summary>
        /// The full name of the application displayed on toolbar and dialogs
        /// </summary>
        public static string ApplicationName { get; set; } = "Markdown Monster";

        public static DateTime Started { get; set; }

        public static string AllowedFileExtensions { get; } =
            ".md,.markdown,.mdown,.mkd,.mkdn,.mdcrypt,.txt,.htm,.html,.php,.cshtml,.txt,.yml,.xml,.json,.js,.ts,.dart,.css,.sass,.less,.ps1,.bat,.sh,.mak,.cs,.fs,.vb,.cpp,.sql,.prg,.config,.ini,.nuspec,";


        public static string NewLine => 
               Configuration.Editor.LinefeedMode == MarkdownMonster.Configuration.LinefeedModes.CrLf ? 
                    "\r\n" :
                    "\n";
    

        /// <summary>
        /// Returns a machine specific encryption key that can be used for passwords
        /// and other settings.
        ///
        /// If the Configuration.UseMachineEcryptionKeyForPasswords flag
        /// is false, no machine specific information is added to the key.
        /// Do this if you want to share your encrypted configuration settings
        /// in cloud based folders like DropBox, OneDrive, etc.
        /// </summary>
        public static string EncryptionMachineKey
        {
            get
            {
                if (!Configuration.UseMachineEncryptionKeyForPasswords)
                    return mkbase;

                return InternalMachineKey;
            }
        }

        /// <summary>
        /// Internal Machine Key which is a registry GUID value
        /// </summary>
        internal static string InternalMachineKey
        {
            get
            {
                if (_internalMachineKey != null)
                    return _internalMachineKey;

                var mmRegKey = @"SOFTWARE\West Wind Technologies\Markdown Monster";

                dynamic data;
                if (!mmWindowsUtils.TryGetRegistryKey(mmRegKey, "MachineKey", out data, UseCurrentUser: true))
                {
                    data = Guid.NewGuid().ToString();
                    var rk = Registry.CurrentUser.OpenSubKey(mmRegKey, true);

                    if (rk == null)
                        rk = Registry.CurrentUser.CreateSubKey(mmRegKey);

                    dynamic value = rk.GetValue("MachineKey");
                    if (value == null)
                        rk.SetValue("MachineKey", data, RegistryValueKind.String);

                    rk.Dispose();
                }

                if (data != null)
                    _internalMachineKey = data;

                return data as string;
            }
        }

        private static string _internalMachineKey = null;
		internal static string Signature { get; } = "S3VwdWFfMTAw";
		internal static string PostFix { get; set;  } = "*~~*";


        /// <summary>
        /// Application related Urls used throughout the application
        /// </summary>
        public static ApplicationUrls Urls { get; set; } = new ApplicationUrls();


	    #region Initialization and Shutdown


        /// <summary>
        /// Static constructor to initialize configuration
        /// </summary>
        static mmApp()
        {
            Configuration = new ApplicationConfiguration();
            Configuration.Initialize();
        }

        public static void ApplicationStart()
        {
            Started = DateTime.UtcNow;
	        Urls = new ApplicationUrls();

            InitializeLogging();
        }

        public static void Shutdown(bool errorShutdown = false)
        {
            ShutdownLogging();

            var tempPath = Path.GetTempPath();

            // Cleanup temp files
            File.Delete(Path.Combine(tempPath, MarkdownDocument.PREVIEW_HTML_FILENAME));
            FileUtils.DeleteTimedoutFiles(Path.Combine(tempPath, "mm_diff_*.*"), 1);
        }
        #endregion


        #region Error Handling, Logging and Telemetry

        private static TelemetryClient AppInsights;
        private static IOperationHolder<RequestTelemetry> AppRunTelemetry;

        /// <summary>
        /// Handles an Application level exception by logging the error
        /// to log, and displaying an error message to the user.
        /// Also sends the error to server if enabled.
        ///
        /// Returns true if application should continue, false to exit.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static bool HandleApplicationException(Exception ex, ApplicationErrorModes errorMode)
        {
            Log($"{errorMode}: " + ex.Message, ex, unhandledException: true, logLevel: LogLevels.Critical);

            var msg = $"Yikes! Something went wrong...\r\n\r\n{ex.Message}\r\n\r\n" +
                        "The error has been recorded and written to a log file and you can " +
                        "review the details or report the error via Help | Show Error Log\r\n\r\n" +
                        "Do you want to continue?";

            var res = MessageBox.Show(msg, ApplicationName + " Error",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error,MessageBoxResult.Yes);


            if (res.HasFlag(MessageBoxResult.No))
            {
                Shutdown(errorShutdown: true);
                return false;
            }
            return true;
        }

        public static void SetWorkingSet(int lnMaxSize, int lnMinSize)
        {
            try
            {
                Process loProcess = Process.GetCurrentProcess();
                loProcess.MaxWorkingSet = (IntPtr) lnMaxSize;
                loProcess.MinWorkingSet = (IntPtr) lnMinSize;
            }
            catch
            {
            }
        }

        /// <summary>
		/// Returns a fully qualified Help URL to a topic in the online
		/// documentation based on a topic id.
		/// </summary>
		/// <param name="topic">The topic id or topic .html file</param>
		/// <returns>Fully qualified URL</returns>
	    public static string GetDocumentionUrl(string topic)
	    {
		    if (!topic.Contains(".htm"))
			    topic += ".htm";

			return Urls.DocumentationBaseUrl + topic;
	    }

        #endregion

        #region Logging

        /// <summary>
        /// Starts the Application Insights logging functionality
        /// Note: this should be set on application startup once
        /// and will not fire multiple times.
        /// </summary>
        public static void InitializeLogging()
        {
            try
            {
                if (Configuration.SendTelemetry &&
                    Telemetry.UseApplicationInsights &&
                    AppInsights == null)
                {
                    AppInsights = new TelemetryClient
                    {
                        InstrumentationKey = Telemetry.Key
                    };
                    AppInsights.Context.Session.Id = Guid.NewGuid().ToString();
                    AppInsights.Context.Component.Version = GetVersion();

                    AppRunTelemetry =
                        AppInsights.StartOperation<RequestTelemetry>(
                            $"{GetVersion()} - {Configuration.ApplicationUpdates.AccessCount + 1} - {(UnlockKey.IsAppRegistered() ? "registered" : "unregistered")}");
                    AppRunTelemetry.Telemetry.Start();
                }
            }
            catch (Exception ex)
            {
                Telemetry.UseApplicationInsights = false;
                LogLocal("Application Insights initialization failure: " + ex.GetBaseException().Message);
            }
        }

        /// <summary>
        /// Shuts down the Application Insights Logging functionality
        /// and flushes any pending requests.
        ///
        /// This handles start and stop times and the application lifetime
        /// log entry that logs duration of operation.
        /// </summary>
        public static void ShutdownLogging()
        {
            if (Configuration.SendTelemetry &&
                Telemetry.UseApplicationInsights &&
                AppInsights != null)
            {
                var t = AppRunTelemetry.Telemetry;

                // multi-instance shutdown - ignore
                if (t.Properties.ContainsKey("usage"))
                {
                    return;
                }

                t.Properties.Add("usage", Configuration.ApplicationUpdates.AccessCount.ToString());
                t.Properties.Add("registered", UnlockKey.IsAppRegistered().ToString());
                t.Properties.Add("version", GetVersion());
                t.Properties.Add("dotnetversion", MarkdownMonster.Utilities.mmWindowsUtils.GetDotnetVersion());
                t.Properties.Add("culture", CultureInfo.CurrentUICulture.IetfLanguageTag);
                t.Stop();

                try
                {
                    AppInsights.StopOperation(AppRunTelemetry);
                }
                catch (Exception ex)
                {
                    LogLocal("Failed to Stop Telemetry Client: " + ex.GetBaseException().Message);
                }

                AppInsights.Flush();
                AppInsights = null;                
                AppRunTelemetry.Dispose();
            }          
        }

        /// <summary>
        /// Logs exceptions in the applications
        /// </summary>
        /// <param name="ex"></param>
        public static void Log(Exception ex, LogLevels logLevel = LogLevels.Error)
        {
            Log(ex.GetBaseException().Message, ex, logLevel: logLevel);
        }

        /// <summary>
        /// Logs messages to the standard log output for Markdown Monster:
        /// 
        /// * Application Insights
        /// * Local Log File
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static void Log(string msg, Exception ex = null, bool unhandledException = false, LogLevels logLevel = LogLevels.Error)
        {
            string version = GetVersion();
            string winVersion = null;
            
            if (Telemetry.UseApplicationInsights && AppRunTelemetry?.Telemetry != null)
            {
                var secs = (int) DateTimeOffset.UtcNow.Subtract(AppRunTelemetry.Telemetry.Timestamp).TotalSeconds;

                if (ex != null)
                {
                    AppRunTelemetry.Telemetry.Success = false;
                    AppInsights.TrackException(ex,
                        new Dictionary<string, string>
                        {
                            {"msg", msg},
                            {"exmsg", ex.Message},
                            {"exbasemsg", ex.GetBaseException().Message},
                            {"exsource", ex.Source},
                            {"extrace", ex.StackTrace},
                            {"severity", unhandledException ? "unhandled" : ""},
                            {"version", version},
                            {"winversion", winVersion},
                            {"dotnetversion", MarkdownMonster.Utilities.mmWindowsUtils.GetDotnetVersion()},
                            {"usage", Configuration.ApplicationUpdates.AccessCount.ToString()},
                            {"registered", UnlockKey.IsAppRegistered().ToString()},
                            {"culture", CultureInfo.CurrentCulture.IetfLanguageTag},
                            {"uiculture", CultureInfo.CurrentUICulture.IetfLanguageTag},
                            {"seconds", secs.ToString() },
                            {"level", ((int) logLevel).ToString() + " - " + logLevel.ToString()}
                        });

                }
                else
                {
                    // message only
                    var props = new Dictionary<string, string>()
                    {
                        {"msg", msg},
                        {"usage", Configuration.ApplicationUpdates.AccessCount.ToString()},
                        {"registered", UnlockKey.IsAppRegistered().ToString()},
                        {"version", GetVersion()},
                        {"culture", CultureInfo.CurrentCulture.IetfLanguageTag},
                        {"uiculture", CultureInfo.CurrentUICulture.IetfLanguageTag},
                        {"seconds", secs.ToString() },
                        {"level", ((int) logLevel).ToString() + " - " + logLevel.ToString() }
                    };
                    AppInsights.TrackTrace(msg, props);                    
                }
            }
            
            // also log to the local error log
            LogLocal(msg,ex);            
        }

        /// <summary>
        /// Writes a trace message
        /// </summary>
        /// <param name="msg"></param>
        public static void LogTrace(string msg, LogLevels logLevel = LogLevels.Trace)
        {
            Log(msg, logLevel: logLevel);
        }

        /// <summary>
        /// Logs an information message
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        public static void LogInfo(string msg, LogLevels logLevel = LogLevels.Information)
        {
            Log(msg, logLevel: logLevel);
        }

        /// <summary>
        /// This method logs only to the local file, not to
        /// the online telemetry. Use primarily for informational
        /// messages and errors.
        /// </summary>
        /// <param name="msg">Optional message</param>
        /// <param name="ex"></param>
        public static void LogLocal(string msg = null, Exception ex = null)
        {
            if (msg == null && ex == null)
                return;

            string exMsg = GetExceptionMessageForLog(ex);

            if (!string.IsNullOrEmpty(msg))
                msg += "\r\n";

            StringUtils.LogString(msg + exMsg + "\r\n\r\n",
                Path.Combine(Configuration.CommonFolder,"MarkdownMonsterErrors.txt"),
                Encoding.UTF8);
        }

        private static string GetExceptionMessageForLog(Exception ex)
        {
            string exMsg = string.Empty;
            if (ex != null)
            {
                string version = GetVersion();
                string winVersion;
                winVersion = MarkdownMonster.Utilities.mmWindowsUtils.GetWindowsVersion() +
                             " - " + CultureInfo.CurrentUICulture.IetfLanguageTag +
                             " - NET " + MarkdownMonster.Utilities.mmWindowsUtils.GetDotnetVersion() + " - " +
                             (Environment.Is64BitProcess ? "64 bit" : "32 bit");

                ex = ex.GetBaseException();
                exMsg = $@"{ex.Message}
Markdown Monster v{version}
{winVersion}
{CultureInfo.CurrentCulture.IetfLanguageTag} - {CultureInfo.CurrentUICulture.IetfLanguageTag}    
---
{ex.Source}
{ex.StackTrace}          
---------------------------
";
            }

            return exMsg;
        }

        #endregion

        #region Version information

        /// <summary>
        /// Gets the Markdown Monster Version as a string
        /// </summary>
        /// <returns></returns>
        public static string GetVersion()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return v.ToString();
        }


        /// <summary>
        /// Compare two version strings.
        /// </summary>
        /// <param name="versionToCompare">Semantic Version string</param>
        /// <param name="versionToCompareAgainst">Semantic Version string</param>
        /// <returns>0 - equal, 1 - greater than compareAgainst,  -1 - smaller than, -2  - Version Format error </returns>
        public static int CompareVersions(string versionToCompare, string versionToCompareAgainst)
        {
            try
            {
                var v1 = new Version(versionToCompare);
                var v2 = new Version(versionToCompareAgainst);
                return v1.CompareTo(v2);
            }
            catch
            {
                return -2;
            }
        }

        /// <summary>
        /// Returns a formatted string value for the version.
        /// </summary>
        /// <returns></returns>
        public static string GetVersionForDisplay(string version = null)
        {
            if (version == null)
                version = GetVersion();

            if (version.EndsWith(".0.0"))
                version = version.Substring(0, version.Length - 4);
            else if (version.EndsWith(".0"))
                version = version.Substring(0, version.Length - 2);
            
            return version;
        }

        /// <summary>
        /// Return the write date for the Main MM application executable
        /// </summary>
        /// <returns></returns>
        public static string GetVersionDate()
        {
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            return fi.LastWriteTime.ToString("MMM d, yyyy");
        }

        #endregion

        #region encryption, decryption
        /// <summary>
        /// Encrypts sensitive user data using an internally generated
        /// encryption key.
        /// </summary>
        /// <param name="value">Value to encrypt</param>
        /// <param name="dontUseMachineKey">
        /// In shared cloud drive situations you might want to not use a machine key
        /// The default uses the UseMachineKeyForPasswords configuration setting.
        /// </param>
        /// <returns></returns>
        public static string EncryptString(string value, bool? dontUseMachineKey = null)
        {
            if (dontUseMachineKey == null)
                dontUseMachineKey = !mmApp.Configuration.UseMachineEncryptionKeyForPasswords;

            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // already encrypted
            if (value.EndsWith(PostFix))
                return value;

            var key = mkbase;
            if (!dontUseMachineKey.Value)
                key = mmApp.EncryptionMachineKey;

            var encrypted = Encryption.EncryptString(value, key) + PostFix;
            return encrypted;
        }

        /// <summary>
        /// Decrypts a string encrypted with EncryptString()
        /// </summary>
        /// <param name="encrypted">pass in the encrypted string</param>
        /// <param name="dontUseMachineKey"></param>
        /// <returns></returns>
        public static string DecryptString(string encrypted, bool? dontUseMachineKey = null)
        {
            if (dontUseMachineKey == null)
                dontUseMachineKey = !mmApp.Configuration.UseMachineEncryptionKeyForPasswords;

            if (string.IsNullOrEmpty(encrypted))
                return string.Empty;

            if (!encrypted.EndsWith(PostFix))
                return encrypted;

            encrypted = encrypted.Replace(PostFix, "");

            var key = mkbase;
            if (!dontUseMachineKey.Value)
                key = mmApp.EncryptionMachineKey;

            var decoded = Encryption.DecryptString(encrypted, key);
            return decoded;
        }
        #endregion

        #region Themes

        /// <summary>
        /// Sets the light or dark theme for a form. Call before
        /// InitializeComponents().
        ///
        /// We only support the dark theme now so this no longer relevant
        /// but left in place in case we decide to support other themes.
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="window"></param>
        public static void SetTheme(Themes theme = Themes.Default, MetroWindow window = null)
        {
            if (theme == Themes.Default)
                theme = mmApp.Configuration.ApplicationTheme;

            if (theme == Themes.Light)
            {
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, "Light.Blue");
            }
            else
            {
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, "Dark.Blue");
            }

            if (window != null)
                SetThemeWindowOverride(window);

        }

        /// <summary>
        /// Overrides specific colors in the active theme depending on the currently active theme.
        /// Use this in every Metro Window form to apply base window styling. Affects:
        ///
        /// * Title Brush, Active Title Brush
        /// * Window Border
        /// </summary>
        /// <param name="window">MetroWindow to apply theme adjustments on</param>
        /// <param name="isMainWindow">Is this the main application window (not used)</param>
        public static void SetThemeWindowOverride(MetroWindow window, bool isMainWindow = false)
        {
            if (window == null)
                return;

            if (Configuration.ApplicationTheme == Themes.Dark)
            {
                window.WindowTitleBrush = (SolidColorBrush) new BrushConverter().ConvertFrom("#333");
                window.NonActiveWindowTitleBrush = (SolidColorBrush) new BrushConverter().ConvertFrom("#444");
                window.BorderBrush = (SolidColorBrush) new BrushConverter().ConvertFrom("#494949");
            }
            else
            {
                // Light theme
                window.WindowTitleBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#CBD4EF");
                window.NonActiveWindowTitleBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#CBD4EF"));
            }
        }
        #endregion
    }

    public enum LogLevels
    {
        Error = 4,
        Critical = 5,        
        Warning =3,
        Information = 2,       
        Debug = 1,
        Trace = 0
    }


    /// <summary>
    /// Supported themes (not used any more)
    /// </summary>
    public enum Themes
    {
        Dark,
        Light,
        Default
    }

	/// <summary>
	/// Urls that are associated with the application
	/// </summary>
	public class ApplicationUrls
	{
		/// <summary>
		/// The URL where new versions are downloaded from
		/// </summary>
		public string InstallerDownloadUrl { get; internal set; } =
			"https://markdownmonster.west-wind.com/download.aspx";


		/// <summary>
		/// Url to go to purchase a registered version of Markdown Monster
		/// </summary>
		public string RegistrationUrl { get; internal set; } =
            "http://markdownmonster.west-wind.com/purchase.aspx";
			//"https://store.west-wind.com/product/order/markdown_monster";


		/// <summary>
		/// Base Url where documentation is found. Add just 'topicid.htm'
		/// </summary>
		public string DocumentationBaseUrl { get; internal set; } =
			"https://markdownmonster.west-wind.com/docs/";


		/// <summary>
		/// Web site home url
		/// </summary>
		public string WebSiteUrl { get; internal set; } =
			"http://markdownmonster.west-wind.com";

		/// <summary>
		/// Url to the Github repo for support and enhancement requests
		/// </summary>
		public string SupportUrl { get; set; } =
			"https://github.com/RickStrahl/MarkdownMonster/issues";

		/// <summary>
		/// Url that is checked for new version
		/// </summary>
		public string VersionCheckUrl { get; set; } =
			"http://west-wind.com/files/MarkdownMonster_version.xml";

		public string AddinRepositoryUrl { get; set; } =
			"https://raw.githubusercontent.com/RickStrahl/MarkdownMonsterAddinsRegistry/master/MarkdownMonsterAddinRegistry.json";
	}

	/// <summary>
	/// Message class that holds information about a bug report
	/// for logging and telemetry reporting
	/// </summary>
	public class BugReport
    {
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
        public string Product { get; set; }
        public string Version { get; set; }
        public string WinVersion { get; set; }
        public string StackTrace { get; set; }

    }

    /// <summary>
    /// Holds telemetry information sent to server for telemetry
    /// reports. Used only for custom telemetry not AppInsights.
    /// </summary>
    public class Telemetry
    {
        public string Version { get; set; }
        public bool Registered { get; set; }
        public string Operation { get; set; }
        public string Data { get; set; }
        public int Access { get; set; }
        public int Time { get; set; }

        public static string Key { get; } = "c73daa21-a2dd-42ae-9a2f-2e7c17b83706";
        public static bool UseApplicationInsights { get; set; } = true;
    }

    public enum ApplicationErrorModes
    {
        AppDispatcher,
        AppRoot
    }

    public class OpenWindows
    {
        public ConfigurationEditorWindow ConfigurationEditor { get; set; }
    }
}
