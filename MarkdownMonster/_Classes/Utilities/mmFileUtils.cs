﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using FontAwesome.WPF;
using MarkdownMonster.Annotations;
using MarkdownMonster.Utilities;
using Microsoft.Win32;
using Westwind.Utilities;

namespace MarkdownMonster
{
    /// <summary>
    /// Internal File Utilities class
    /// </summary>
    public static class mmFileUtils
    {



        #region File Utilities
        /// <summary>
        /// Method checks for existance of full filename and tries
        /// to check for file in the initial startup folder.
        /// </summary>
        /// <param name="file">Name of file - fully qualified or local folder file</param>
        /// <returns>filename or null if file doesn't exist</returns>
        public static string FixupDocumentFilename(string file)
        {
            if (file == null)
                return null;

            file = file.Replace("/", "\\");

            if (File.Exists(file))
                return file;

            var newFile = Path.Combine(App.InitialStartDirectory, file);
            if (File.Exists(newFile))
                return newFile;

            return null;
        }

        /// <summary>
        /// Normalizes a potentially relative pathname to a base path name if the
        /// exact filename doesn't exist by prepending the base path explicitly.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="basePath"></param>
        /// <returns>Normalized file name, or null if the file is not found</returns>
        public static string NormalizeFilenameWithBasePath(string file, string basePath)
        {
            if (File.Exists(file))
                return file;

            var newFile = Path.Combine(basePath, file);
            if (File.Exists(newFile))
                return newFile;

            return null;
        }



