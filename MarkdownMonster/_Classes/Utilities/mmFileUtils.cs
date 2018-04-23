using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
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

            //using (var reader = new StreamReader(srcFile, true))
            //{
            //    enc = reader.CurrentEncoding;
            //}

            //return enc;

            // Detect byte order mark if any - otherwise assume default
            byte[] buffer = new byte[5];
            using (FileStream file = new FileStream(srcFile, FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
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

     //   /// <summary>
     //   /// This function returns the actual filename of a file
     //   /// that exists on disk. If you provide a path/file name
     //   /// that is not proper cased as input, this function fixes
     //   /// it up and returns the file using the path and file names
     //   /// as they exist on disk.
     //   /// </summary>
     //   /// <param name="filename">A filename to check</param>
     //   /// <returns></returns>
	    //public static string GetPhysicalPath(string filename)
     //   {
     //       try
     //       {
     //           StringBuilder sb = new StringBuilder(1500);
     //           uint result = GetLongPathName(filename, sb, sb.Capacity);
     //           if (result > 0)
     //               filename = sb.ToString();
     //       }
     //       catch { }

     //       return filename;
     //   }

       

     //   /// <summary>
     //   /// API call that takes an input path and turns it into a long path
     //   /// that matches the actual signature on disk
     //   /// </summary>
     //   /// <param name="ShortPath"></param>
     //   /// <param name="sb"></param>
     //   /// <param name="buffer"></param>
     //   /// <returns></returns>
     //   [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
     //   static extern uint GetLongPathName(string ShortPath, StringBuilder sb, int buffer);


     //   /// <summary>
     //   /// Returns a compact path with elipsis from a long path
     //   /// </summary>
     //   /// <param name="path"></param>
     //   /// <param name="length"></param>
     //   /// <returns></returns>
     //   public static string GetCompactPath(string path, int length = 70)
     //   {

     //       if (string.IsNullOrEmpty(path))
     //           return path;

     //       StringBuilder sb = new StringBuilder(length + 1);
     //       PathCompactPathEx(sb, path, length +1, 0);
     //       return sb.ToString();
     //   }

     //   [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
     //   static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);


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
			if (!mmApp.Configuration.EditorExtensionMappings.TryGetValue(ext,out editorSyntax))
				return null;

		    return editorSyntax;			
	    }

		#endregion

	    #region Type Utilities       

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
            catch(Exception ex)
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
        /// <param name="level">Optimization Level from 1-7</param>
        public static void OptimizePngImage(string pngFilename, int level = 5)
        {
            try
            {
                var pi = new ProcessStartInfo(Path.Combine(Environment.CurrentDirectory, "optipng.exe"),
                    $"-o{level} \"" + pngFilename + "\"");

                pi.WindowStyle = ProcessWindowStyle.Hidden;
                pi.WorkingDirectory = Environment.CurrentDirectory;
                Process.Start(pi);
            }
            catch { }
        }

		/// <summary>
		/// Opens an image in the configured image editor
		/// </summary>
		/// <param name="imageFile"></param>
	    public static bool OpenImageInImageEditor(string imageFile)
	    {
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

        public static void OpenFileInExplorer(string filename)
        {
            if (Directory.Exists(filename))
                ShellUtils.GoUrl(filename);
            else
                Process.Start("explorer.exe", $"/select,\"{filename}\"");
        }

        /// <summary>
        /// Executes a process with given command line parameters
        /// </summary>
        /// <param name="executable">Executable to run</param>
        /// <param name="arguments"></param>
        /// <param name="timeoutMs">Timeout of the process in milliseconds. Pass -1 to wait forever. Pass 0 to not wait.</param>
        /// <returns></returns>
        public static int ExecuteProcess(string executable, string arguments =  null, int timeoutMs = 0, ProcessWindowStyle windowStyle= ProcessWindowStyle.Hidden)
	    {
		    Process process;

		    try
		    {
			    using (process = new Process())
			    {
				    process.StartInfo.FileName = executable;
				    process.StartInfo.Arguments = arguments;
				    process.StartInfo.WindowStyle = windowStyle;
			        if (windowStyle == ProcessWindowStyle.Hidden)
			            process.StartInfo.CreateNoWindow = true;

                    process.StartInfo.UseShellExecute = false;

				    process.StartInfo.RedirectStandardOutput = true;
				    process.StartInfo.RedirectStandardError = true;

				    process.OutputDataReceived += (sender, args) =>
				    {
					    Console.WriteLine(args.Data);
				    };
				    process.ErrorDataReceived += (sender, args) =>
				    {
					    Console.WriteLine(args.Data);
				    };

				    process.Start();
					
				    if (timeoutMs < 0)
					    timeoutMs = 99999999; // indefinitely

				    if (timeoutMs > 0)
				    {
					    if (!process.WaitForExit(timeoutMs))
					    {
						    Console.WriteLine("Process timed out.");
						    return 1460;
					    }
				    }
				    else
					    return 0;

				    return process.ExitCode;
			    }
		    }
		    catch (Exception ex)
		    {
			    Console.WriteLine("Error executing process: " + ex.Message);
			    return -1; // unhandled error
		    }


	    }


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
                ExecuteProcess(mmApp.Configuration.WebBrowserPreviewExecutable, $"\"{url}\"");
            }
        }
        #endregion

        #region Git Operations
        /// <summary>
        /// Opens the configured Git Client in the specified folder
        /// </summary>
        /// <param name="folder"></param>
        public static bool OpenGitClient(string folder)
        {            
            
            var exe = mmApp.Configuration.GitClientExecutable;
            if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
                return false;

            try
            {
                var pi = Process.Start(exe, folder);
                if (pi == null)
                    return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks to see if 
        /// </summary>
        /// <returns></returns>
        internal static string FindGitClient()
        {
            string git = null;

            git = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "SmartGit\\bin\\SmartGit64.exe");
            if (File.Exists(git))
                return git;

            git = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GitHubDesktop\\GitHubDesktop.exe");
            if (File.Exists(git))
                return git;
            
            git = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SourceTree\\sourcetree.exe");
            if (File.Exists(git))
                return git;

            return git;
        }


        /// <summary>
        /// Commits a specific, individual file to Git and optionally pushes
        /// to the Git origin.
        /// 
        /// Note: Push operation may end up pushing more than the single file
        /// committed if previous commits were made.
        /// </summary>
        /// <param name="filename">File to commit</param>
        /// <param name="push">If set pushes to the currently configured Git Origin</param>
        /// <param name="message">A string message passed in that is set if an error occurs. Message can be used directly for error/status messages.</param>
        /// <returns>true or false. If false message parameter is set.</returns>
        public static bool CommitFileToGit(string filename, bool push, out string message)
        {
            if (!mmApp.Model.ActiveDocument.Save())
            {
                message = "Couldn't save file.";
                return false;
            }

            //  git commit --only Build.ps1 -m "Updating documentation for readme.md."
            //  git push origin

            string file = Path.GetFullPath(filename);
            string justFile = System.IO.Path.GetFileName(file);

            if (!File.Exists(file))
            {
                message = $"File {justFile} doesn't exist.";
                return false;
            }

            string path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path))
            {
                message = $"File {file} doesn't exist.";
                return false;
            }


            string origPath = Environment.CurrentDirectory;
            try
            {
                Directory.SetCurrentDirectory(path);

                int result = ExecuteProcess("git.exe",
                    $"commit --only \"{file}\" -m \"Updating documentation for {justFile}\".", timeoutMs: 10000);
                if (result != 0)
                {
                    message = $"There are no changes to commit for {justFile}.";
                    return false;
                }

                if (push)
                    ExecuteProcess("git.exe", "push origin",timeoutMs: 10000);
            }
            catch(Exception ex)
            {
                message = "An error occurred committing to Git: " + ex.Message;
                return false;
            }
            finally
            {
                Directory.SetCurrentDirectory(origPath);
            }

            message = string.Empty;
            return true;            
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
		    int result =SHFileOperation(ref shf);

		    return result == 0;
	    }
	    #endregion
    }


}
