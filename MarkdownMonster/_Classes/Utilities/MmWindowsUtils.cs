using System;
using System.IO;
using Microsoft.Win32;
#if NETFULL
using Microsoft.WindowsAPICodePack.Dialogs;
#endif

namespace MarkdownMonster.Utilities
{
    /// <summary>
    /// Windows specific system and information helpers
    /// Helper class that provides Windows and .NET Version numbers.
    /// </summary>
    public static class mmWindowsUtils
    {
        /// <summary>
        /// Returns the Windows major version number for this computer.
        /// based on this: http://stackoverflow.com/questions/21737985/windows-version-in-c-sharp/37716269#37716269
        /// </summary>
        public static uint WinMajorVersion
        {
            get
            {
                dynamic major;
                // The 'CurrentMajorVersionNumber' string value in the CurrentVersion key is new for Windows 10,
                // and will most likely (hopefully) be there for some time before MS decides to change this - again...
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentMajorVersionNumber",
                    out major))
                {
                    return (uint)major;
                }

                // When the 'CurrentMajorVersionNumber' value is not present we fallback to reading the previous key used for this: 'CurrentVersion'
                dynamic version;
                if (!TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion",
                    out version))
                    return 0;

                var versionParts = ((string)version).Split('.');
                if (versionParts.Length != 2) return 0;
                uint majorAsUInt;
                return uint.TryParse(versionParts[0], out majorAsUInt) ? majorAsUInt : 0;
            }
        }

        /// <summary>
        ///     Returns the Windows minor version number for this computer.
        /// </summary>
        public static uint WinMinorVersion
        {
            get
            {
                dynamic minor;
                // The 'CurrentMinorVersionNumber' string value in the CurrentVersion key is new for Windows 10,
                // and will most likely (hopefully) be there for some time before MS decides to change this - again...
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentMinorVersionNumber",
                    out minor))
                {
                    return (uint)minor;
                }

                // When the 'CurrentMinorVersionNumber' value is not present we fallback to reading the previous key used for this: 'CurrentVersion'
                dynamic version;
                if (!TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion",
                    out version))
                    return 0;

                var versionParts = ((string)version).Split('.');
                if (versionParts.Length != 2) return 0;
                uint minorAsUInt;
                return uint.TryParse(versionParts[1], out minorAsUInt) ? minorAsUInt : 0;
            }
        }

        /// <summary>
        ///     Returns the Windows minor version number for this computer.
        /// </summary>
        public static uint WinBuildVersion
        {
            get
            {
                dynamic buildNumber;
                // The 'CurrentMinorVersionNumber' string value in the CurrentVersion key is new for Windows 10,
                // and will most likely (hopefully) be there for some time before MS decides to change this - again...
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber",
                    out buildNumber))
                {
                    return Convert.ToUInt32(buildNumber);
                }


                if (!TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuild",
                    out buildNumber))
                    return 0;

                return Convert.ToUInt32(buildNumber);
            }
        }

        /// <summary>
        ///     Returns the Windows minor version number for this computer.
        /// </summary>
        public static string WinBuildLabVersion
        {
            get
            {
                dynamic buildNumber;
                // The 'CurrentMinorVersionNumber' string value in the CurrentVersion key is new for Windows 10,
                // and will most likely (hopefully) be there for some time before MS decides to change this - again...
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "BuildLabEx",
                    out buildNumber))
                {
                    return buildNumber;
                }

                return WinBuildVersion.ToString();
            }
        }

        /// <summary>
        /// Returns whether or not the current computer is a server or not.
        /// </summary>
        public static uint IsServer
        {
            get
            {
                dynamic installationType;
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "InstallationType",
                    out installationType))
                {
                    return (uint)(installationType.Equals("Client") ? 0 : 1);
                }

                return 0;
            }
        }

        static string DotnetVersion = null;

        /// <summary>
        /// Returns the .NET framework version installed on the machine
        /// as a string  of 4.x.y version
        /// </summary>
        /// <remarks>Minimum version supported is 4.0</remarks>
        /// <returns></returns>
        public static string GetDotnetVersion()
        {
            if (!string.IsNullOrEmpty(DotnetVersion))
                return DotnetVersion;

            dynamic value;
            TryGetRegistryKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\", "Release", out value);

            if (value == null)
            {
                DotnetVersion = "4.0";
                return DotnetVersion;
            }

            int releaseKey = value;

            // https://msdn.microsoft.com/en-us/library/hh925568(v=vs.110).aspx
            // RegEdit paste: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full
            if (releaseKey >= 528040)
                DotnetVersion = "4.8";
            else if (releaseKey >= 461808)
                DotnetVersion = "4.7.2";
            else if (releaseKey >= 461308)
                DotnetVersion = "4.7.1";
            else if (releaseKey >= 460798)
                DotnetVersion = "4.7";
            else if (releaseKey >= 394802)
                DotnetVersion = "4.6.2";
            else if (releaseKey >= 394254)
                DotnetVersion = "4.6.1";
            else if (releaseKey >= 393295)
                DotnetVersion = "4.6";
            else if ((releaseKey >= 379893))
                DotnetVersion = "4.5.2";
            else if ((releaseKey >= 378675))
                DotnetVersion = "4.5.1";
            else if ((releaseKey >= 378389))
                DotnetVersion = "4.5";

            // This line should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            else
                DotnetVersion = "4.0";

            return DotnetVersion;
        }

        static string _WindowsVersion = null;

        /// <summary>
        /// Returns a Windows Version string including build number
        /// </summary>
        /// <returns></returns>
        public static string GetWindowsVersion()
        {

            if (string.IsNullOrEmpty(_WindowsVersion))
                _WindowsVersion = WinMajorVersion + "." + WinMinorVersion + "." +
                                  WinBuildLabVersion;
            return _WindowsVersion;
        }

        public static bool TryGetRegistryKey(string path, string key, out dynamic value,
            bool UseCurrentUser = false)
        {
            value = null;
            try
            {
                RegistryKey rk;
                if (UseCurrentUser)
                    rk = Registry.CurrentUser.OpenSubKey(path);
                else
                    rk = Registry.LocalMachine.OpenSubKey(path);

                if (rk == null) return false;
                value = rk.GetValue(key);
                return value != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Displays a folder dialog
        /// </summary>
        /// <param name="initialPath"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string ShowFolderDialog(string initialPath, string title)
        {

#if NETFULL
            var dlg = new CommonOpenFileDialog();

            dlg.Title = "Select or create a folder to clone Repository to:";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = initialPath;
            dlg.RestoreDirectory = true;
            dlg.ShowHiddenItems = true;
            dlg.ShowPlacesList = true;
            dlg.EnsurePathExists = true;

            var result = dlg.ShowDialog();

            if (result != CommonFileDialogResult.Ok)
                return null;

            return dlg.FileName;
#else
            // Use updated FolderBrowserDialog
            var oldPath = Directory.GetCurrentDirectory();
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = title;

            dlg.SelectedPath = initialPath;
            dlg.ShowNewFolderButton = true;
            dlg.UseDescriptionForTitle = true;
            var result = dlg.ShowDialog();

            Directory.SetCurrentDirectory(oldPath);

            if (result != System.Windows.Forms.DialogResult.OK || !Directory.Exists(dlg.SelectedPath))
                return null;

            return dlg.SelectedPath;
#endif

        }
    }
}
