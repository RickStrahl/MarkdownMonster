using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MarkdownMonster.Windows;
using MarkdownMonster.Windows.PreviewBrowser;
using Microsoft.Web.WebView2.Core;
using Westwind.Utilities;

namespace WebViewPreviewerAddin
{
    /// <summary>
    /// WebViewPreviewerAddin
    ///
    /// More info on Addin Development:
    /// https://markdownmonster.west-wind.com/docs/_4ne0s0qoi.htm
    /// </summary>
    public class WebViewPreviewerAddin : MarkdownMonster.AddIns.MarkdownMonsterAddin
    {
        private bool IsActive
        {
            get => WebViewPreviewerAddinConfiguration.Current.IsActive;
            set
            {
                WebViewPreviewerAddinConfiguration.Current.IsActive = value;
                if (MenuItem?.MenuItemButton != null)
                {
                    var img = MenuItem?.MenuItemButton.Content as Image;

                    if (IsActive)
                    {
                        MenuItem.MenuItemButton.Opacity = 1.0;
                        img.ToolTip =
                            "Chromium based Previewer is active. Click to switch to default Internet Explorer Previewer.";
                    }

                    else
                    {
                        MenuItem.MenuItemButton.Opacity = 0.50;
                        img.ToolTip =
                            "Default Internet Explorer Previewer is active. Click to switch to Chromium based Previewer.";
                    }
                }
            }
        }
        AddInMenuItem MenuItem = null;

        public WebViewPreviewerAddinConfiguration Configuration { get =>  WebViewPreviewerAddinConfiguration.Current ; }   
        

        /// <summary>
        /// Fired when the Addin is initially loaded. This is very early in
        /// the lifecycle and should only be used to create the addin name
        /// and UI options.
        /// </summary>
        /// <remarks>
        /// You do not have access to the Model or UI from this overload.
        /// </remarks>  
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();


            // Id - should match output folder name. REMOVE 'Addin' from the Id
            Id = "WebViewPreviewer";

            // a descriptive name - shows up on labels and tooltips for components
            Name = "Chromium WebView Previewer";


            // by passing in the add in you automatically
            // hook up OnExecute/OnExecuteConfiguration/OnCanExecute
            var menuItem = new AddInMenuItem(this)
            {
                Caption = Name,

                // if an icon is specified it shows on the toolbar
                // if not the add-in only shows in the add-ins menu
                FontawesomeIcon = FontAwesomeIcon.Chrome,
            };
            menuItem.IconImageSource = new ImageSourceConverter() 
                .ConvertFromString("pack://application:,,,/WebViewPreviewerAddin;component/icon_32.png") as ImageSource;

            MenuItem = menuItem;

            //menuItem.Execute = null;

            // if you don't want to display config or main menu item clear handler
            menuItem.ExecuteConfiguration = null;

            // Must add the menu to the collection to display menu and toolbar items            
            MenuItems.Add(menuItem);
        }

        /// <summary>
        /// Fired after the model has been loaded. If you need model access during loading
        /// this is the place to hook up your code.
        /// </summary>
        /// <param name="model">The Markdown Monster Application model</param>
        public override void OnModelLoaded(AppModel model)
        {
            
        }


        /// <summary>
        /// Fired after the Markdown Monster UI becomes available
        /// for manipulation.
        ///
        /// If you add UI elements as part of your Addin, this is the
        /// place where you can hook them up.
        /// </summary>
        public override void OnWindowLoaded()
        {
            // force UI to update
            IsActive = Configuration.IsActive;
        }


        /// <summary>
        /// Fired when you click the addin button in the toolbar.
        /// </summary>
        /// <param name="sender"></param>
        public override void OnExecute(object sender)
        {
            if (Model.Window == null)
                return;

            IsActive = !IsActive;
            if (IsActive &&
                 !IsWebViewVersionInstalled(true))
            {
                IsActive = false;
                return;
            }
                
            Model.Window.LoadPreviewBrowser();

            if (IsActive)
                Model.Window.ShowStatusSuccess("Switched to Chromium based Preview Browser.");
            else
                Model.Window.ShowStatusSuccess("Switched to Internet Explorer based Preview Browser.");
        }
        

