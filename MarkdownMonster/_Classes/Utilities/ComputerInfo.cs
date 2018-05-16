using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace MarkdownMonster
{
    /// <summary>
    /// Static class that adds convenient methods for getting information on the running computers basic hardware and os setup.
    /// Based on this: http://stackoverflow.com/questions/21737985/windows-version-in-c-sharp/37716269#37716269
    /// </summary>
    public static class ComputerInfo
    {
        /// <summary>
        ///     Returns the Windows major version number for this computer.
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
                    return (uint) major;
                }

                // When the 'CurrentMajorVersionNumber' value is not present we fallback to reading the previous key used for this: 'CurrentVersion'
                dynamic version;
                if (!TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion", out version))
                    return 0;

                var versionParts = ((string) version).Split('.');
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
                    return (uint) minor;
                }

                // When the 'CurrentMinorVersionNumber' value is not present we fallback to reading the previous key used for this: 'CurrentVersion'
                dynamic version;
                if (!TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion", out version))
                    return 0;

                var versionParts = ((string) version).Split('.');
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


                if (!TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuild", out buildNumber))
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
        ///     Returns whether or not the current computer is a server or not.
        /// </summary>
        public static uint IsServer
        {
            get
            {
                dynamic installationType;
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "InstallationType",
                    out installationType))
                {
                    return (uint) (installationType.Equals("Client") ? 0 : 1);
                }

                return 0;
            }
        }

        static string DotnetVersion = null;

        /// <summary> 
        /// Returns the framework version installed on the machine
        ///  as a string  of 4.x.y version
        /// </summary>        
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
            Console.WriteLine(value);

            // https://msdn.microsoft.com/en-us/library/hh925568(v=vs.110).aspx
            // RegEdit paste: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full
            if(releaseKey >= 461808)
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
                _WindowsVersion = ComputerInfo.WinMajorVersion + "." + ComputerInfo.WinMinorVersion + "." + ComputerInfo.WinBuildLabVersion;
            return _WindowsVersion;
        }


        /// <summary>
        /// Set Internet Explorer browser compatibility
        /// </summary>
        /// <param name="exename"></param>
        public static void EnsureBrowserEmulationEnabled(string exename = "MarkdownMonster.exe", bool uninstall = false)
        {

            try
            {
                using (
                    var rk =
                        Registry.CurrentUser.OpenSubKey(
                            @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true)
                )
                {
                    if (!uninstall)
                    {
                        dynamic value = rk.GetValue(exename);
                        if (value == null)
                            rk.SetValue(exename, (uint)11001, RegistryValueKind.DWord);
                    }
                    else
                        rk.DeleteValue(exename);
                }
            }
            catch
            {
            }
        }

        public static void EnsureAssociations(bool force = false, bool uninstall = false)
        {
            dynamic value = null;

            string installFolder = App.InitialStartDirectory;
            //.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            if (uninstall)
            {
                Registry.CurrentUser.DeleteSubKeyTree("Software\\Classes\\Markdown Monster",false);

                if (TryGetRegistryKey("Software\\Classes\\.md", null, out value, true) && value == "Markdown Monster")
                    Registry.CurrentUser.DeleteSubKey("Software\\Classes\\.md");

                if (TryGetRegistryKey("Software\\Classes\\.markdown", null, out value, true) && value == "Markdown Monster")
                    Registry.CurrentUser.DeleteSubKey("Software\\Classes\\.markdown");

                return;
            }


            if (!TryGetRegistryKey("Software\\Classes\\Markdown Monster", null, out value, true))
            {
                using (var rk = Registry.CurrentUser.CreateSubKey("Software\\Classes\\Markdown Monster", true))
                {
                    rk.SetValue(null, "Program Markdown Monster");
                }
            }
            else
            {
                if (!force)                    
                    return; // already exists
            }

            if (!TryGetRegistryKey("Software\\Classes\\Markdown Monster\\shell\\open\\command", null, out value, true))
            {
                using (var rk = Registry.CurrentUser.CreateSubKey("Software\\Classes\\Markdown Monster\\shell\\open\\command", true))
                {
                    rk.SetValue(null, $"\"{installFolder}\\MarkdownMonster.exe\" \"%1\"");
                }
            }

            if (!TryGetRegistryKey("Software\\Classes\\Markdown Monster\\DefaultIcon", null, out value, true))
            {
                var rk = Registry.CurrentUser.CreateSubKey("Software\\Classes\\Markdown Monster\\DefaultIcon", true);
                rk.SetValue(null, $"{installFolder}\\MarkdownMonster.exe,0");
            }


            if (!TryGetRegistryKey("Software\\Classes\\.md", null, out value,true))
            {
                var rk = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.md");
                rk.SetValue(null, "Markdown Monster");                
            }

            if (!TryGetRegistryKey("Software\\Classes\\.markdown", null, out value, true))
            {
                using (var rk = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.markdown"))
                {
                    rk.SetValue(null, "Markdown Monster");
                }
            }
            if (!TryGetRegistryKey("Software\\Classes\\.mdcrypt", null, out value, true))
            {
                using (var rk = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.mdcrypt"))
                {
                    rk.SetValue(null, "Markdown Monster");
                }
            }
        }

        public static void EnsureSystemPath(bool uninstall = false)
        {
            try
            {
                using (var sk = Registry.CurrentUser.OpenSubKey("Environment", true))
                {
                    string mmFolder = Path.Combine(App.InitialStartDirectory,"Markdown Monster");
                    string path = sk.GetValue("Path").ToString();

                    if (uninstall)
                    {
                        path = path.Replace(";" + mmFolder, "");
                        sk.SetValue("Path", path);
                    }
                    else if (!path.Contains(mmFolder))
                    {
                        var pathList = path.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries).ToList();
                        pathList.Add(mmFolder);
                        path = string.Join(";", pathList.Distinct().ToArray());

                        sk.SetValue("Path", path);
                    }
                    
                }
            }
            catch
            {
            }
        }

    

        public static bool TryGetRegistryKey(string path, string key, out dynamic value, bool UseCurrentUser = false)
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


        
    }

   

}
