using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Westwind.Utilities;

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

        public string MarkdownFile { get; set; }

        public AppModel AppModel { get; set; } = mmApp.Model;

        public PasteHref()
        {
            
            InitializeComponent();

            DataContext = this;
            mmApp.SetThemeWindowOverride(this);

            Loaded += PasteHref_Loaded;
            Activated += PasteHref_Activated;

            IsExternal = mmApp.Configuration.LastLinkExternal;

        }

        private void PasteHref_Activated(object sender, EventArgs e)
        {
            string clip = Clipboard.GetText(TextDataFormat.Text);
            if (string.IsNullOrEmpty(Link) &&
                clip.StartsWith("http://") || clip.StartsWith("https://") || clip.StartsWith("mail:") ||
                clip.StartsWith("ftp://"))
                Link = clip;
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
                    if (clipText != null && clipText.Contains("://") && !clipText.Contains("\n"))
                        Link = clipText;
                }
            }

            if (string.IsNullOrEmpty(LinkText) && !string.IsNullOrEmpty(Link))
                LinkText = Link;

            if (string.IsNullOrEmpty(LinkText))
                this.TextLinkText.Focus();
            else
                this.TextLink.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == ButtonCancel)
                DialogResult = false;
            else
            {
                mmApp.Configuration.LastLinkExternal = IsExternal;
                DialogResult = true;                
            }

            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SelectLocalLinkFile_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog
            {
                DefaultExt = ".html",
                Filter = "Linkable Files (*.html,*.htm,*.md,*.pdf;*.zip)|*.html;*.htm;*.md;*.pdf;*.zip|All Files (*.*)|*.*",
                CheckFileExists = true,
                RestoreDirectory = true,
                Multiselect = false,
                Title = "Embed a local relative link"
            };

            if (!string.IsNullOrEmpty(MarkdownFile))
                fd.InitialDirectory = System.IO.Path.GetDirectoryName(MarkdownFile);
            else
                fd.InitialDirectory = mmApp.Configuration.LastFolder;

            var res = fd.ShowDialog();
            if (res == null || !res.Value)
                return;

            Link = fd.FileName;

            // Normalize the path relative to the Markdown file
            if (!string.IsNullOrEmpty(MarkdownFile))
            {
                string mdPath = System.IO.Path.GetDirectoryName(MarkdownFile);

                string relPath = fd.FileName;
                try
                {
                    relPath = FileUtils.GetRelativePath(fd.FileName, mdPath);
                }
                catch (Exception ex)
                {
                    mmApp.Log($"Failed to get relative path.\r\nFile: {fd.FileName}, Path: {mdPath}", ex);
                }
                
                // not relative
                if (!relPath.StartsWith("..\\"))
                    Link = relPath.Replace("\\","/");

                // is it a physical path?
                if (Link.Contains(":\\"))
                    Link = "file:///" + Link;

            }

            Link = StringUtils.UrlEncode(Link);

            mmApp.Configuration.LastFolder = System.IO.Path.GetDirectoryName(fd.FileName);
            TextLink.Focus();
        }
    }
}
