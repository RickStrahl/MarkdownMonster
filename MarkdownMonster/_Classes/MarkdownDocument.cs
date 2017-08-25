#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 04/28/2016
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Newtonsoft.Json;
using Westwind.Utilities;
using System.Linq;
using System.Security;

namespace MarkdownMonster
{
    /// <summary>
    /// Class that wraps the Active Markdown document used in the
    /// editor.
    /// [ComVisible] is important as we access this from JavaScript
    /// </summary>
    [ComVisible(true)]
    public class MarkdownDocument : INotifyPropertyChanged
    {
        private const string ENCRYPTION_PREFIX = "__ENCRYPTED__";

        /// <summary>
        /// Name of the Markdown file. If this is a new file the file is 
        /// named 'untitled'
        /// </summary>
        public string Filename
        {
            get { return _filename; }
            set
            {
                if (value == _filename) return;
                _filename = value;
                OnPropertyChanged();                
                OnPropertyChanged(nameof(FilenameWithIndicator));
                OnPropertyChanged(nameof(HtmlRenderFilename));
            }
        }
        private string _filename;


        /// <summary>
        /// Holds the disk file Crc of the document. This value is
        /// used to determine if the document on disk has changed when
        /// activating a document after having navigated off and when
        /// saving.
        /// </summary>
        [JsonIgnore]
        public string FileCrc { get; set; }


        /// <summary>
        /// Returns the filename with a dirty indicator (*) if the
        /// document has changed
        /// </summary>
        [JsonIgnore]
        public string FilenameWithIndicator
        {
            get
            {
                var fname = Filename;
                if (string.IsNullOrEmpty(Filename))
                    fname = "Untitled";

                return Path.GetFileName(fname) + (IsDirty ? "*" : "");                
            }
        }


        /// <summary>
        /// Returns a filename plus path and a change indicator
        /// Used when multiple tabs with the same file are open
        /// </summary>
        [JsonIgnore]
        public string FilenamePathWithIndicator
        {
            get
            {
                if (string.IsNullOrEmpty(Filename) || Filename == "untitled" || Filename == "Untitled")
                    return FilenameWithIndicator;

                string path = Path.GetFileName(Path.GetDirectoryName(Filename));

                return Path.GetFileName(Filename) + "  –  " + path  + (IsDirty ? "*" : ""); 
            }
        }

        /// <summary>
        /// Name of the auto save backup file
        /// </summary>
        [JsonIgnore]
        public string BackupFilename
        {
            get
            {
                return Path.Combine(
                    Path.GetDirectoryName(Filename),
                    Path.GetFileName(Filename) + ".saved.bak");
            }
        }

        /// <summary>
        /// Tries to return the title from the active Markdown document
        /// by looking at:
        /// 
        /// 1 - For # title line in the first 5 lines of text
        /// 2 - Camel Case File names
        /// 3 - Untitled - stays untitled.
        /// </summary>
        [JsonIgnore]
        public string Title
        {
            get
            {
                string title = null;

                // try to find
                if (!String.IsNullOrEmpty(CurrentText))
                {
                    var lines = StringUtils.GetLines(CurrentText);
                    var lineCount = Math.Min(lines.Length, 5);


                    // # Header in first 5 lines
                    var line = lines.Take(lineCount).FirstOrDefault(ln => ln.Trim().StartsWith("# "));
                    if (!string.IsNullOrEmpty(line))
                    {
                        title = line.Trim().Substring(2);
                        return title;
                    }


                    // Front Matter Title
                    if (lines.Length > 2 && (lines[0] == "---" || lines[0] == "..."))
                    {
						var start = lines[0];
						string end = "---";

						var endBlock1 = CurrentText.IndexOf("---", 3);
						var endBlock2 = CurrentText.IndexOf("...", 3);
						if (endBlock2 > -1 && (endBlock2 == -1 || endBlock2 < endBlock1))
							end = "...";

						var block = mmFileUtils.ExtractString(CurrentText, start, end, returnDelimiters: true);
                        if (!string.IsNullOrEmpty(block))
                        {
                            title = StringUtils.ExtractString(block, "title: ", "\n").Trim();
                            if (!string.IsNullOrEmpty(title))
                                return title;
                        }
                    }

                }
                
                if (Filename == "Untitled")
                    return "Untitled";

                title = StringUtils.FromCamelCase(Path.GetFileNameWithoutExtension(Filename));

                return title;
            }
        }

