using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Westwind.Utilities;
using Westwind.Utilities.InternetTools;

namespace MarkdownMonster
{

    /// <summary>
    /// Checks for new versions and allows downloading of the latest version
    /// and installation.
    /// </summary>
    public class ApplicationUpdater
    {
        /// <summary>
        /// Version info captured by NewVersionAvailable
        /// </summary>
        public VersionInfo VersionInfo { get; set; }

        /// <summary>
        /// The current version we're checking for updates
        /// </summary>
        public string CurrentVersion { get; set; }

        /// <summary>
        /// The local file that identifies the local version
        /// </summary>
        public string VersionFile { get; set; }

        /// <summary>
        /// The URL on a remote server HTTP link that contains 
        /// the Version XML with the VersionInfo data
        /// </summary>
        public string VersionCheckUrl { get; set; }

        /// <summary>
        /// The URL from which the installer is downloaded
        /// </summary>
        public string DownloadUrl { get; set; }
        
        /// <summary>
        /// Determines where the updated version is downloaded to
        /// </summary>
        public string DownloadStoragePath { get; set; }

        /// <summary>
        /// How frequently to check for updates
        /// </summary>
        public int CheckDays { get; set; }

        /// <summary>
        /// Last time updates were checked for
        /// </summary>
        public DateTime LastCheck { get; set; }


        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Overload that requires a semantic versioning number
        /// as a string (0.56 or 9.44.44321)
        /// </summary>
        /// <param name="currentVersion"></param>
        public ApplicationUpdater(string currentVersion)
        {
            VersionInfo = new VersionInfo();
            CurrentVersion = currentVersion;
            Initialize();
        }

        /// <summary>
        /// Overload that requires a semantic versioning number
        /// as a string (0.56 or 9.44.44321)
        /// </summary>
        /// <param name="version"></param>
        public ApplicationUpdater(Version version)
        {
            CurrentVersion = version.ToString(); // GetVersionStringFromVersion(version);
            Initialize();
        }


        /// <summary>
        /// Overload that accepts a type from an assembly that holds
        /// version information
        /// </summary>
        /// <param name="assemblyType"></param>
        public ApplicationUpdater(Type assemblyType)
        {
            VersionInfo = new VersionInfo();
            var version = assemblyType.Assembly.GetName().Version;
            CurrentVersion = version.ToString();
            Initialize();
        }

        /// <summary>
        /// Do all operation that checks for new version, brings up the change
        /// dialog, allows downloading etc. UI can just call this method to
        /// do it all.
        /// </summary>
        /// <param name="force">Forces the version check even if it was done recently. Otherwise LastChecked and Interval is used to decide if to hit the server</param>
        /// <param name="closeApplication">It trye forces MM to be shutdown after downloading</param>
        /// <param name="failTimeout">Max time to for the HTTP check to take before considering failed</param>
        /// <returns></returns>
        public static bool  CheckForNewVersion(bool force, bool closeApplication = true, int failTimeout = 2000)
        {
            
            var updater = new ApplicationUpdater(typeof(MainWindow));
            bool isNewVersion = updater.IsNewVersionAvailable(!force, timeout: failTimeout);
            if (isNewVersion)
            {
                var res = MessageBox.Show(updater.VersionInfo.Detail + "\r\n\r\n" +
                                          "Do you want to download and install this version?",
                    updater.VersionInfo.Title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        ShellUtils.GoUrl(mmApp.Urls.InstallerDownloadUrl);
                    }
                    catch (Exception ex)
                    {
                        mmApp.Log("Couldn't access update Url: " + mmApp.Urls.InstallerDownloadUrl, ex, false,
                            logLevel: LogLevels.Error);

                        mmApp.Model.Window.ShowStatusError(
                            $"Unable to access update url: {mmApp.Urls.InstallerDownloadUrl}. {ex.Message}");
                    }

                    if (closeApplication)
                        mmApp.Model.Window.Close();
                }
            }
            

            return isNewVersion;
        }

        private void Initialize()
        {
            VersionFile = mmApp.Configuration.CommonFolder + "MarkdownMonster_Version.xml";
            DownloadStoragePath = Path.Combine(KnownFolders.GetDefaultPath(KnownFolder.Downloads), "MarkDownMonsterSetup.exe");

            VersionCheckUrl = mmApp.Urls.VersionCheckUrl;            
            CheckDays = mmApp.Configuration.ApplicationUpdates.UpdateFrequency;
            LastCheck = mmApp.Configuration.ApplicationUpdates.LastUpdateCheck;
        }

