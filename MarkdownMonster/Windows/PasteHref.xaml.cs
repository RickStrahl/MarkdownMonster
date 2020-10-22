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

        public bool IsLinkReference { get; set;  }

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
            IsLinkReference = mmApp.Configuration.LastUseReferenceLinks;
        }

     
        private void PasteHref_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Link))
            {
                if (IsLink(LinkText))
                    Link = LinkText;
                else
                {
                    string clipText = ClipboardHelper.GetText();
                    if (IsLink(clipText))
                        Link = clipText;
                }
            }
            if (string.IsNullOrEmpty(LinkText) && !string.IsNullOrEmpty(Link))
                LinkText = Link;

            if (string.IsNullOrEmpty(LinkText))
                TextLinkText.Focus();
            else
                TextLink.Focus();
        }

        private void PasteHref_Activated(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Link))
            {
                string clip = ClipboardHelper.GetText();
                if (IsLink(clip))
                {
                    Link = clip.Replace(" ", "%20");
                    TextLink.SelectAll();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //WindowUtilities.FixFocus(this, CheckExternalLink);

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
                Filter = "Linkable Files (*.html,*.htm,*.md,*.pdf;*.zip;.7z)|*.html;*.htm;*.md;*.pdf;*.zip;*.7z;|All Files (*.*)|*.*",
                CheckFileExists = true,
                RestoreDirectory = true,
                Multiselect = false,
                Title = "Embed a local relative link"
            };

            fd.InitialDirectory = mmApp.Configuration.LastLinkFolder;

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

            Link = Link.Replace(" ", "%20");

            mmApp.Configuration.LastLinkFolder = System.IO.Path.GetDirectoryName(fd.FileName);
            mmApp.Configuration.LastLinkExternal = IsExternal;
            mmApp.Configuration.LastUseReferenceLinks = IsLinkReference;

            TextLink.Focus();
        }

        /// <summary>
        /// External link and Link Reference are mutually exclusive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Checkbox_Handler(object sender, RoutedEventArgs e)
        {
            if (sender == CheckExternalLink)
            {
                if (CheckExternalLink.IsChecked.Value)
                    CheckLinkReference.IsChecked = false;
            }
            if (sender == CheckLinkReference)
            {
                if (CheckLinkReference.IsChecked.Value)
                {
                    CheckExternalLink.IsChecked = false;
                }
            }

        }


        private bool IsLink(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return text.StartsWith("http://") ||
                   text.StartsWith("https://") ||
                   text.StartsWith("mail:") ||
                   text.StartsWith("ftp://") ||
                   IsLinkHash(text);
        }

        private bool IsLinkHash(string text)
        {
            if (text == null || text.Length < 2 )
                return false;
            
            return text[0] == '#' && !char.IsWhiteSpace(text[1]);
        }
    }
}
