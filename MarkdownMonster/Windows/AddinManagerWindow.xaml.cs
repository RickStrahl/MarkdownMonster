using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MarkdownMonster.AddIns;
using MarkdownMonster.Annotations;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for AddinManagerWindow.xaml
    /// </summary>
    public partial class AddinManagerWindow : MetroWindow, INotifyPropertyChanged
    {
        
        public ObservableCollection<AddinItem> AddinList
        {
            get { return _addinList; }
            set
            {
                if (Equals(value, _addinList)) return;
                _addinList = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<AddinItem> _addinList;

        private Brush oldBgColor;

        public StatusBarHelper StatusBar { get; set; }

        public AddinManagerWindow()
        {
            InitializeComponent();
            mmApp.SetThemeWindowOverride(this);

            Loaded += AddinManagerWindow_Loaded;
            DataContext = this;

            oldBgColor = StatusText.Background;

            StatusBar = new StatusBarHelper(StatusText, StatusIcon);
        }
        

        public AddinItem ActiveAddin
        {
            get { return _activeAddin; }
            set
            {
                if (Equals(value, _activeAddin)) return;
                _activeAddin = value;
                OnPropertyChanged();
            }
        }
        private AddinItem _activeAddin;

        private async void AddinManagerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // fill and sort as data is filled out
            var addinList = await AddinManager.Current.GetAddinListAsync();
            if (addinList == null)
            {
                AddinList = new ObservableCollection<AddinItem>();
                StatusBar.ShowStatusError("Unable to load addin list.");
                return;
            }
            AddinList = new ObservableCollection<AddinItem>(addinList);

            if (AddinList.Count > 0)
                ActiveAddin = AddinList[0];
        }

        

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var addin = button.DataContext as AddinItem;
            if (addin == null)
                return;

            StatusBar.ShowStatusProgress($"Downloading and installing {addin.name} Addin...");
            
            var url = addin.gitVersionUrl.Replace("version.json","addin.zip");
            var result = AddinManager.Current.DownloadAndInstallAddin(url, mmApp.Configuration.AddinsFolder, addin);
            if (result.IsError)
                StatusBar.ShowStatusError(AddinManager.Current.ErrorMessage);
            else if (result.NeedsRestart)
            {
                string msg = addin.name +
                             " addin has been installed.\r\n\r\nYou need to restart Markdown Monster to finalize the addin installation.";
                StatusBar.ShowStatusSuccess(msg);
                if (MessageBox.Show(msg + "\r\n\r\nDo you want to restart Markdown Monster?", "Addin Installed",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Owner.Close();
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = Assembly.GetEntryAssembly().Location,
                    });
                }
            }
            else
                StatusBar.ShowStatusSuccess($"Addin {addin.name} has been installed");
        }

        private void ButtonUnInstall_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var addin = button.DataContext as AddinItem;
            if (addin == null)
                return;
                        
            if ( AddinManager.Current.UninstallAddin(addin.id))
            {
                StatusBar.ShowStatusSuccess($"{addin.name} marked for deletion. Please restart Markdown Monster to finalize un-install.");
                addin.isInstalled = false;
            }
            else
                StatusBar.ShowStatusError($"{addin.name} failed to uninstall.");
        }
        

        private void ButtonMoreInfo_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveAddin == null)
                return;

            ShellUtils.GoUrl(ActiveAddin.gitUrl);
        }
    }
}