        /// <summary>
        /// Creates an MD5 checksum of a file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetChecksumFromFile(string file)
        {
            if (!File.Exists(file))
                return null;

            try
            {
                byte[] checkSum;
                using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var md = new MD5CryptoServiceProvider();
                    checkSum = md.ComputeHash(stream);
                }

                return StringUtils.BinaryToBinHex(checkSum);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve the file encoding for a given file so we can capture
        /// and store the Encoding when writing the file back out after
        /// editing.
        ///
        /// Default is Utf-8 (w/ BOM). If file without BOM is read it is
        /// assumed it's UTF-8.
        /// </summary>
        /// <param name="srcFile"></param>
        /// <returns></returns>
        public static Encoding GetFileEncoding(string srcFile)
        {
            if (string.IsNullOrEmpty(srcFile) || srcFile == "untitled")
                return Encoding.UTF8;

            // Use Default of Encoding.Default (Ansi CodePage)
            Encoding enc;

            // Detect byte order mark if any - otherwise assume default
            byte[] buffer = new byte[5];
            using (FileStream file = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                file.Read(buffer, 0, 5);
                file.Close();
            }

            if (buffer.Length > 2 && buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                enc = Encoding.UTF8;
            else if (buffer.Length > 1 && buffer[0] == 0xff && buffer[1] == 0xfe)
                enc = Encoding.Unicode; //UTF-16LE
            else if (buffer.Length > 1 && buffer[0] == 0xfe && buffer[1] == 0xff)
                enc = Encoding.BigEndianUnicode; //UTF-16BE
            else if (buffer.Length > 2 && buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
                enc = Encoding.UTF7;
            else if (buffer.Length > 3 && buffer[0] != 0 && buffer[1] == 0 && buffer[2] != 0 && buffer[3] == 0)
                enc = Encoding.Unicode;  // no BOM Unicode - bad idea: Should always have BOM and we'll write it
            else
                // no identifiable BOM - use UTF-8 w/o BOM
                enc = new UTF8Encoding(false);

            return enc;
        }


        /// <summary>
        /// Retrieves the editor syntax for a file based on extension for use in the editor
        ///
        /// Unknown file types returning null
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetEditorSyntaxFromFileType(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            if (filename.ToLower() == "untitled")
                return "markdown";

            string editorSyntax = null;

            var ext = Path.GetExtension(filename).ToLower().Replace(".", "");
            if (ext == "md")
                return "markdown"; // most common use case

            // look up all others
            if (!mmApp.Configuration.EditorExtensionMappings.TryGetValue(ext, out editorSyntax))
                return null;

            return editorSyntax;
        }

        #endregion

        #region Type and Language Utilities

        /// <summary>
        /// Safely converts a double to an integer
        /// </summary>
        /// <param name="value"></param>
        /// <param name="failValue"></param>
        /// <returns></returns>
        public static int TryConvertToInt32(double value, int failValue = 0)
        {
            if (double.IsNaN(value) || double.IsNegativeInfinity(value) || double.IsPositiveInfinity(value))
            {
                mmApp.Log("Double to Int Conversion failed: " + value + " - failValue: " + failValue);
                return failValue;
            }

            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                mmApp.Log("Double to Int Conversion failed: " + value + " - failValue: " + failValue +
                         "\r\n" + ex.GetBaseException().Message +
                         "\r\n" + ex.StackTrace);
                return failValue;
            }
        }
        #endregion


        #region Image Utilities

        /// <summary>
        /// Tries to optimize png images in the background
        /// This is not fast and does not happen right away
        /// so generally this can be applied when images are saved.
        /// </summary>
        /// <param name="pngFilename">Filename to optimize</param>
        /// <param name="level">Optimization Level from 1-9</param>
        public static void OptimizePngImage(string pngFilename, int level = 9)
        {
            try
            {
                var pi = new ProcessStartInfo(Path.Combine(App.InitialStartDirectory, "pingo.exe"),
                    "-auto \"" + pngFilename + "\"");

                pi.WindowStyle = ProcessWindowStyle.Hidden;
                pi.WorkingDirectory = Environment.CurrentDirectory;
                Process.Start(pi);
            }
            catch { }
        }

        /// <summary>
        /// Optimizes a jpeg image
        /// </summary>
        /// <param name="imageFilename"></param>
        /// <param name="imageQuality">Optional image quality. If not specified auto is used</param>
        public static void OptimizeImage(string imageFilename, int imageQuality = 0, Action<bool> onComplete = null)
        {
            try
            {
                string exec = Path.Combine(App.InitialStartDirectory, "pingo.exe");
                string args;

                if (imageQuality > 0)
                {
                    args = $"-auto={imageQuality} \"" + imageFilename + "\"";
                }
                else
                {
                    args = "auto \"" + imageFilename + "\"";
                }

                if (onComplete != null)
                {
                    Task.Run(() =>
                    {
                        int result = ShellUtils.ExecuteProcess(exec, args, 30000, ProcessWindowStyle.Hidden);
                        onComplete(result == 0);
                    }).GetAwaiter();
                }
                else
                    ShellUtils.ExecuteProcess(exec, args, 0, ProcessWindowStyle.Hidden);
            }
            catch { }
        }

        /// <summary>
        /// Opens an image in the configured image editor
        /// </summary>
        /// <param name="imageFile"></param>
        public static bool OpenImageInImageEditor(string imageFile)
        {
            imageFile = System.Net.WebUtility.UrlDecode(imageFile);

            try
            {
                string exe = mmApp.Configuration.ImageEditor;
                if (!string.IsNullOrEmpty(exe))
                    Process.Start(new ProcessStartInfo(exe, $"\"{imageFile}\""));
                else
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = imageFile,
                        UseShellExecute = true,
                        Verb = "Edit"
                    });
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Opens an image in the configured image viewer.
        /// If none is specified uses default image viewer
        /// </summary>
        /// <param name="imageFile"></param>
        public static bool OpenImageInImageViewer(string imageFile)
        {
            imageFile = System.Net.WebUtility.UrlDecode(imageFile);

            try
            {
                string exe = mmApp.Configuration.ImageViewer;
                if (!string.IsNullOrEmpty(exe))
                    Process.Start(new ProcessStartInfo(exe, $"\"{imageFile}\""));
                else
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = imageFile,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }



        /// <summary>
        /// Tries to find an installed image editor on
        /// the system as a default.
        /// </summary>
        /// <returns></returns>
        public static string FindImageEditor()
        {
            string file = null;
            var programFiles64 = Environment.GetEnvironmentVariable("ProgramW6432");

            file = Path.Combine(programFiles64, "Techsmith", "Snagit 2018", "SnagItEditor.exe");
            if (File.Exists(file))
                return file;

            file = Path.Combine(programFiles64,
                "paint.net", "PaintDotnet.exe");
            if (File.Exists(file))
                return file;

            file = Path.Combine(programFiles64,
                "irfanview", "i_view64.exe");
            if (File.Exists(file))
                return file;

            file = Path.Combine(programFiles64, "GIMP 2", "bin");
            if (Directory.Exists(file))
            {
                var di = new DirectoryInfo(file);

                var fi = di.GetFiles("gimp-*.*.*")
                    .OrderByDescending(d => d.LastWriteTime)
                    .FirstOrDefault();

                if (fi != null)
                    return Path.Combine(fi.FullName);
            }

            file = @"mspaint.exe";

            return file;
        }

        /// <summary>
        /// Returns the image media type for a give file extension based
        /// on a filename or url passed in.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetImageMediaTypeFromFilename(string file)
        {
            if (string.IsNullOrEmpty(file))
                return file;

            string ext = Path.GetExtension(file).ToLower();
            if (ext == ".jpg" || ext == ".jpeg")
                return "image/jpeg";
            if (ext == ".png")
                return "image/png";
            if (ext == ".gif")
                return "image/gif";
            if (ext == ".bmp")
                return "image/bmp";
            if (ext == ".tif" || ext == ".tiff")
                return "image/tiff";

            // fonts
            if (ext == ".woff")
                return "application/font-woff";
            if (ext == ".svg")
                return "image/svg+xml";

            // ignored fonts
            if (ext == ".woff2")
                return "font/woff2";
            //if (ext == ".otf")
            //    return "application/x-font-opentype";
            //if (ext == ".ttf")
            //    return "application/x-font-ttf";
            //if (ext == ".eot")
            //    return "application/vnd.ms-fontobject";

            return "application/image";
        }


        #endregion

        #region Shell Operations

        /// <summary>
        /// Opens the configured image editor. If command can't be executed
        /// the function returns false
        /// </summary>
        /// <param name="folder"></param>
        /// <returns>false if process couldn't be started - most likely invalid link</returns>
        public static bool OpenTerminal(string folder)
        {
            try
            {
                Process.Start(mmApp.Configuration.TerminalCommand,
                    string.Format(mmApp.Configuration.TerminalCommandArgs, folder));
            }
            catch
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// Shows external browser that's been configured in the MM Configuration.
        /// Defaults to Chrome
        /// </summary>
        /// <param name="url"></param>
        public static void ShowExternalBrowser(string url)
        {
            if (string.IsNullOrEmpty(mmApp.Configuration.WebBrowserPreviewExecutable) ||
                !File.Exists(mmApp.Configuration.WebBrowserPreviewExecutable))
            {
                mmApp.Configuration.WebBrowserPreviewExecutable = null;
                ShellUtils.GoUrl(url);
            }
            else
            {
                ShellUtils.ExecuteProcess(mmApp.Configuration.WebBrowserPreviewExecutable, $"\"{url}\"");
            }
        }


        /// <summary>
        /// Displays the Windows Open With dialog with options.
        /// </summary>
        /// <param name="path">file to open</param>
        public static void ShowOpenWithDialog(string path)
        {
            var args = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
            args += ",OpenAs_RunDLL " + path;
            Process.Start("rundll32.exe", args);
        }
        #endregion

        #region Installation Helpers

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
                Registry.CurrentUser.DeleteSubKeyTree("Software\\Classes\\Markdown Monster", false);

                if (WindowsUtils.TryGetRegistryKey("Software\\Classes\\.md", null, out value, true) && value == "Markdown Monster")
                    Registry.CurrentUser.DeleteSubKey("Software\\Classes\\.md");

                if (WindowsUtils.TryGetRegistryKey("Software\\Classes\\.markdown", null, out value, true) && value == "Markdown Monster")
                    Registry.CurrentUser.DeleteSubKey("Software\\Classes\\.markdown");

                return;
            }


            if (!WindowsUtils.TryGetRegistryKey("Software\\Classes\\Markdown Monster", null, out value, true))
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

            if (!WindowsUtils.TryGetRegistryKey("Software\\Classes\\Markdown Monster\\shell\\open\\command", null, out value, true))
            {
                using (var rk = Registry.CurrentUser.CreateSubKey("Software\\Classes\\Markdown Monster\\shell\\open\\command", true))
                {
                    rk.SetValue(null, $"\"{installFolder}\\MarkdownMonster.exe\" \"%1\"");
                }
            }

            if (!WindowsUtils.TryGetRegistryKey("Software\\Classes\\Markdown Monster\\DefaultIcon", null, out value, true))
            {
                var rk = Registry.CurrentUser.CreateSubKey("Software\\Classes\\Markdown Monster\\DefaultIcon", true);
                rk.SetValue(null, $"{installFolder}\\MarkdownMonster.exe,0");
            }


            if (!WindowsUtils.TryGetRegistryKey("Software\\Classes\\.md", null, out value, true))
            {
                var rk = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.md");
                rk.SetValue(null, "Markdown Monster");
            }

            if (!WindowsUtils.TryGetRegistryKey("Software\\Classes\\.markdown", null, out value, true))
            {
                using (var rk = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.markdown"))
                {
                    rk.SetValue(null, "Markdown Monster");
                }
            }
            if (!WindowsUtils.TryGetRegistryKey("Software\\Classes\\.mdcrypt", null, out value, true))
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
                    string mmFolder = Path.Combine(App.InitialStartDirectory, "Markdown Monster");
                    string path = sk.GetValue("Path").ToString();

                    if (uninstall)
                    {
                        path = path.Replace(";" + mmFolder, "");
                        sk.SetValue("Path", path);
                    }
                    else if (!path.Contains(mmFolder))
                    {
                        var pathList = path.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
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


        #endregion

        #region Recycle Bin Deletion
        // Credit: http://stackoverflow.com/a/3282450/11197
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)] public int wFunc;
            public string pFrom;
            public string pTo;
            public short fFlags;
            [MarshalAs(UnmanagedType.Bool)] public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        public const int FO_DELETE = 3;
        public const int FOF_ALLOWUNDO = 0x40;
        public const int FOF_NOCONFIRMATION = 0x10; // Don't prompt the user

        public static bool MoveToRecycleBin(string filename)
        {
            var shf = new SHFILEOPSTRUCT();
            shf.wFunc = FO_DELETE;
            shf.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION;
            shf.pFrom = filename + '\0'; // required!
            int result = SHFileOperation(ref shf);

            return result == 0;
        }
        #endregion
    }
}
