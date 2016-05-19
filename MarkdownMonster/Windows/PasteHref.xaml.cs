using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
using MarkdownMonster.Annotations;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for PasteHref.xaml
    /// </summary>
    public partial class PasteHref : MetroWindow, INotifyPropertyChanged
    {
        private string _link;
        private string _linkText;
        private bool _isExternal;
        private string _htmlResult;

        public string Link
        {
            get { return _link; }
            set
            {
                if (value == _link) return;
                _link = value;
                OnPropertyChanged(nameof(Link));
            }
        }

        public string LinkText
        {
            get { return _linkText; }
            set
            {
                if (value == _linkText) return;
                _linkText = value;
                OnPropertyChanged(nameof(LinkText));
            }
        }

        public bool IsExternal
        {
            get { return _isExternal; }
            set
            {
                if (value == _isExternal) return;
                _isExternal = value;
                OnPropertyChanged(nameof(IsExternal));
            }
        }

        public string HtmlResult
        {
            get { return _htmlResult; }
            set
            {
                if (value == _htmlResult) return;
                _htmlResult = value;
                OnPropertyChanged(nameof(HtmlResult));
            }
        }


        public PasteHref()
        {
            
            InitializeComponent();

            DataContext = this;
            mmApp.SetThemeWindowOverride(this);

            Loaded += PasteHref_Loaded;
            
        }

        private void PasteHref_Loaded(object sender, RoutedEventArgs e)
        {
                        if (string.IsNullOrEmpty(Link))
            {
                if (LinkText != null && LinkText.Contains("://") && !LinkText.Contains("\n"))
                    Link = LinkText;
                else
                {
                    string clipText = Clipboard.GetText();
                    if (clipText != null && clipText.Contains("://") && !LinkText.Contains("\n"))
                        Link = clipText;
                }
            }

            if (string.IsNullOrEmpty(LinkText) && !string.IsNullOrEmpty(Link))
                LinkText = Link;


            this.TextLink.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == ButtonCancel)
                DialogResult = false;
            else
            {
                DialogResult = true;                
            }

            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