        /// <summary>
        /// Determines whether documents are automatically saved in
        /// the background as soon as changes are made and you stop
        /// typiing for a second. This setting takes precendence over
        /// AutoSaveBackups.
        /// 
        /// Defaults to Configuration.AutoSaveDocuments
        /// </summary>
        [JsonIgnore]
        public bool AutoSaveDocuments { get; set; }

        /// <summary>
        /// Determines whether backups are automatically saved
        /// 
        /// Defaults to Configuration.AutoSaveBackups
        /// </summary>
        [JsonIgnore]
        public bool AutoSaveBackups { get; set; }

        /// <summary>
        /// Document encoding used when writing the document to disk.
        /// Default: UTF-8 without a BOM
        /// </summary>
        [JsonIgnore]
        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }
        private Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// Determines whether the active document has changes
        /// that have not been saved yet
        /// </summary>
        [JsonIgnore]
        public bool IsDirty
        {
            get { return _IsDirty; }
            set
            {
                if (value != _IsDirty)
                {
                    _IsDirty = value;
                    IsDirtyChanged?.Invoke(value);
                    OnPropertyChanged(nameof(IsDirty));
                    OnPropertyChanged(nameof(FilenameWithIndicator));                    
                }
            }
        }
        private bool _IsDirty;
        

        /// <summary>
        /// Determines whether the document is the active document
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (value == _isActive) return;
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
        private bool _isActive = false;

        /// <summary>
        /// This is the filename used when rendering this document to HTML
        /// </summary>
        [JsonIgnore]
        public string HtmlRenderFilename
        {
            get
            {
                string path = null;
                string file = null;

                if (Filename == "untitled")
                {
                    path = Path.GetDirectoryName(mmApp.Configuration.CommonFolder);
                    file = "__untitled.htm";
                }
                else
                {
                    path = Path.GetTempPath();  //Path.GetDirectoryName(Filename);
                    file = "_MarkdownMonster_Preview.html";                              
                }

                return Path.Combine(path, file);
            }
        }

        /// <summary>
        /// Event fired when 
        /// </summary>
        public event Action<bool> IsDirtyChanged;

        /// <summary>
        /// Holds the last preview window browser scroll position so it can be restored
        /// when refreshing the preview window.
        /// </summary>
        public int LastEditorLineNumber { get; set; }

        