        /// <summary>
        /// Fired when you click on the configuration button in the addin
        /// </summary>
        /// <param name="sender">The Execute toolbar button for this addin</param>
        public override void OnExecuteConfiguration(object sender)
        {
            Model.Window.OpenFile(Path.Combine(Model.Configuration.CommonFolder,Configuration.ConfigurationFilename));
        }


        /// <summary>
        /// Re-read the config file after making changes and reloading if active status is changed
        /// </summary>
        /// <param name="doc"></param>
        public override void OnAfterSaveDocument(MarkdownDocument doc)
        {
            if (StringUtils.Contains(doc.Filename, "WebViewPreviewerAddin.json", StringComparison.OrdinalIgnoreCase))
            {
                var oldActive = Configuration.IsActive;
                Configuration.Read();

                if (oldActive != Configuration.IsActive)
                    Model.Window.LoadPreviewBrowser();
            }
        }


        /// <summary>
        /// Determines on whether the addin can be executed
        /// </summary>
        /// <param name="sender">The Execute toolbar button for this addin</param>
        /// <returns></returns>
        public override bool OnCanExecute(object sender)
        {
            return true;
        }

        public override IPreviewBrowser GetPreviewBrowserUserControl()
        {
            if (!IsActive || !IsWebViewVersionInstalled())
                return null;  // use the default
            
            return new WebViewPreviewControl();
        }

        public override void OnApplicationShutdown()
        {
            Configuration.Write();
        }

        private bool IsWebViewVersionInstalled(bool showDownloadUi = false)
        {

            string versionNo = null;
            Version asmVersion = null;
            Version ver = null;

            try
            {
                versionNo = CoreWebView2Environment.GetAvailableBrowserVersionString();

                // strip off 'canary' or 'stable' verison
                versionNo = StringUtils.ExtractString(versionNo, "", " ", allowMissingEndDelimiter: true)?.Trim();
                ver = new Version(versionNo);

                asmVersion = typeof(CoreWebView2Environment).Assembly.GetName().Version;

                if (ver.Build >= asmVersion.Build)
                    return true;
            }
            catch {}

            IsActive = false;

            if (!showDownloadUi)
                return false;


            var form = new BrowserMessageBox() {
                Owner = mmApp.Model.Window,
                Width = 600,
                Height= 440,
                Title="WebView Runtime Installation",
            };

            form.Dispatcher.Invoke(() => form.Icon = new ImageSourceConverter()
                .ConvertFromString("pack://application:,,,/WebViewPreviewerAddin;component/icon_32.png") as ImageSource);

            var markdown = $@"
### WebView Runtime not installed or out of Date
The Microsoft Edge WebView Runtime is
{ ( !string.IsNullOrEmpty(versionNo) ?
                "out of date\n\nYour Build: " + ver.Build +
                "   -   Required Build: " + asmVersion.Build :
                "not installed")  }.

In order to use the Chromium preview you need to install this runtime by downloading from the [Microsoft Download Site](https://developer.microsoft.com/en-us/microsoft-edge/webview2/).

**Do you want to download and install the Edge WebView Runtime?**

*<small>clicking **Yes** sends you to the Microsoft download site.  
choose the **Evergreen Bootstrapper** download.</small>*";

                form.ClearButtons();
                var yesButton = form.AddButton("Yes", FontAwesomeIcon.CheckCircle, Brushes.Green);
                yesButton.Width = 90;
                var noButton = form.AddButton("No", FontAwesomeIcon.TimesCircle, Brushes.Firebrick);
                noButton.Width = 90;
                form.ShowMarkdown(markdown);
            

                form.ShowDialog();
                if (form.ButtonResult == yesButton)
                {
                    mmFileUtils.OpenBrowser("https://developer.microsoft.com/en-us/microsoft-edge/webview2/");
                }

                return false;
            
        }
    }
}