        /// <summary>
        /// This is the do it all function that checks for a new version
        /// downloads if it doesn't exist and immediately executes it.
        /// </summary>
        public bool CheckDownloadExecute(bool executeImmediately = true)
        {
            if (!IsNewVersionAvailable())
                return true;

            if (!Download())
                return false;

            if (!executeImmediately)
                return true;

            return ExecuteDownloadedFile();
        }


        /// <summary>
        /// Checks to see if a new version is available at the 
        /// VersionCheckUrl
        /// </summary>
        /// <param name="checkDate"></param>
        /// <returns></returns>
        public bool IsNewVersionAvailable(bool checkDate = false, int timeout = 1500)
        {
            if (checkDate)
            {
                if (LastCheck.Date > DateTime.UtcNow.Date.AddDays(CheckDays * -1))
                    return false;
            }

            string xml = null;
            try
            {
                xml = HttpUtils.HttpRequestString(new HttpRequestSettings
                {
                    Url = VersionCheckUrl, 
                    Timeout = timeout,
                    HttpVerb = "GET"
                });
            }
            catch
            {
                // fail silently if no connection or invalid url
                return false;
            }

            mmApp.Configuration.ApplicationUpdates.LastUpdateCheck = DateTime.UtcNow.Date;
            mmApp.Configuration.Write();

            if (!string.IsNullOrEmpty(xml))
            {
                var ver = SerializationUtils.DeSerializeObject(xml, typeof(VersionInfo)) as VersionInfo;
                if (ver != null)
                {
                    VersionInfo = ver;

                    var curVer = new Version(CurrentVersion);                    
                    var onlineVer = new Version(ver.Version);

                    // Strip off revisions
                    ReflectionUtils.SetField(curVer, "_Revision", 0);
                    ReflectionUtils.SetField(onlineVer, "_Revision", 0);

                    if (onlineVer.CompareTo(curVer) > 0)
                        return true;
                }

                LastCheck = DateTime.UtcNow;
            }

            return false;
        }

        /// <summary>
        /// Downloads the update exe
        /// </summary>
        /// <returns></returns>
        public bool Download()
        {            
            try
            {
                var http = new HttpClient();
                if (!http.DownloadFile(DownloadUrl, 35665, DownloadStoragePath))
                    mmApp.Log("Warning: Couldn't download update file. " + http.ErrorMessage);
                
                //var client = new WebClient();                
                //client.DownloadProgressChanged += client_DownloadProgressChanged;
                //// In order to get events we have to run this async and wait
                //client.DownloadFile(DownloadUrl, DownloadStoragePath);
            }
            catch(Exception ex)
            {
                ErrorMessage = ex.Message;
                mmApp.Log("Warning: Unable to download update.",ex);
                return false;
            }

            return true;
        }


        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public async Task<bool> DownloadAsync()
        //{
        //    var client = new WebClient();
        //    client.DownloadProgressChanged += client_DownloadProgressChanged;
        //    await client.DownloadFileTaskAsync(DownloadUrl, DownloadStoragePath);

        //    return true;
        //}

        /// <summary>
        /// Event you can use to get download progress information
        /// </summary>
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

        /// <summary>
        /// Forwards the DownloadProgressChangedEventHandler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgressChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// Executes the downloaded file in the download folder
        /// </summary>
        public bool ExecuteDownloadedFile()
        {
            try
            {
                var proc = new Process();
                Process.Start(DownloadStoragePath);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                mmApp.Log("Update Installer Execution Error", ex);
                return false;
            }

            return true;
        }

        

        /// <summary>
        /// Creates a string 
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private string GetVersionStringFromVersion(Version version)
        {
            var v = version;        
            return v.Major + "." + v.Minor + (v.Build > 0 ? "." + v.Build : "");
        }
    }



    /// <summary>
    /// Version info class used to 
    /// </summary>
    public class VersionInfo
    {
        public string Version { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string DownloadUrl { get; set; }
        public int DownloadSize { get; set; }

        public VersionInfo()
        {
            Version = "0.01";
        }
    }


}
