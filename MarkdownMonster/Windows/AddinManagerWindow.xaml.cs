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

        public AddinManagerWindow()
        {
            InitializeComponent();
            mmApp.SetThemeWindowOverride(this);

            Loaded += AddinManagerWindow_Loaded;
            DataContext = this;            
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
            AddinList = new ObservableCollection<AddinItem>(addinList);

            if (AddinList.Count > 0)
                ActiveAddin = AddinList[0];
        }

        private void ListViewAddins_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void ListViewAddins_KeyDown(object sender, KeyEventArgs e)
        {

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

            ShowStatus("Downloading and installing " + addin.name + " Addin...");


            var url = addin.gitVersionUrl.Replace("version.json","addin.zip");
            var result = AddinManager.Current.DownloadAndInstallAddin(url, mmApp.Configuration.AddinsFolder, addin);
            if (result.IsError)
                ShowStatus(addin.name + "  installation  failed.", 6000);
            else
            {
                addin.isInstalled = true;
                addin.isEnabled = true;
                addin.installedVersion = addin.version;

                if (!result.ExistingAddin)
                {
                    if (!AddinManager.Current.InstallAddin(addin.id))
                        ShowStatus(addin.name + "  installation  failed.", 6000);
                    else
                    {
                        var installedAddin = AddinManager.Current.AddIns.FirstOrDefault(ai => ai.Id == addin.id);
                        if (installedAddin != null)
                        {                            
                            installedAddin.OnApplicationStart();

                            // trigger parser addins to refresh MarkdownParsers if they are provided
                            if (installedAddin.GetMarkdownParser() != null)
                                mmApp.Model.OnPropertyChanged(nameof(AppModel.MarkdownParserColumnWidth));                            

                            AddinManager.Current.InitializeAddinsUi(Owner as MainWindow,
                                new List<MarkdownMonsterAddin>() {installedAddin});
                            ShowStatus(addin.name + "  addin installed.", 6000);
                        }
                    }
                }
                else
                {
                    string msg = addin.name +
                                 " addin has been installed.\r\n\r\nYou need to restart Markdown Monster to finalize the addin installation.";
                    ShowStatus(msg, 6000);
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

                
                //AddinManager.Current.LoadAddins();
            }
        }

        private void ButtonUnInstall_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var addin = button.DataContext as AddinItem;
            if (addin == null)
                return;
                        
            if ( AddinManager.Current.UninstallAddin(addin.id))
            {
                ShowStatus(addin.name + 
                    " marked for deletion. Please restart Markdown Monster to finalize un-install.", 
                    6000);
                addin.isInstalled = false;
            }
            else
                ShowStatus(addin.name + " failed to uninstall.",6000);
        }

        private Timer timer;

        public void ShowStatus(string message = null, int milliSeconds = 0)
        {
            if (message == null)
            {
                message = "Ready";                
            }

            Brush oldBgColor = StatusText.Background;
            StatusBar.Background = Brushes.SteelBlue;

            StatusText.Text = message;

            if (milliSeconds > 0)
            {
                timer = new Timer((object win) =>
                {
                    var window = win as AddinManagerWindow;
                    if (window == null)
                        return;
                    window.Dispatcher.Invoke(() =>
                    {
                        window.ShowStatus(null, 0);
                        StatusBar.Background = oldBgColor;
                    });
                }, this, milliSeconds, Timeout.Infinite);
            }
            WindowUtilities.DoEvents();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            if (ActiveAddin == null)
                return;

            ShellUtils.GoUrl(ActiveAddin.gitUrl);
        }
    }
}
