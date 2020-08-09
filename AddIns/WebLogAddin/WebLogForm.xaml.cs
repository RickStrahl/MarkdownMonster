using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CookComputing.XmlRpc;
using MahApps.Metro.Controls;
using MarkdownMonster;
using MarkdownMonster.Windows;
using WebLogAddin;
using WebLogAddin.LocalJekyll;
using WebLogAddin.Medium;
using WebLogAddin.MetaWebLogApi;
using Westwind.Utilities;

namespace WeblogAddin
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class WeblogForm : MetroWindow
    {
        public WeblogAddinModel Model { get; set; }

        public StatusBarHelper StatusBar { get; }


        #region Startup and Shutdown

        public WeblogForm(WeblogAddinModel model)
        {
            Model = model;
            model.ActivePostMetadata = new WeblogPostMetadata();

            model.ActiveWeblogInfo = new WeblogInfo();

            model.Window = this;

            InitializeComponent();

            mmApp.SetThemeWindowOverride(this);
            if (mmApp.Configuration.ApplicationTheme == Themes.Light)
                TabControl.Background = (SolidColorBrush) Resources["LightThemeTitleBackground"];

            Loaded += WebLogStart_Loaded;
            Closing += WebLogStart_Closing;

            StatusBar = new StatusBarHelper(StatusText, StatusIcon);
        }

        private void WebLogStart_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

            // Code bindings
            ComboWeblogType.ItemsSource = Enum.GetValues(typeof(WeblogTypes)).Cast<WeblogTypes>();

            var editor = Model.AppModel.ActiveEditor;

            if (editor == null)
            {
                DataContext = Model;
                return;
            }

            var markdown = editor.GetMarkdown();
            Model.ActivePostMetadata =
                WeblogPostMetadata.GetPostConfigFromMarkdown(markdown, Model.ActivePost, Model.ActiveWeblogInfo);

            Model.MetadataCustomFields =
                new ObservableCollection<CustomField>(
                    Model.ActivePostMetadata.CustomFields.Select(kv => kv.Value));

            var lastBlog = WeblogAddinConfiguration.Current.LastWeblogAccessed;

            if (string.IsNullOrEmpty(Model.ActivePostMetadata.WeblogName))
                Model.ActivePostMetadata.WeblogName = lastBlog;


            // have to do this here otherwise MetadataCustomFields is not updating in model
            DataContext = Model;
        }


        private void WebLogStart_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // save settings
            if (Model.ActiveWeblogInfo != null)
                Model.Configuration.LastWeblogAccessed = Model.ActiveWeblogInfo.Name;
            Model.Configuration.Write();
        }

        #endregion

        #region Button Handlers

        private async void ButtonPostBlog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StatusBar.ShowStatusProgress("Uploading Blog post...");

            GetCustomFieldsFromObservableCollection();


            if (string.IsNullOrEmpty(Model.ActivePostMetadata.WeblogName))
            {
                MessageBox.Show("Please select or create a Weblog to post to before posting.",
                    "No Weblog Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                if (Model.WeblogNames.Count == 0)
                    ActiveWebLogsPageForNew();
                else
                    ComboWebLogSite.Focus();

                StatusBar.ShowStatus();
                return;
            }

            var editor = Model.AppModel.ActiveEditor;
            // Update the Markdown document first
            string markdown = Model.ActivePostMetadata.SetPostYamlFromMetaData();
            editor.SetMarkdown(markdown);
            editor.SaveDocument();

            WeblogAddinConfiguration.Current.LastWeblogAccessed = Model.ActivePostMetadata.WeblogName;

            var window = Model.AppModel.Window;

            try
            {
                bool result = await Model.Addin.SendPost(Model.ActiveWeblogInfo,
                    Model.ActivePostMetadata.PostStatus == "draft");
                if (result)
                {
                    Close();
                    window.ShowStatusSuccess($"Blog post '{Model.ActivePost.Title}` uploaded.");
                }
                else
                    window.ShowStatusError("Upload of blog post failed.");
            }
            finally
            {
                StatusBar.ShowStatus();
            }
        }

        void ButtonSaveMeta_Click(object sender, RoutedEventArgs e)
        {
            GetCustomFieldsFromObservableCollection();

            // Update the Markdown document first
            string markdown = Model.ActivePostMetadata.SetPostYamlFromMetaData();
            Model.AppModel.ActiveEditor.SetMarkdown(markdown, updateDirtyFlag: true);
            Model.AppModel.ActiveEditor.SaveDocument();

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
            if (!string.IsNullOrEmpty(TextWeblogPassword.Password) && Model.ActiveWeblogInfo != null)
            {
                Model.ActiveWeblogInfo.Password = TextWeblogPassword.Password;
                TextWeblogPassword.Password = string.Empty;
            }
        }

        private void TextWeblogToken_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Model.ActiveWeblogInfo != null)
            {
                Model.ActiveWeblogInfo.AccessToken = TextWeblogToken.Password;
                TextWeblogToken.Password = string.Empty;
            }
        }

        private void ComboWebLogName_SelectionChanged(object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
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
            Model.ActiveWeblogInfo = new WeblogInfo();
        }

        private async void Button_DownloadPosts_Click(object sender, RoutedEventArgs e)
        {
            WeblogInfo weblogInfo = Model.ActiveWeblogInfo;

            if (weblogInfo.Type == WeblogTypes.MetaWeblogApi || weblogInfo.Type == WeblogTypes.Wordpress )
                await DownloadMetaWeblogAndWordPressPosts();
            else if(weblogInfo.Type == WeblogTypes.LocalJekyll)
            {
                await DownloadJekyllPosts();
            }
            else
            {
                StatusBar.ShowStatusError($"The Weblog {weblogInfo.Name} doesn't support downloading of posts.");
            }
        }

        public Task DownloadJekyllPosts()
        {
            var publisher = new LocalJekyllPublisher(Model.ActivePostMetadata, Model.ActiveWeblogInfo,null);

            StatusBar.ShowStatusProgress($"Downloading last {Model.NumberOfPostsToRetrieve} posts...");

            var posts = publisher.GetRecentPosts(Model.NumberOfPostsToRetrieve)?.ToList();
            if (posts == null)
                StatusBar.ShowStatusError($"An error occurred trying to retrieve posts: {publisher.ErrorMessage}");

            Dispatcher.Invoke(() =>
            {
                StatusBar.ShowStatusSuccess($"{posts.Count} posts downloaded.");
                Model.PostList = posts;
            });

            return Task.CompletedTask;
        }


        public async Task DownloadMetaWeblogAndWordPressPosts()
        {
            WeblogInfo weblogInfo = Model.ActiveWeblogInfo;

            if (weblogInfo.Name == null)
            {
                StatusBar.ShowStatusError("Please select a Weblog configuration to list posts for.");
                return;
            }

            var client = new MetaWebLogWordpressApiClient(weblogInfo);
            Model.Configuration.LastWeblogAccessed = weblogInfo.Name;

            Model.PostList = new List<Post>();
            StatusBar.ShowStatusProgress($"Downloading last {Model.NumberOfPostsToRetrieve} posts...");

            List<Post> posts = null;
            try
            {
                bool result = await Task.Run(() =>
                {
                    posts = client.GetRecentPosts(Model.NumberOfPostsToRetrieve).ToList();
                    return false;
                });
            }
            catch (XmlRpcException ex)
            {
                string message = ex.Message;
                if (message == "Not Found")
                    message = $"Invalid Blog API Url:\r\n{weblogInfo.ApiUrl}";
                MessageBox.Show($"Unable to download posts:\r\n{message}", mmApp.ApplicationName,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to download posts:\r\n{ex.Message}", mmApp.ApplicationName,
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
                StatusBar.ShowStatusSuccess($"{posts.Count} posts downloaded.");
                Model.PostList = posts;
            });
        }

        private async void ListViewPosts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
             await CreateDownloadedPost();
        }

        private async void ButtonDownloadPost_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var li = button.TryFindParent<ListBoxItem>();
            li.IsSelected = true;

            await CreateDownloadedPost();
        }

        private async void ListViewPosts_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
               await CreateDownloadedPost();
        }

        #endregion


        private async Task CreateDownloadedPost()
        {

            var item = ListViewPosts.SelectedItem as Post;
            if (item == null)
                return;

            WeblogInfo weblogInfo = Model.ActiveWeblogInfo;

           StatusBar.ShowStatusProgress("Downloading Weblog post '" + item.Title + "'");


            string postId = item.PostId?.ToString();
            
            Post post = null;

            if (weblogInfo.Type == WeblogTypes.MetaWeblogApi)
            {
                var wrapper = new MetaWeblogWrapper(weblogInfo.ApiUrl,
                    weblogInfo.Username,
                    mmApp.DecryptString(weblogInfo.Password),
                    weblogInfo.BlogId);

                try
                {
                    post = await Task.Run(() => wrapper.GetPost(postId));
                }
                catch (Exception ex)
                {
                    StatusBar.ShowStatusError("Unable to download post.\r\n\r\n" + ex.Message);
                    return;
                }

                Model.Addin.CreateDownloadedPostOnDisk(post, weblogInfo.Name);
            }
            else if(weblogInfo.Type == WeblogTypes.Wordpress)
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
                    StatusBar.ShowStatus();
                    StatusBar.ShowStatusError("Unable to download post.\r\n\r\n" + ex.Message);
                    return;
                }

                Model.Addin.CreateDownloadedPostOnDisk(post, weblogInfo.Name);
            }
            else if (weblogInfo.Type == WeblogTypes.LocalJekyll)
            {
                var pub = new LocalJekyllPublisher(null, weblogInfo,null);
                post = pub.GetPost(postId);
                if (post == null)
                {
                    StatusBar.ShowStatusError("Unable to import post from Jekyll.");
                    return;
                }

                string outputFile = pub.CreateDownloadedPostOnDisk(post,weblogInfo.Name);

                mmApp.Model.Window.OpenTab(outputFile);
                mmApp.Model.Window.ShowFolderBrowser(folder: Path.GetDirectoryName(outputFile));
            }
           

            Close();
            StatusBar.ShowStatusSuccess("Post has been imported into Markdown Monster Web log Posts.");
        }

        private void ButtonApiUrlInfo_Click(object sender, RoutedEventArgs e)
        {
            ShellUtils.GoUrl("http://markdownmonster.west-wind.com/docs/_4rg0qzg1i.htm");
        }

        private async void ButtonDiscoverEndpoint_Click(object sender, RoutedEventArgs e)
        {
            if (Model.ActiveWeblogInfo == null)
                return;

            var discover = new BlogEndpointDiscovery();

            var url = Model.ActiveWeblogInfo.ApiUrl;
            if (string.IsNullOrEmpty(url))
                return;

            StatusBar.ShowStatusProgress("Checking Endpoint Url...");

            if (await discover.CheckRpcEndpointAsync(url))
            {
                StatusBar.ShowStatusSuccess("The Weblog Endpoint is a valid RPC endpoint.", 10000);
                return;
            }

            StatusBar.ShowStatusProgress("Checking for RSD links...");

            var blogInfo = await discover.DiscoverBlogEndpointAsync(url, Model.ActiveWeblogInfo.BlogId as string,
                Model.ActiveWeblogInfo.Type.ToString());

            if (blogInfo.HasError)
            {
                if (url.IndexOf("medium.com", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    Model.ActiveWeblogInfo.ApiUrl = "https://api.medium.com/v1/";
                    Model.ActiveWeblogInfo.Type = WeblogTypes.Medium;
                    StatusBar.ShowStatusSuccess("Weblog API Endpoint Url found and updated.");
                    return;
                }

                StatusBar.ShowStatusError($"Endpoint Discovery: {blogInfo.ErrorMessage}");
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

            StatusBar.ShowStatusSuccess("Weblog API Endpoint Url found and updated.", 6000);
        }

        private void ComboWeblogType_SelectionChanged(object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
        {

            Model.PropertyChangeForVisibility();
        }

        private void DropDownButton_Click(object sender, RoutedEventArgs e)
        {
            StatusBar.ShowStatusProgress("Getting Blog listing information from service...");
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
                        StatusBar.ShowStatusError("Failed to get blog listing: " + client.ErrorMessage);
                }
                else if (Model.ActiveWeblogInfo.Type == WeblogTypes.MetaWeblogApi ||
                         Model.ActiveWeblogInfo.Type == WeblogTypes.Wordpress)
                {
                    var client = new MetaWebLogWordpressApiClient(Model.ActiveWeblogInfo);
                    blogs = client.GetBlogs();
                    if (blogs == null)
                        StatusBar.ShowStatusError("Failed to get blog listing: " + client.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                StatusBar.ShowStatusError($"Failed to get blogs: {ex.Message}");
                return;
            }

            StatusBar.ShowStatusSuccess("Blogs retrieved.");

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
                item.Click += (s, ea) =>
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


        #region CustomField Handling

        private void ButtonAddCustomField_Click(object sender, RoutedEventArgs e)
        {
            Model.MetadataCustomFields.Add(new CustomField
            {
                Key = "new_key", Value = "value"
            });
            RefreshCustomFields();
        }

        private void ButtonDeleteCustomField_Click(object sender, RoutedEventArgs e)
        {

            var item = ListCustomFields.SelectedItem as CustomField;
            if (item == null)
                return;

            Model.MetadataCustomFields.Remove(item);
            RefreshCustomFields();
        }

        private void GetCustomFieldsFromObservableCollection()
        {
            Model.ActivePostMetadata.CustomFields.Clear();
            foreach (var cf in Model.MetadataCustomFields)
            {
                Model.ActivePostMetadata.CustomFields[cf.Key] = cf;
            }
        }


        private void ButtonCustomFieldHelp_Click(object sender, RoutedEventArgs e)
        {
            ShellUtils.GoUrl(mmApp.GetDocumentionUrl("_4wq1dbsnh"));
        }

        private void RefreshCustomFields()
        {
            Model.ActivePostMetadata.OnPropertyChanged(nameof(WeblogPostMetadata.CustomFields));
            Model.OnPropertyChanged(nameof(WeblogAddinModel.MetadataHasCustomFields));
        }

        #endregion


        #region WebLog Site Validations

        private void ButtonSaveWebLogInfo_Click(object sender, RoutedEventArgs e)
        {
            if (!Model.Configuration.Weblogs.ContainsKey(Model.ActiveWeblogInfo.Id))
            {
                Model.Configuration.Weblogs[Model.ActiveWeblogInfo.Id] = Model.ActiveWeblogInfo;
                Model.Configuration.OnPropertyChanged(nameof(WeblogAddinConfiguration.Weblogs));
                ComboWebLogName.SelectedValue = Model.Configuration.Weblogs[Model.ActiveWeblogInfo.Id];
            }

            WeblogAddinConfiguration.Current.Write();
        }

        private void ActiveWebLogsPageForNew()
        {
            TabControl.SelectedItem = TabWeblogs;
            Dispatcher.InvokeAsync(ButtonNewWeblog.Focus);
        }


        private void ComboWebLogSite_DropDownOpened(object sender, EventArgs e)
        {
            if (ComboWebLogSite.Items.Count == 0)
                ActiveWebLogsPageForNew();
        }

        #endregion

        private async void ButtonValidatePassword_Click(object sender, RoutedEventArgs e)
        {
            WeblogInfo weblogInfo = Model.ActiveWeblogInfo;
            if (string.IsNullOrEmpty(weblogInfo.Username) || string.IsNullOrEmpty(weblogInfo.Password))
            {
                StatusBar.ShowStatusError("Username and/or password are not set.");
                return;
            }
            var client = new MetaWebLogWordpressApiClient(weblogInfo);
         
            Model.PostList = new List<Post>();
            StatusBar.ShowStatusProgress($"Checking for Weblog access...");

            List<Post> posts = null;
            try
            {
                bool result = await Task.Run(() =>
                {
                    posts = client.GetRecentPosts(1).ToList();
                    return true;
                });
                StatusBar.ShowStatusSuccess("Password is valid",5000);
            }
            catch (XmlRpcException ex)
            {
                StatusBar.ShowStatusError("Password is invalid: " + ex.Message);
            }
            catch (Exception ex)
            {
                StatusBar.ShowStatusError("Password is invalid: " + ex.Message);                
            }

        }
    }
}
