using System;
using System.Diagnostics;
using System.Reflection;
using MahApps.Metro.Controls;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : MetroWindow
    {
        public About()
        {           
            InitializeComponent();
            
            mmApp.SetThemeWindowOverride(this);

            VersionLabel.Content = "Version " +  mmApp.GetVersionForDisplay();
            VersionDateLabel.Content = mmApp.GetVersionDate();
            OsLabel.Content = (Environment.Is64BitProcess ? "64 bit" : "32 bit") + " • " +
                             ".NET " + MarkdownMonster.Utilities.WindowsUtils.GetDotnetVersion();
            if(App.IsPortableMode)
                PortableMode.Content = "Portable mode";

            if (UnlockKey.IsRegistered())
            {
                PanelFreeNotice.Visibility = System.Windows.Visibility.Hidden;
                LabelRegistered.Visibility = System.Windows.Visibility.Visible;
            }
            

            if (mmApp.Configuration.ApplicationUpdates.AccessCount > 20)
                LabelUsingFreeVersion.Text =
                    $"You've used the free version {mmApp.Configuration.ApplicationUpdates.AccessCount:n0} times.";

        }

        private void WestWindIcon_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShellUtils.GoUrl("http://weblog.west-wind.com");
        }

        private void Register_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ShellUtils.GoUrl(mmApp.Urls.RegistrationUrl);
        }
    }
}
