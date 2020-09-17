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
using System.Runtime.CompilerServices;
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

            FrontMatterTemplate = @"---
Title: {0}
Published: {1:dd/MM/yyyy}
Tags:
- Keyword1
---
";
        }

        
        /// <summary>
        /// Keyboard shortcut for this addin.
        /// </summary>
        public string KeyboardShortcut
        {
            get { return _keyboardShortcut; }
            set
            {
                if (_keyboardShortcut == value) return;
                _keyboardShortcut = value;
                OnPropertyChanged(nameof(KeyboardShortcut));
            }
        }
        private string _keyboardShortcut = string.Empty;

        /// <summary>
        /// Determines the folder were new Posts are stored
        /// </summary>
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
                        basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

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
        /// Determines if the Post Images are replaced in
        /// the original Markdown document to reflect the
        /// new online URL
        /// </summary>
        public bool ReplacePostImagesWithOnlineUrls
        {
            get { return _replacePostImagesWithOnlineUrls; }
            set
            {
                if (value == _replacePostImagesWithOnlineUrls) return;
                _replacePostImagesWithOnlineUrls = value;
                OnPropertyChanged(nameof(ReplacePostImagesWithOnlineUrls));
            }
        }
        private bool _replacePostImagesWithOnlineUrls;



        /// <summary>
        /// Keeps track of the last WebLog you used to post a post to a site
        /// </summary>
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

        
        /// <summary>
        /// Determines whether the post is saved to disk
        /// automatically when posted to the blog or when
        /// using Save MetaData
        /// </summary>
        public bool AutoSavePost
        {
            get { return _autoSavePost; }
            set
            {
                if (value == _autoSavePost) return;
                _autoSavePost = value;
                OnPropertyChanged(nameof(AutoSavePost));
            }
        }
        private bool _autoSavePost;

        /// <summary>
        /// When true renders links to open external window with Target="blank"
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

        private bool _addFrontMatterToNewBlogPost;

        /// <summary>
        /// If true adds a Front Matter header to the beginning of new blog posts
        /// using the FrontMatterTemplate.
        /// </summary>
        public bool AddFrontMatterToNewBlogPost
        {
            get { return _addFrontMatterToNewBlogPost; }
            set
            {
                if (_addFrontMatterToNewBlogPost == value) return;
                _addFrontMatterToNewBlogPost = value;
                OnPropertyChanged(nameof(AddFrontMatterToNewBlogPost));
            }
        }

           
        /// <summary>
        /// A string that comprises text to be inserted at the beginning of a post.
        /// By default this template is a Front Matter template but it can be 
        /// any text based string.
        /// </summary>
        public string FrontMatterTemplate
        {
            get { return _frontMatterTemplate; }
            set
            {
                if (_frontMatterTemplate == value) return;
                _frontMatterTemplate = value;
                OnPropertyChanged(nameof(FrontMatterTemplate));
            }
        }
        private string _frontMatterTemplate;

        
        public Dictionary<string, WeblogInfo> Weblogs
        {
            get { return _weblogs; }
            set
            {
                if (value == _weblogs) return;
                _weblogs = value;
                OnPropertyChanged(nameof(Weblogs));
            }
        }
        private Dictionary<string, WeblogInfo> _weblogs;



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

        //public override bool Write()
        //{
        //    foreach (var blog in Weblogs.Values)
        //    {
        //        blog.Password = blog.EncryptPassword(blog.Password);
        //    }

        //    bool result = base.Write();


        //    foreach (var blog in Weblogs.Values)
        //    {
        //        blog.Password = blog.DecryptPassword(blog.Password);
        //    }
        //    if (!result)
        //        return false;
            
        //    return true;
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