        /// <summary>
        /// Holds the actively edited Markdown text
        /// </summary>
        [JsonIgnore]
        public string CurrentText
        {
            get { return _currentText; }
            set
            {
                bool isDirty = IsDirty;
                _currentText = value;

                if (isDirty)
                    AutoSaveAsync();
            }
        }
        private string _currentText;

        
        /// <summary>
        /// Holds the username and password
        /// </summary>
        [JsonIgnore]
        public SecureString Password
        {
            get { return _password; }
            set
            {
                if (_password == value) return;
                _password = value;                
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEncrypted));
            }
        }
        private SecureString _password;

        
        [JsonIgnore]
        /// <summary>
        /// Determines whether the file is encrypted
        /// </summary>
        public bool IsEncrypted => _password != null;

        /// <summary>
        /// The original text of the document since the last save
        /// operation. Updated whenever a document is saved.
        /// </summary>
        [JsonIgnore]
        public string OriginalText { get; set; }

        /// <summary>
        /// Window dispatcher to ensure we're synchronizing in
        /// the right context always.
        /// </summary>
        [JsonIgnore]
        public Dispatcher Dispatcher { get; set; }

        #region Read and Write Files

        public MarkdownDocument()
        {
            
        }

        /// <summary>
        /// Loads the markdown document into the CurrentText
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Load(string filename = null, SecureString password = null)
        {
            if (string.IsNullOrEmpty(filename))
                filename = Filename;

            if (password == null)
                password = Password;
            else
                Password = password;
            
            if (!File.Exists(filename))
            {
                FileCrc = null;
                return false;
            }

            UpdateCrc();
            GetFileEncoding();
            
            try
            {
                CurrentText = File.ReadAllText(filename,Encoding);

                if (password != null)
                {
                    if (CurrentText.StartsWith(ENCRYPTION_PREFIX))
                    {
                        string encrypted = CurrentText.Substring(ENCRYPTION_PREFIX.Length);
                        CurrentText = Encryption.DecryptString(encrypted, password.GetString());
                        if (string.IsNullOrEmpty(CurrentText))
                            return false;
                    }
                }

                OriginalText = CurrentText;
                AutoSaveBackups = mmApp.Configuration.AutoSaveBackups;
                AutoSaveDocuments = mmApp.Configuration.AutoSaveDocuments;

                Filename = filename;
            }
            catch
            {                
                return false;
            }
            
            return true;
        }

        private object _SaveLock = new object();
        private bool _IsSaving = false;

        /// <summary>
        /// Saves the CurrentText into the specified filename
        /// </summary>
        /// <param name="filename">filename to save (optional)</param>
        /// <param name="noBackupFileCleanup">if true doesn't delete backup files that might exist</param>
        /// <returns>true or false (no exceptions on failure)</returns>
        public bool Save(string filename = null, bool noBackupFileCleanup = false, SecureString password = null)
        {
            if (string.IsNullOrEmpty(filename))
                filename = Filename;

            try
            {                
                lock (_SaveLock)
                {
                    _IsSaving = true;

                    string fileText = CurrentText;

                    password = password ?? Password;

                    if (password != null)
                    {
                        fileText = ENCRYPTION_PREFIX + Encryption.EncryptString(fileText, password.GetString());
                        if (Password == null)
                            Password = password;
                    }

                    File.WriteAllText(filename, fileText, Encoding);
                    OriginalText = CurrentText;

                    UpdateCrc(filename);

                    if (!noBackupFileCleanup)
                        CleanupBackupFile();

                    IsDirty = false;

                    _IsSaving = false;
                }                
            }
            catch
            {                
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the file on disk is encrypted
        /// </summary>
        /// <param name="filename">Optional filename - if not specified Filename is used</param>
        /// <returns></returns>
        public bool IsFileEncrypted(string filename = null)
        {
            filename = filename ?? Filename;

            if (string.IsNullOrEmpty(filename))
                return false;

            
            using (var fs = File.OpenRead(Filename))
            {
                int count;
                var bytes = new char[ENCRYPTION_PREFIX.Length];

                using (var sr = new StreamReader(fs))
                {
                    count = sr.Read(bytes, 0, bytes.Length);
                }
                if (count == ENCRYPTION_PREFIX.Length)
                {
                    if (new string(bytes) == ENCRYPTION_PREFIX)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Cleans up after the file is closed by deleting
        /// the HTML render file.
        /// </summary>
        public void Close()
        {
            if (File.Exists(HtmlRenderFilename))
            {
                try
                {
                    File.Delete(HtmlRenderFilename);
                }
                catch { /* ignore */ }
            }
            CleanupBackupFile();
        }


        /// <summary>
        /// Writes the file with a retry
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="html"></param>
        public bool WriteFile(string filename, string html)
        {
            if (string.IsNullOrEmpty(filename))
                filename = Filename;

            int written = 0;
            while (written < 4) // try 4 times
            {
                try
                {
                    File.WriteAllText(filename, html, Encoding.UTF8);
                    written = 10;
                }              
                catch(Exception ex)
                {
                    // wait wind retry 3 times
                    Thread.Sleep(150);
                    written++;
                    if (written == 4)
                    {
                        mmApp.Log("Warning: Unable to write output file: " + filename + "\r\n" + ex.Message);
                        return false;                    
                    }
                }
            }
            return true;
        }

        #endregion

        #region Auto-Save Backups

        /// <summary>
        /// Creates a backup file
        /// </summary>
        /// <param name="filename"></param>
        public void AutoSaveAsync(string filename = null)
        {                    
            if (AutoSaveDocuments)
            {
                if (_IsSaving)
                    return;

                Task.Run(() =>
                {                    
                    filename = Filename;

                    if (filename == "untitled")
                        return;

                    try
                    {
                        lock (_SaveLock)
                        {
                            File.WriteAllText(filename, CurrentText, Encoding);
                            OriginalText = CurrentText;
                            UpdateCrc(filename);

                            if (Dispatcher != null)
                                // need dispatcher in order to handle the 
                                // hooked up OnPropertyChanged events that fire
                                // on the UI which otherwise fail.
                                Dispatcher.InvokeAsync(() => { IsDirty = false; });
                            else
                                IsDirty = false;
                        }
                    }
                    catch
                    {
                        /* ignore save error, write next cycle */
                    }
                });                
            }
            else if (AutoSaveBackups)
            {
                // fire and forget
                Task.Run(() =>
                {
                    if (string.IsNullOrEmpty(filename))
                        filename = BackupFilename;

                    if (Filename == "untitled" || Filename.Contains("saved.bak"))
                        return;

                    try
                    {
                        File.WriteAllText(filename, CurrentText, Encoding);
                    }
                    catch
                    { /* ignore save error, write next cycle */ }                
                });
            }
        }

        /// <summary>
        /// Cleans up the backup file and removes the timer
        /// </summary>
        /// <param name="filename"></param>
        public void CleanupBackupFile(string filename = null)
        {
            if (!AutoSaveBackups)
                return;

            if (string.IsNullOrEmpty(filename))
            {
                if (Filename == "untitled")
                    return;

                filename = BackupFilename;
            }
            
            try
            {
                File.Delete(filename);
            }
            catch { }
        }
        
        /// <summary>
        ///  Checks to see whether there's a backup file present
        /// </summary>
        /// <returns></returns>
        public bool HasBackupFile()
        {
            return Filename != "untitled" && File.Exists(BackupFilename);
        }

        #endregion

        #region File Information Manipulation

        /// <summary>
        /// Stores the CRC of the file as currently exists on disk
        /// </summary>
        /// <param name="filename"></param>
        public void UpdateCrc(string filename = null)
        {
            if (filename == null)
                filename = Filename;

            FileCrc = mmFileUtils.GetChecksumFromFile(filename);
        }


        /// <summary>
        /// Checks to see if the CRC has changed
        /// </summary>
        /// <returns></returns>
        public bool HasFileCrcChanged()
        {
            if (string.IsNullOrEmpty(Filename) || !File.Exists(Filename) || string.IsNullOrEmpty(FileCrc))            
                return false;
            
            var crcNow = mmFileUtils.GetChecksumFromFile(Filename);
            return crcNow != FileCrc;
        }


        /// <summary>
        /// Determines whether text has changed from original.
        /// 
        /// This method exists to explicitly check the dirty
        /// state which can be set from a number of sources.
        /// </summary>
        /// <param name="currentText">Text to compare to original text. If omitted uses CurrentText property to compare</param>
        /// <returns></returns>
        public bool HasFileChanged(string currentText = null)
        {
            if (currentText != null)
                CurrentText = currentText;

            IsDirty = CurrentText != OriginalText;
            return IsDirty;
        }

        /// <summary>
        /// Retrieve the file encoding for a given file so we can capture
        /// and store the Encoding when writing the file back out after
        /// editing.
        /// 
        /// Default is Utf-8 (w/ BOM). If file without BOM is read it is
        /// assumed it's UTF-8.
        /// </summary>
        /// <param name="filename">file to get encoding from</param>
        /// <returns></returns>
        public void GetFileEncoding(string filename = null)
        {
            if (filename == null)
                filename = Filename;
                  
            Encoding = mmFileUtils.GetFileEncoding(filename);
        }
        #endregion

        #region Output Generation

        /// <summary>
        /// Renders markdown of the current document text into raw HTML
        /// </summary>
        /// <param name="markdown">markdown to render</param>
        /// <param name="renderLinksExternal">Determines whether links have a target='top' attribute</param>
        /// <param name="usePragmaLines">renders line numbers into html output as ID tags for editor positioning</param>
        /// <returns></returns>
        public string RenderHtml(string markdown = null, bool renderLinksExternal = false, bool usePragmaLines = false)
        {
            if (string.IsNullOrEmpty(markdown))
                markdown = CurrentText;
            
            var parser = MarkdownParserFactory.GetParser(usePragmaLines: usePragmaLines,                                                         
                                                         forceLoad: true, 
                                                         parserAddinId: mmApp.Configuration.MarkdownOptions.MarkdownParserName);            
            var html = parser.Parse(markdown);


            if (!string.IsNullOrEmpty(html) && !UnlockKey.IsRegistered() && mmApp.Configuration.ApplicationUpdates.AccessCount > 20)
            {
                html += @"
<div style=""margin-top: 30px;margin-bottom: 10px;font-size: 0.8em;border-top: 1px solid #eee;padding-top: 8px;opacity: 0.75;""
     title=""This message doesn't display in the registered version of Markdown Monster."">
    <img src=""https://markdownmonster.west-wind.com/favicon.png""
         style=""height: 20px;float: left; margin-right: 10px;opacity: 0.75;""/>
    created with the free version of
    <a href=""https://markdownmonster.west-wind.com"" 
       target=""top"">Markdown Monster</a> 
</div>
";
            }

            return html;
        }

        /// <summary>
        /// Renders the HTML to a file with a related template
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="filename"></param>
        /// <param name="renderLinksExternal"></param>
        /// <returns></returns>
        public string RenderHtmlToFile(string markdown = null, string filename = null, bool renderLinksExternal = false, string theme = null, bool usePragmaLines = false)
        {
            string markdownHtml = RenderHtml(markdown, renderLinksExternal, usePragmaLines);

            if (string.IsNullOrEmpty(filename))
                filename = HtmlRenderFilename;

            if (string.IsNullOrEmpty(theme))
                theme = mmApp.Configuration.RenderTheme;

            var themePath = Path.Combine(Environment.CurrentDirectory, "PreviewThemes\\" + theme);
            var docPath = Path.GetDirectoryName(Filename) + "\\";

            if (!Directory.Exists(themePath))
            {
                mmApp.Configuration.RenderTheme = "Dharkan";
                themePath = Path.Combine(Environment.CurrentDirectory, "PreviewThemes\\Dharkan");
                theme = "Dharkan";
            }

            string themeHtml = null;
            try
            {
                themeHtml = File.ReadAllText(themePath + "\\theme.html");
                themePath = themePath + "\\";
            }
            catch (FileNotFoundException)
            {
                // reset to default
                mmApp.Configuration.RenderTheme = "Dharkan";
                themeHtml = "<html><body><h3>Invalid Theme or missing files. Resetting to Dharkan.</h3></body></html>";
            }
            var html = themeHtml.Replace("{$themePath}", themePath)
                .Replace("{$docPath}", docPath)
                .Replace("{$markdownHtml}", markdownHtml);

            if (!WriteFile(filename, html))
                return null;

            return html;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        ~MarkdownDocument()
        {            
            Close();
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Filename))
                return "No document loaded";

            return Path.GetFileName(Filename);
        }

        
    }

    static class SecureStringExtensions
    {
        public static string GetString(
            this SecureString source)
        {
            string result = null;
            int length = source.Length;
            IntPtr pointer = IntPtr.Zero;
            char[] chars = new char[length];

            try
            {
                pointer = Marshal.SecureStringToBSTR(source);
                Marshal.Copy(pointer, chars, 0, length);

                result = string.Join("", chars);
            }
            finally
            {
                if (pointer != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(pointer);
                }
            }

            return result;
        }
    }
}