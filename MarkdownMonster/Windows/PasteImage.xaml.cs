using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using MarkdownMonster;
using MarkdownMonster.AddIns;
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

            Loaded += PasteImage_Loaded;
            SizeChanged += PasteImage_SizeChanged;
        }

        private void PasteImage_Loaded(object sender, RoutedEventArgs e)
        {
            TextImage.Focus();
            if (Clipboard.ContainsText())
            {
                string clip = Clipboard.GetText().ToLower();
                if ((clip.StartsWith("http://") || clip.StartsWith("https://")) &&
                    (clip.Contains(".png") || clip.Contains("jpg")))
                {
                    TextImage.Text = clip;
                    SetImagePreview(clip);
                }
            }
        }

        private void PasteImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var image = ImagePreview.Source as BitmapSource;
            if (image == null)
                return;

            if (image.Width < Width - 20 && image.Height < PageGrid.RowDefinitions[1].ActualHeight)
                ImagePreview.Stretch = Stretch.None;
            else
                ImagePreview.Stretch = Stretch.Uniform;
        }

        private void TextImage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextImage.Text))
            {
                ImagePreview.Source = null;
                return;
            }
                
            string href = TextImage.Text.ToLower();
            if (href.StartsWith("http://") || href.StartsWith("https://"))
            {
                SetImagePreview(href);                
            }

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
            if (!string.IsNullOrEmpty(MarkdownFile) && MarkdownFile != "untitled")
            {
                Image = AddinManager.Current.RaiseOnSaveImage(fd.FileName);
                if (!string.IsNullOrEmpty(Image))
                {
                    TextImageText.Focus();
                    return;
                }

                string mdPath = Path.GetDirectoryName(MarkdownFile);
                string relPath = fd.FileName;
                try
                {
                    relPath = FileUtils.GetRelativePath(fd.FileName, mdPath);
                }
                catch (Exception ex)
                {
                    mmApp.Log($"Failed to get relative path.\r\nFile: {fd.FileName}, Path: {mdPath}", ex);
                }


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

                    if (mbres.Equals(MessageBoxResult.Yes))
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
                            try
                            {
                                File.Copy(fd.FileName, sd.FileName, true);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Couldn't copy file to new location: \r\n" + ex.Message,
                                    mmApp.ApplicationName);
                                return;
                            }
                            try
                            {
                                relPath = FileUtils.GetRelativePath(sd.FileName, mdPath);
                            }
                            catch (Exception ex)
                            {
                                mmApp.Log($"Failed to get relative path.\r\nFile: {sd.FileName}, Path: {mdPath}", ex);
                            }
                            if (!relPath.StartsWith("..\\"))
                                Image = relPath;
                        }
                    }
                }
            }

            if (Image.Contains(":\\"))
                Image = "file:///" + Image;

            SetImagePreview("file:///" + fd.FileName);            

            mmApp.Configuration.LastImageFolder = Path.GetDirectoryName(fd.FileName);
            TextImageText.Focus();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private void SetImagePreview(string url)
        {
            try
            {
                ImagePreview.Source = BitmapFrame.Create(new Uri(url));
                if (Height < 400)
                {
                    Top -= 300;
                    Left -= 100;                    
                    Width = 800;
                    Height = 800;
                }

                WindowUtilities.DoEvents();
                PasteImage_SizeChanged(this, null);
            }
            catch
            {
            }
        }
    }
}
