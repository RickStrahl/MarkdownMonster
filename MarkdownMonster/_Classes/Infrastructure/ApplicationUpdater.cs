using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
            CurrentVersion = GetVersionStringFromVersion(version);
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
            CurrentVersion = GetVersionStringFromVersion(version);
            Initialize();
        }


        private void Initialize()
        {
            VersionFile = mmApp.Configuration.CommonFolder + "MarkdownMonster_Version.xml";
            DownloadStoragePath = Path.Combine(KnownFolders.GetDefaultPath(KnownFolder.Downloads), "MarkDownMonsterSetup.exe");

            VersionCheckUrl = mmApp.Configuration.ApplicationUpdates.UpdateCheckUrl;
            DownloadUrl = mmApp.Configuration.ApplicationUpdates.InstallerDownloadUrl;
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

            if (!string.IsNullOrEmpty(xml))
            {
                var ver = SerializationUtils.DeSerializeObject(xml, typeof(VersionInfo)) as VersionInfo;
                if (ver != null)
                {
                    VersionInfo = ver;

                    if (ver.Version.CompareTo(CurrentVersion) > 0)
                        return true;
                }
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

    /// <summary>
    /// Class containing methods to retrieve specific file system paths.
    /// </summary>
    public static class KnownFolders
    {
        private static string[] _knownFolderGuids = new string[]
        {
            "{56784854-C6CB-462B-8169-88E350ACB882}", // Contacts
            "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}", // Desktop
            "{FDD39AD0-238F-46AF-ADB4-6C85480369C7}", // Documents
            "{374DE290-123F-4565-9164-39C4925E467B}", // Downloads
            "{1777F761-68AD-4D8A-87BD-30B759FA33DD}", // Favorites
            "{BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968}", // Links
            "{4BD8D571-6D19-48D3-BE97-422220080E43}", // Music
            "{33E28130-4E1E-4676-835A-98395C3BC3BB}", // Pictures
            "{4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4}", // SavedGames
            "{7D1D3A04-DEBB-4115-95CF-2F29DA2920DA}", // SavedSearches
            "{18989B1D-99B5-455B-841C-AB7C74E4DDFC}", // Videos
        };

        /// <summary>
        /// Gets the current path to the specified known folder as currently configured. This does
        /// not require the folder to be existent.
        /// </summary>
        /// <param name="knownFolder">The known folder which current path will be returned.</param>
        /// <returns>The default path of the known folder.</returns>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the path
        ///     could not be retrieved.</exception>
        public static string GetPath(KnownFolder knownFolder)
        {
            return GetPath(knownFolder, false);
        }

        /// <summary>
        /// Gets the current path to the specified known folder as currently configured. This does
        /// not require the folder to be existent.
        /// </summary>
        /// <param name="knownFolder">The known folder which current path will be returned.</param>
        /// <param name="defaultUser">Specifies if the paths of the default user (user profile
        ///     template) will be used. This requires administrative rights.</param>
        /// <returns>The default path of the known folder.</returns>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the path
        ///     could not be retrieved.</exception>
        public static string GetPath(KnownFolder knownFolder, bool defaultUser)
        {
            return GetPath(knownFolder, KnownFolderFlags.DontVerify, defaultUser);
        }

        /// <summary>
        /// Gets the default path to the specified known folder. This does not require the folder
        /// to be existent.
        /// </summary>
        /// <param name="knownFolder">The known folder which default path will be returned.</param>
        /// <returns>The current (and possibly redirected) path of the known folder.</returns>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the path
        ///     could not be retrieved.</exception>
        public static string GetDefaultPath(KnownFolder knownFolder)
        {
            return GetDefaultPath(knownFolder, false);
        }

        /// <summary>
        /// Gets the default path to the specified known folder. This does not require the folder
        /// to be existent.
        /// </summary>
        /// <param name="knownFolder">The known folder which default path will be returned.</param>
        /// <param name="defaultUser">Specifies if the paths of the default user (user profile
        ///     template) will be used. This requires administrative rights.</param>
        /// <returns>The current (and possibly redirected) path of the known folder.</returns>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the path
        ///     could not be retrieved.</exception>
        public static string GetDefaultPath(KnownFolder knownFolder, bool defaultUser)
        {
            return GetPath(knownFolder, KnownFolderFlags.DefaultPath | KnownFolderFlags.DontVerify,
                defaultUser);
        }

        /// <summary>
        /// Creates and initializes the known folder.
        /// </summary>
        /// <param name="knownFolder">The known folder which will be initialized.</param>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the known
        ///     folder could not be initialized.</exception>
        public static void Initialize(KnownFolder knownFolder)
        {
            Initialize(knownFolder, false);
        }

        /// <summary>
        /// Creates and initializes the known folder.
        /// </summary>
        /// <param name="knownFolder">The known folder which will be initialized.</param>
        /// <param name="defaultUser">Specifies if the paths of the default user (user profile
        ///     template) will be used. This requires administrative rights.</param>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the known
        ///     folder could not be initialized.</exception>
        public static void Initialize(KnownFolder knownFolder, bool defaultUser)
        {
            GetPath(knownFolder, KnownFolderFlags.Create | KnownFolderFlags.Init, defaultUser);
        }

        private static string GetPath(KnownFolder knownFolder, KnownFolderFlags flags,
            bool defaultUser)
        {
            IntPtr outPath;
            int result = SHGetKnownFolderPath(new Guid(_knownFolderGuids[(int)knownFolder]),
                (uint)flags, new IntPtr(defaultUser ? -1 : 0), out outPath);
            if (result >= 0)
            {
                return Marshal.PtrToStringUni(outPath);
            }
            else
            {
                throw new ExternalException("Unable to retrieve the known folder path. It may not "
                                            + "be available on this system.", result);
            }
        }

        /// <summary>
        /// Retrieves the full path of a known folder identified by the folder's KnownFolderID.
        /// </summary>
        /// <param name="rfid">A KnownFolderID that identifies the folder.</param>
        /// <param name="dwFlags">Flags that specify special retrieval options. This value can be
        ///     0; otherwise, one or more of the KnownFolderFlag values.</param>
        /// <param name="hToken">An access token that represents a particular user. If this
        ///     parameter is NULL, which is the most common usage, the function requests the known
        ///     folder for the current user. Assigning a value of -1 indicates the Default User.
        ///     The default user profile is duplicated when any new user account is created.
        ///     Note that access to the Default User folders requires administrator privileges.
        ///     </param>
        /// <param name="ppszPath">When this method returns, contains the address of a string that
        ///     specifies the path of the known folder. The returned path does not include a
        ///     trailing backslash.</param>
        /// <returns>Returns S_OK if successful, or an error value otherwise.</returns>
        [DllImport("Shell32.dll")]
        private static extern int SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken,
            out IntPtr ppszPath);

        [Flags]
        private enum KnownFolderFlags : uint
        {
            SimpleIDList = 0x00000100,
            NotParentRelative = 0x00000200,
            DefaultPath = 0x00000400,
            Init = 0x00000800,
            NoAlias = 0x00001000,
            DontUnexpand = 0x00002000,
            DontVerify = 0x00004000,
            Create = 0x00008000,
            NoAppcontainerRedirection = 0x00010000,
            AliasOnly = 0x80000000
        }


    }

    /// <summary>
    /// Standard folders registered with the system. These folders are installed with Windows Vista
    /// and later operating systems, and a computer will have only folders appropriate to it
    /// installed.
    /// </summary>
    public enum KnownFolder
    {
        Contacts,
        Desktop,
        Documents,
        Downloads,
        Favorites,
        Links,
        Music,
        Pictures,
        SavedGames,
        SavedSearches,
        Videos
    }
}
