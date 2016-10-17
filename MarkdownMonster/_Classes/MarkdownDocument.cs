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
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MarkdownMonster._Classes.Utilities;
using Newtonsoft.Json;

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

        public string FileCrc { get; set; }

        /// <summary>
        /// Holds the last preview window browser scroll position so it can be restored
        /// when refreshing the preview window.
        /// </summary>
        public int LastBrowserScrollPosition { get; set; }        


        /// <summary>
        /// Markdown style used on this document. Not used at the moment
        /// </summary>
        //public MarkdownStyles MarkdownStyle = MarkdownStyles.GitHub;


        /// <summary>
        /// Returns the filename with a dirty indicator (*) if the
        /// document has changed
        /// </summary>
        [JsonIgnore]
        public string FilenameWithIndicator
        {
            get
            {
                return Path.GetFileName(Filename) + (IsDirty ? "*" : "");                
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
                if (Filename == "untitled")
                    return FilenameWithIndicator;

                string path = Path.GetFileName(Path.GetDirectoryName(Filename));

                return Path.GetFileName(Filename) + "  –  " + path  + (IsDirty ? "*" : ""); 
            }
        }

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
        /// Holds the actively edited Markdown text
        /// </summary>
        [JsonIgnore]
        public string CurrentText { get; set; }


        /// <summary>
        /// Loads the markdown document into the CurrentText
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Load(string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
                filename = Filename;

            if (!File.Exists(filename))
                return false;

            UpdateCrc();
            try
            {
                CurrentText = File.ReadAllText(filename);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves the CurrentText into the specified filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Save(string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
                filename = Filename;

            try
            {
                File.WriteAllText(filename, CurrentText, Encoding.UTF8);
                IsDirty = false;

                UpdateCrc(filename);

                return true;
            }
            catch {  }
            
            return false;
        }

        /// <summary>
        /// Checks to see if the CRC has changed
        /// </summary>
        /// <returns></returns>
        public bool HasFileCrcChanged()
        {
            if (string.IsNullOrEmpty(Filename) || !File.Exists(Filename) || string.IsNullOrEmpty(FileCrc))
                return false;

            var crcNow = ChecksumHelper.GetChecksumFromFile(Filename);
            return crcNow != FileCrc;
        }


        public void UpdateCrc(string filename = null)
        {
            if (filename == null)
                filename = Filename;

            FileCrc = ChecksumHelper.GetChecksumFromFile(filename);
        }

        /// <summary>
        /// Cleans up after the file is closed by deleting
        /// the HTML render file.
        /// </summary>
        public void Close()
        {
            if (File.Exists(HtmlRenderFilename))
                File.Delete(HtmlRenderFilename);
        }
        
        /// <summary>
        /// Renders markdown of the current document text into raw HTML
        /// </summary>
        /// <param name="markdown">markdown to render</param>
        /// <param name="renderLinksExternal">Determines whether links have a target='top' attribute</param>
        /// <returns></returns>
        public string RenderHtml(string markdown = null, bool renderLinksExternal = false)
        {
            if (string.IsNullOrEmpty(markdown))
                markdown = CurrentText;
            
            var parser = MarkdownParserFactory.GetParser(renderLinksExternal);            
            return parser.Parse(markdown);
        }

        /// <summary>
        /// Renders the HTML to a file with a related template
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="filename"></param>
        /// <param name="renderLinksExternal"></param>
        /// <returns></returns>
        public string RenderHtmlToFile(string markdown = null, string filename = null, bool renderLinksExternal = false, string theme = null)
        {
            string markdownHtml = RenderHtml(markdown, renderLinksExternal);

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

        /// <summary>
        /// Output routWrites the file with a hidden attribute
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
                        throw new ApplicationException("Unable to write output file:  " + filename + "\r\n" + ex.Message);
                    }
                }
            }
            return true;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        ~MarkdownDocument()
        {
            this.Close();
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Filename))
                return "No document loaded";

            return Path.GetFileName(Filename);
        }

        
    }
}