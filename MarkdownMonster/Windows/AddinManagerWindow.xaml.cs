using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

            Loaded += AddinManagerWindow_Loaded;
            DataContext = this;

            
        }

        private async void AddinManagerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var listOfAddins = await AddinManager.Current.GetAddinListAsync();
            AddinList = new ObservableCollection<AddinItem>(listOfAddins);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var addin = button.DataContext as AddinItem;
            if (addin == null)
                return;

            ShowStatus("Downloading and installing " + addin.name + " Addin...");

            var url = addin.gitVersionUrl.Replace("Version.json","addin.zip");            
            if (!AddinManager.Current.DownloadAndInstallAddin(url, ".\\Addins\\Install\\" + addin.id))
                ShowStatus(addin.name + "  installation  failed.",6000);
            else
                ShowStatus(addin.name + " installed. Restart required...",6000);
        }

        private Timer timer;

        public void ShowStatus(string message = null, int milliSeconds = 0)
        {
            if (message == null)
            {
                message = "Ready";                
            }

            StatusText.Text = message;

            if (milliSeconds > 0)
            {
                timer = new Timer((object win) =>
                {
                    var window = win as AddinManagerWindow;
                    if (window == null)
                        return;
                    window.Dispatcher.Invoke(() => { window.ShowStatus(null, 0); });
                }, this, milliSeconds, Timeout.Infinite);
            }
            WindowUtilities.DoEvents();
        }
    }
}
