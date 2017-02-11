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



        public CommandBase PasteCommand
        {
            get { return _pasteCommand; }
            set
            {
                _pasteCommand = value;
                OnPropertyChanged(nameof(PasteCommand));
            }
        }

        private CommandBase _pasteCommand;

        public bool IsFileImage
        {
            get { return !_isMemoryImage; }
        }

        public bool IsMemoryImage
        {
            get { return _isMemoryImage; }
            set
            {
                if (value == _isMemoryImage) return;
                _isMemoryImage = value;
                OnPropertyChanged(nameof(IsMemoryImage));
                OnPropertyChanged(nameof(IsFileImage));
            }
        }

        private bool _isMemoryImage;

        MarkdownDocumentEditor Editor { get; set; }
        MarkdownDocument Document { get; set; }


        public PasteImage()
        {
            InitializeComponent();

            DataContext = this;
            mmApp.SetThemeWindowOverride(this);

            Loaded += PasteImage_Loaded;
            SizeChanged += PasteImage_SizeChanged;
            Activated += PasteImage_Activated;
        }

        private void PasteImage_Activated(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Image) && Clipboard.ContainsImage())            
                PasteImageFromClipboard();            
        }

        private void PasteImage_Loaded(object sender, RoutedEventArgs e)
        {
            var window = this.Owner as MainWindow;
            Editor = window.Model.ActiveEditor;
            Document = window.Model.ActiveDocument;

            PasteCommand = new CommandBase((s, args) =>
            {
                MessageBox.Show("PasteCommand");
            });

            TextImage.Focus();
            if (Clipboard.ContainsImage())
            {
                Button_PasteImage(null, null);
            }
            else if (Clipboard.ContainsText())
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
                var imgUrl = AddinManager.Current.RaiseOnSaveImage(fd.FileName);
                if (!string.IsNullOrEmpty(imgUrl))
                {
                    Image = imgUrl;
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
                            Image = relPath;
                        }
                    }
                    else
                        Image = relPath;
                }
            }

            if (Image.Contains(":\\"))
                Image = "file:///" + Image;
            else
                Image = Image.Replace("\\", "/");

            SetImagePreview("file:///" + fd.FileName);

            IsMemoryImage = false;

            mmApp.Configuration.LastImageFolder = Path.GetDirectoryName(fd.FileName);
            TextImageText.Focus();
            
        }

        private void TextImage_LostFocus(object sender, RoutedEventArgs e)
        {
            
            if (IsMemoryImage && string.IsNullOrEmpty(TextImage.Text))
            {
                ImagePreview.Source = null;                
                return;
            }

            IsMemoryImage = false;

            string href = TextImage.Text.ToLower();
            if (href.StartsWith("http://") || href.StartsWith("https://"))
            {
                SetImagePreview(href);
            }            
        }

        #region Main Buttons

        private void Button_SaveImage(object sender, RoutedEventArgs e)
        {
            string imagePath = null;
            using (var bitMap = WindowUtilities.BitmapSourceToBitmap(ImagePreview.Source as BitmapSource))
            {
                imagePath = AddinManager.Current.RaiseOnSaveImage(bitMap);
            }

            if (!string.IsNullOrEmpty(imagePath))
            {
                TextImage.Text = imagePath;
                IsMemoryImage = false;
                return;
            }


            string initialFolder = null;
            if (!string.IsNullOrEmpty(Document.Filename) && Document.Filename != "untitled")
                initialFolder = Path.GetDirectoryName(Document.Filename);

            var sd = new SaveFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.gif;)|*.png;*.jpg;*.jpeg;*.gif|All Files (*.*)|*.*",
                FilterIndex = 1,
                Title = "Save Image from Clipboard as",
                InitialDirectory = initialFolder,
                CheckFileExists = false,
                OverwritePrompt = true,
                CheckPathExists = true,
                RestoreDirectory = true
            };
            var result = sd.ShowDialog();
            if (result != null && result.Value)
            {
                imagePath = sd.FileName;

                try
                {
                    var ext = Path.GetExtension(imagePath)?.ToLower();

                    using (var fileStream = new FileStream(imagePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = null;
                        if (ext == ".png")
                            encoder = new PngBitmapEncoder();
                        else if (ext == ".jpg")
                            encoder = new JpegBitmapEncoder();
                        else if (ext == ".gif")
                            encoder = new GifBitmapEncoder();

                        encoder.Frames.Add(BitmapFrame.Create(ImagePreview.Source as BitmapSource));
                        encoder.Save(fileStream);

                        if (ext == ".png")
                            mmFileUtils.OptimizePngImage(sd.FileName, 5); // async
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Couldn't copy file to new location: \r\n" + ex.Message, mmApp.ApplicationName);
                    return;
                }

                string relPath = Path.GetDirectoryName(sd.FileName);
                if (initialFolder != null)
                {
                    try
                    {
                        relPath = FileUtils.GetRelativePath(sd.FileName, initialFolder);
                    }
                    catch (Exception ex)
                    {
                        mmApp.Log($"Failed to get relative path.\r\nFile: {sd.FileName}, Path: {imagePath}", ex);
                    }
                    imagePath = relPath;
                }

                if (imagePath.Contains(":\\"))
                    imagePath = "file:///" + imagePath;

                imagePath = imagePath.Replace("\\", "/");

                this.Image = imagePath;
                IsMemoryImage = false;
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

        private void PasteImageFromClipboard()
        {
            SetImagePreview(Clipboard.GetImage());
            IsMemoryImage = true;
        }

        private void Button_PasteImage(object sender, RoutedEventArgs e)
        {
            PasteImageFromClipboard();
        }

        #endregion

        #region Image Display

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

        private void SetImagePreview(BitmapSource source)
        {
            try
            {
                ImagePreview.Source = source;
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

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}
