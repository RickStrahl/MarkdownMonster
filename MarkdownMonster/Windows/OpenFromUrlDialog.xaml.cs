using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for PasteHref.xaml
    /// </summary>
    public partial class OpenFromUrlDialog : MetroWindow, INotifyPropertyChanged
    {
   

        public string Url
        {
            get { return _url; }
            set
            {
                if (value == _url) return;
                _url = value;
                OnPropertyChanged(nameof(Url));
            }
        }
        private string _url;


        public bool FixupImageLinks
        {
            get => _fixupImageLinks;
            set
            {
                if (value == _fixupImageLinks) return;
                _fixupImageLinks = value;
                OnPropertyChanged(nameof(FixupImageLinks));
            }
        }
        private bool _fixupImageLinks;

        public OpenFromUrlDialog()
        {

            InitializeComponent();

            DataContext = this;
            mmApp.SetThemeWindowOverride(this);

            Loaded += OpenFromUrl_Loaded;
            Activated += OpenFromUrl_Activated;            
        }

        private void OpenFromUrl_Activated(object sender, EventArgs e)
        {
            string clip = Clipboard.GetText(TextDataFormat.Text);
            if (string.IsNullOrEmpty(Url) &&
                clip.StartsWith("http://") || clip.StartsWith("https://"))
                Url = clip;
        }

        private void OpenFromUrl_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Url))
            {
                string clipText = Clipboard.GetText();
                if (clipText != null && clipText.Contains("://") && !clipText.Contains("\n"))
                    Url = clipText;
            
            }

            TextUrl.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == ButtonCancel)
                DialogResult = false;
            else                            
                DialogResult = true;                
            
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void BrowseForLink_Click(object sender, RoutedEventArgs e)
        {

            mmApp.Model.Window.ShowStatus("Browse for URL then return to the URL form...", 10000, FontAwesome.WPF.FontAwesomeIcon.InfoCircle, Colors.DarkGoldenrod);
            if (!string.IsNullOrEmpty(Url))
                ShellUtils.GoUrl(Url);
            else
                ShellUtils.GoUrl("https://bing.com");
        }
    }
}
