using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CookComputing.XmlRpc;
using FontAwesome.WPF;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MarkdownMonster;
using MarkdownMonster.Windows;
using WebLogAddin;
using WebLogAddin.Medium;
using WebLogAddin.MetaWebLogApi;
using Westwind.Utilities;

namespace WeblogAddin
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class WebLogForm : MetroWindow
    {
        public WeblogAddinModel Model { get; set;  }


        #region Startup and Shutdown

        public WebLogForm(WeblogAddinModel model)
        {
            Model = model;

            model.ActivePostMetadata = new WeblogPostMetadata();
            model.ActiveWeblogInfo = new WeblogInfo();

            model.Window = this;
            
            mmApp.SetTheme(mmApp.Configuration.ApplicationTheme);

            InitializeComponent();

            DataContext = Model;

            // Code bindings
            ComboWeblogType.ItemsSource = Enum.GetValues(typeof(WeblogTypes)).Cast<WeblogTypes>();            

            Loaded += WebLogStart_Loaded;
            Closing += WebLogStart_Closing;

            
        }

        private void WebLogStart_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //Model.LoadWebLognames();

            var editor = Model.AppModel.ActiveEditor;
            if (editor == null)
                return;

            var markdown = editor.GetMarkdown();
            Model.ActivePostMetadata = WeblogPostMetadata.GetPostConfigFromMarkdown(markdown,Model.ActivePost, Model.ActiveWeblogInfo);
         
            var lastBlog = WeblogAddinConfiguration.Current.LastWeblogAccessed;

            if (string.IsNullOrEmpty(Model.ActivePostMetadata.WeblogName))
                Model.ActivePostMetadata.WeblogName = lastBlog;
        }


        private void WebLogStart_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // save settings
            WeblogAddinConfiguration.Current.Write();
        }

        #endregion

        #region Button Handlers
        private async void ButtonPostBlog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ShowStatus("Uploading Blog post...");
            SetStatusIcon(FontAwesome.WPF.FontAwesomeIcon.Upload, Colors.Orange, true);


            if (string.IsNullOrEmpty(Model.ActivePostMetadata.WeblogName))
            {
                MessageBox.Show("Please select or create a Weblog to post to before posting.",
                    "No Weblog Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Update the Markdown document first
            string markdown =  Model.ActivePostMetadata.SetPostYaml();
            Model.AppModel.ActiveEditor.SetMarkdown(markdown);
            
            WeblogAddinConfiguration.Current.LastWeblogAccessed = Model.ActivePostMetadata.WeblogName;

            var window = Model.AppModel.Window;
            
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    // Then send the post - it will re-read the new values
                    if (Model.Addin.SendPost(Model.ActiveWeblogInfo, Model.ActivePostMetadata.IsDraft))
                        Close();
                    else
                        window.ShowStatus("Failed to upload blog post.", 5000);

                }, System.Windows.Threading.DispatcherPriority.Background);                
            }
            finally
            {
                ShowStatus();
                window.ShowStatus("Blog post uploaded successfully.", 5000);
                SetStatusIcon();
            }
        }

        void ButtonSaveMeta_Click(object sender, RoutedEventArgs e)
        {                        
            // Update the Markdown document first
            string markdown = Model.ActivePostMetadata.SetPostYaml();
            Model.AppModel.ActiveEditor.SetMarkdown(markdown);            
        }

        private void ButtonNewPost_Click(object sender, System.Windows.RoutedEventArgs e)
        {
         
            string title = Model.NewTitle;
            if (string.IsNullOrEmpty(title))
                return;
            string weblogName = Model.Configuration.LastWeblogAccessed;

            Model.Addin.CreateNewPostOnDisk(Model.NewTitle, Model.NewFilename, weblogName);                    

            Close();
        }


        private void TextWeblogPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextWeblogPassword.Password))
            {
                Model.ActiveWeblogInfo.Password = TextWeblogPassword.Password;
                TextWeblogPassword.Password = string.Empty;
            }
        }

        private void TextWeblogToken_LostFocus(object sender, RoutedEventArgs e)
        {
            Model.ActiveWeblogInfo.AccessToken = TextWeblogToken.Password;
            TextWeblogToken.Password = string.Empty;            
        }

        private void ComboWebLogName_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {            
            TextWeblogPassword.Password = "";            
        }

        private void Button_DeleteWeblog(object sender, RoutedEventArgs e)
        {
            if (Model.ActiveWeblogInfo != null)
            {
                var id = Model.ActiveWeblogInfo.Id;
                if (Model.Configuration.Weblogs.Count > 1)
                    Model.ActiveWeblogInfo = Model.Configuration.Weblogs.FirstOrDefault().Value;

                Model.Configuration.Weblogs.Remove(id);                                
            }
        }

        private void Button_NewWeblog(object sender, RoutedEventArgs e)
        {
            Model.ActiveWeblogInfo = new WeblogInfo()
            {                 
                Name = "New Weblog"
            };
            Model.Configuration.Weblogs.Add(Model.ActiveWeblogInfo.Id,Model.ActiveWeblogInfo);
            Model.Configuration.OnPropertyChanged("Weblogs");

            this.ComboWebLogName.SelectedValue = Model.Configuration.Weblogs[Model.ActiveWeblogInfo.Id];
        }

        private async void Button_DownloadPosts_Click(object sender, RoutedEventArgs e)
        {
            WeblogInfo weblogInfo = Model.ActiveWeblogInfo;

            var wrapper = new MetaWeblogWrapper(weblogInfo.ApiUrl,
                weblogInfo.Username,
                mmApp.DecryptString(weblogInfo.Password),
                weblogInfo.BlogId);


            Model.Configuration.LastWeblogAccessed = weblogInfo.Name;

            Dispatcher.Invoke(() =>
            {
                Model.PostList = new List<Post>();
                SetStatusIcon(FontAwesomeIcon.Download, Colors.Orange,true); 
                ShowStatus("Downloading last " + Model.NumberOfPostsToRetrieve + " posts...");                    
            });

            WindowUtilities.DoEvents();

            List<Post> posts = null;
            try
            {
                bool result = await Task.Run(() =>
                {
                    posts = wrapper.GetRecentPosts(Model.NumberOfPostsToRetrieve).ToList();
                    return false;
                });
            }
            catch (XmlRpcException ex)
            {
                string message = ex.Message;
                if (message == "Not Found")
                    message = "Invalid Blog API Url:\r\n" + weblogInfo.ApiUrl;
                MessageBox.Show("Unable to download posts:\r\n" + message,mmApp.ApplicationName,
                    MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to download posts:\r\n" + ex.Message,mmApp.ApplicationName,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            for (int i = 0; i < posts.Count; i++)
            {
                var post = posts[i];
                post.mt_excerpt = StringUtils.TextAbstract(post.mt_excerpt, 220);
            }

            WindowUtilities.DoEvents();

            Dispatcher.Invoke(() =>
            {
                ShowStatus(posts.Count + " posts downloaded.",5000);
                SetStatusIcon();
                Model.PostList = posts;
            });

                       
        }

        private void ListViewPosts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CreateDownloadedPost();
        }
        private void ListViewPosts_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CreateDownloadedPost();
        }

        #endregion

        #region StatusBar

        public void ShowStatus(string message = null, int milliSeconds = 0)
        {
            if (message == null)
                message = "Ready";

            StatusText.Text = message;

            if (milliSeconds > 0)
            {
                var t = new Timer(new TimerCallback((object win) =>
                {
                    var window = win as WebLogForm;
                    if (window == null)
                        return;

                    window.Dispatcher.Invoke(() => { window.ShowStatus(null, 0); });
                }), this, milliSeconds, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Status the statusbar icon on the left bottom to some indicator
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        /// <param name="spin"></param>
        public void SetStatusIcon(FontAwesomeIcon icon, Color color, bool spin = false)
        {
            StatusIcon.Icon = icon;
            StatusIcon.Foreground = new SolidColorBrush(color);
            if (spin)
                StatusIcon.SpinDuration = 30;
            StatusIcon.Spin = spin;
        }

        /// <summary>
        /// Resets the Status bar icon on the left to its default green circle
        /// </summary>
        public void SetStatusIcon()
        {
            StatusIcon.Icon = FontAwesomeIcon.Circle;
            StatusIcon.Foreground = new SolidColorBrush(Colors.Green);
            StatusIcon.Spin = false;
            StatusIcon.SpinDuration = 0;
            StatusIcon.StopSpin();
        }
        #endregion

        private void CreateDownloadedPost()
        {

            var item = ListViewPosts.SelectedItem as Post;
            if (item == null)
                return;

            string postId = item.PostId.ToString();
            WeblogInfo weblogInfo = Model.ActiveWeblogInfo;

            
            Post post = null;

            if (weblogInfo.Type == WeblogTypes.MetaWeblogApi)
            {
                var wrapper = new MetaWeblogWrapper(weblogInfo.ApiUrl,
                    weblogInfo.Username,
                    mmApp.DecryptString(weblogInfo.Password),
                    weblogInfo.BlogId);

                try
                {
                    post = wrapper.GetPost(postId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to download post.\r\n\r\n" + ex.Message);
                    return;
                }                
            }
            else
            {
                var wrapper = new WordPressWrapper(weblogInfo.ApiUrl,
                    weblogInfo.Username,
                    mmApp.DecryptString(weblogInfo.Password));

                try
                {
                    post = wrapper.GetPost(postId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to download post.\r\n\r\n" + ex.Message);
                    return;
                }               
            }

            Model.Addin.CreateDownloadedPostOnDisk(post, weblogInfo.Name);

            Close();        
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonApiUrlInfo_Click(object sender, RoutedEventArgs e)
        {
            ShellUtils.GoUrl("http://markdownmonster.west-wind.com/docs/_4rg0qzg1i.htm");
        }

        private void ButtonDiscoverEndpoint_Click(object sender, RoutedEventArgs e)
        {
            var discover = new BlogEndpointDiscovery();

            var url = Model.ActiveWeblogInfo.ApiUrl;

            ShowStatus("Checking Endpoint Url...");

            if (discover.CheckRpcEndpoint(url))
            {
                ShowStatus("The Weblog Endpoint is a valid RPC endpoint.");
                return;
            }

            var blogInfo = discover.DiscoverBlogEndpoint(url, Model.ActiveWeblogInfo.BlogId as string, Model.ActiveWeblogInfo.Type.ToString());

            if (blogInfo.HasError)
            {
                MessageBox.Show(blogInfo.ErrorMessage, "Unable to discover Endpoint Url",MessageBoxButton.OK,MessageBoxImage.Warning);
                ShowStatus("Endpoint discovery failed: " + blogInfo.ErrorMessage, 6000);
                return;
            }

            Model.ActiveWeblogInfo.ApiUrl = blogInfo.ApiUrl;
            Model.ActiveWeblogInfo.BlogId = blogInfo.BlogId;
            if (blogInfo.BlogType == "MetaWeblog" || blogInfo.BlogType == "MetaWeblogApi")
                Model.ActiveWeblogInfo.Type = WeblogTypes.MetaWeblogApi;
            else if (blogInfo.BlogType == "WordPress")
                Model.ActiveWeblogInfo.Type = WeblogTypes.Wordpress;
            else
                Model.ActiveWeblogInfo.Type = WeblogTypes.Unknown;

            ShowStatus("Weblog API Endpoint Url found and updated...", 6000);
        }

        private void ComboWeblogType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // since we
            Model.OnPropertyChanged(nameof(Model.IsTokenVisible));
            Model.OnPropertyChanged(nameof(Model.IsUserPassVisible));           
        }

        private void DropDownButton_Click(object sender, RoutedEventArgs e)
        {
            ShowStatus("Getting Blog listing information from service...");
            WindowUtilities.DoEvents();

            var context = Resources["BlogsContextMenu"] as ContextMenu;
            context.Items.Clear();

            IEnumerable<UserBlog> blogs = null;

            try
            {
                if (Model.ActiveWeblogInfo.Type == WeblogTypes.Medium)
                {
                    var client = new MediumApiClient(Model.ActiveWeblogInfo);
                    blogs = client.GetBlogs();
                    if (blogs == null)
                        ShowStatus("Failed to get blog listing: " + client.ErrorMessage,6000);
                }
                else if (Model.ActiveWeblogInfo.Type == WeblogTypes.MetaWeblogApi ||
                         Model.ActiveWeblogInfo.Type == WeblogTypes.Wordpress)
                {
                    var client = new MetaWebLogWordpressApiClient(Model.ActiveWeblogInfo);
                    blogs = client.GetBlogs();
                    if (blogs == null)
                        ShowStatus("Failed to get blog listing: " + client.ErrorMessage, 6000);
                } 
            }
            catch (Exception ex)
            {
                ShowStatus("Failed to get blogs: " + ex.Message, 6000);
                return;
            }

            ShowStatus("Blogs retrieved...", 2000);

            if (blogs == null)
                return;

            string blogId = Model.ActiveWeblogInfo.BlogId as string;

            if (!string.IsNullOrEmpty(blogId) && !blogs.Any(b => blogId == b.BlogId as string))
                context.Items.Add(new MenuItem {Header = blogId, Tag = blogId});

            foreach (var blog in blogs)
            {
                var item = new MenuItem()
                {
                    Header = blog.BlogName,
                    Tag = blog.BlogId,
                };
                item.Click += (s,ea) =>
                {                    
                    var mitem = s as MenuItem;
                    if (mitem == null)
                        return;

                    Model.ActiveWeblogInfo.BlogId = mitem.Tag as string;
                    context.Items.Clear();
                };
                context.Items.Add(item);
            }            
        }

        private void Hyperlink_UrlNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            ShellUtils.GoUrl("https://markdownmonster.west-wind.com/docs/_4rg0qzg1i.htm");
        }
    }
}
