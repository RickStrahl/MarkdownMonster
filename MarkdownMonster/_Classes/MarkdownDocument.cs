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
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Westwind.Utilities;

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

        /// <summary>
        /// Holds the last preview window browser scroll position so it can be restored
        /// when refreshing the preview window.
        /// </summary>
        public int LastBrowserScrollPosition { get; set; }        


        /// <summary>
        /// Markdown style used on this document. Not used at the moment
        /// </summary>
        public MarkdownStyles MarkdownStyle = MarkdownStyles.GitHub;

        /// <summary>
        /// Returns the filename with a dirty indicator (*) if the
        /// document has changed
        /// </summary>
        public string FilenameWithIndicator
        {
            get
            {
                return Path.GetFileName(Filename) + (IsDirty ? "*" : "");                
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
                    path = Path.GetDirectoryName(Filename);
                    file = "__" + Path.ChangeExtension(Path.GetFileName(Filename), "htm");                    
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

            CurrentText = File.ReadAllText(filename);

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

            File.WriteAllText(filename, CurrentText);
            IsDirty = false;

            return true;
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
        /// Renders markdown of the current document text into HTML
        /// </summary>
        /// <param name="markdown">markdown to render</param>
        /// <param name="renderLinksExternal">Determines whether links have a target='top' attribute</param>
        /// <returns></returns>
        public string RenderHtml(string markdown = null, bool renderLinksExternal = false)
        {
            if (string.IsNullOrEmpty(markdown))
                markdown = CurrentText;
            
            var parser = MarkdownParser.GetParser(MarkdownStyle,renderLinksExternal);            
            return parser.Parse(markdown);
        }

        public string RenderHtmlToFile(string markdown = null, string filename = null, bool renderLinksExternal = false)
        {
            string markdownHtml = RenderHtml(markdown,renderLinksExternal);

            if (string.IsNullOrEmpty(filename))
                filename = HtmlRenderFilename;

            var themePath = Path.Combine(Environment.CurrentDirectory, "PreviewThemes\\" + 
                mmApp.Configuration.RenderTheme + "\\");
            var themeHtml = File.ReadAllText(themePath + "theme.html");

            var html = themeHtml.Replace("{$themePath}", themePath);
            html = html.Replace("{$markdownHtml}",markdownHtml);

            File.WriteAllText(filename, html);
                return html;
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