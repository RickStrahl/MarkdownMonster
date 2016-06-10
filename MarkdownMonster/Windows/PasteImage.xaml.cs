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
    public partial class PasteImage : MetroWindow, INotifyPropertyChanged   
    {
        private string _image;
        private string _imageText;

        public string Image
        {
            get { return _image; }
            set
            {
                if (value == _image) return;
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

        public string ImageText
        {
            get { return _imageText; }
            set
            {
                if (value == _imageText) return;
                _imageText = value;
                OnPropertyChanged(nameof(ImageText));
            }
        }


        public string MarkdownFile { get; set; }

        
        public PasteImage()
        {
            InitializeComponent();

            DataContext = this;
            mmApp.SetThemeWindowOverride(this);

            Loaded += PasteHref_Loaded;
        }

        private void PasteHref_Loaded(object sender, RoutedEventArgs e)
        {
            this.TextImage.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == ButtonCancel)
                DialogResult = false;
            else
                DialogResult = true;

            Close();
        }

        private void SelectLocalImageFile_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog
            {
                DefaultExt = ".png",
                Filter = "Image files (*.png;*.jpg;*.gif;)|*.png;*.jpg;*.jpeg;*.gif|All Files (*.*)|*.*",
                CheckFileExists = true,
                RestoreDirectory = true,
                Multiselect = false,
                Title = "Embed Image"
            };

            if (!string.IsNullOrEmpty(MarkdownFile))
                fd.InitialDirectory = System.IO.Path.GetDirectoryName(MarkdownFile);
            else
            {
                if (!string.IsNullOrEmpty(mmApp.Configuration.LastImageFolder))
                    fd.InitialDirectory = mmApp.Configuration.LastImageFolder;
                else
                    fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }

            var res = fd.ShowDialog();
            if (res == null || !res.Value)
                return;

            Image = fd.FileName;
            
            // Normalize the path relative to the Markdown file
            if (!string.IsNullOrEmpty(MarkdownFile))
            {
                string mdPath = System.IO.Path.GetDirectoryName(this.MarkdownFile);
                string relPath = FileUtils.GetRelativePath(fd.FileName, mdPath);

                
                if (!relPath.StartsWith("..\\"))
                    Image = relPath;
                else
                {
                    // not relative 
                    var mbres = MessageBox.Show(
                        "The image you are linking, is not in a relative path.\r\n" +
                        "Do you want to copy it to a local path?",
                        "Non-relative Image",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (mbres.HasFlag(MessageBoxResult.Yes))
                    {
                        string newImageFileName = System.IO.Path.Combine(mdPath, System.IO.Path.GetFileName(fd.FileName));
                        var sd = new SaveFileDialog
                        {
                            Filter = "Image files (*.png;*.jpg;*.gif;)|*.png;*.jpg;*.jpeg;*.gif|All Files (*.*)|*.*",
                            FilterIndex = 1,
                            FileName = newImageFileName,
                            InitialDirectory = mdPath,
                            CheckFileExists = false,
                            OverwritePrompt = true,
                            CheckPathExists = true,
                            RestoreDirectory = true
                        };
                        var result = sd.ShowDialog();
                        if (result != null && result.Value)
                        {
                            System.IO.File.Copy(fd.FileName, sd.FileName);
                            Image = FileUtils.GetRelativePath(sd.FileName, mdPath);
                        }
                    }
                }
            }
            mmApp.Configuration.LastImageFolder = System.IO.Path.GetDirectoryName(fd.FileName);
            TextImageText.Focus();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
