#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 05/15/2016
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster;

using Westwind.Utilities.Configuration;

namespace WeblogAddin
{
    //public class xWeblogApp
    //{
    //    public static WeblogAddinConfiguration Current;


    //    static WeblogApp()
    //    {
    //        Current = new WeblogAddinConfiguration();
    //        Current.Initialize();
    //    }
    //}

    public class WeblogAddinConfiguration : AppConfiguration, INotifyPropertyChanged
    {
        public static WeblogAddinConfiguration Current;

        static WeblogAddinConfiguration()
        {
            Current = new WeblogAddinConfiguration();
            Current.Initialize();
        }

        public WeblogAddinConfiguration()
        {
            Weblogs = new Dictionary<string, WeblogInfo>();
        }



        public Dictionary<string,WeblogInfo> Weblogs
        {
            get { return _weblogs; }
            set
            {
                if (value == _weblogs) return;
                _weblogs = value;
                OnPropertyChanged(nameof(Weblogs));
            }
        }
        private Dictionary<string,WeblogInfo> _weblogs;
        


        public string LastWeblogAccessed
        {
            get { return _lastWeblogAccessed; }
            set
            {
                if (value == _lastWeblogAccessed) return;
                _lastWeblogAccessed = value;
                OnPropertyChanged(nameof(LastWeblogAccessed));
            }
        }
        private string _lastWeblogAccessed;


        public string PostsFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_postsFolder))
                {
                    var basePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "DropBox");
                    if (!Directory.Exists(basePath))
                        basePath = mmApp.Configuration.CommonFolder;

                    basePath = Path.Combine(basePath, "Markdown Monster Weblog Posts");
                    if (!Directory.Exists(basePath))
                        Directory.CreateDirectory(basePath);
                    _postsFolder = basePath;
                }
                return _postsFolder;
            }
            set
            {
                _postsFolder = value;
            }
        }
        private string _postsFolder;


        /// <summary>
        /// When true renders links to open externally.
        /// </summary>
        public bool RenderLinksOpenExternal
        {
            get { return _RenderLinksOpenExternal; }
            set
            {
                if (value == _RenderLinksOpenExternal) return;
                _RenderLinksOpenExternal = value;
                OnPropertyChanged(nameof(RenderLinksOpenExternal));
            }
        }
        private bool _RenderLinksOpenExternal = true;

        
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var provider = new JsonFileConfigurationProvider<WeblogAddinConfiguration>()
            {
                JsonConfigurationFile = Path.Combine(mmApp.Configuration.CommonFolder, "WebLogAddIn.json")                                
            };

            if (!File.Exists(provider.JsonConfigurationFile))
            {
                if (!Directory.Exists(Path.GetDirectoryName(provider.JsonConfigurationFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(provider.JsonConfigurationFile));
            }

            return provider;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}