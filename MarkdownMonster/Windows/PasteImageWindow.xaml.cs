using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using MarkdownMonster.AddIns;
using Microsoft.Win32;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for PasteHref.xaml
    /// </summary>
    public partial class PasteImageWindow : MetroWindow, INotifyPropertyChanged
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
                OnPropertyChanged(nameof(IsEditable));
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

     
        public bool PasteAsBase64Content
        {
            get { return _PasteAsBase64Content; }
            set
            {
                if (value == _PasteAsBase64Content) return;
                _PasteAsBase64Content = value;
                OnPropertyChanged(nameof(PasteAsBase64Content));
            }
        }
        private bool _PasteAsBase64Content = false;




        public string MarkdownFile { get; set; }

        public AppModel AppModel { get; set; } = mmApp.Model;

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
                OnPropertyChanged(nameof(IsPreview));
            }
        }
        private bool _isMemoryImage;

        public bool IsEditable
        {
            get { return !string.IsNullOrEmpty(Image); }
        }

        public bool IsPreview
        {
            get { return ImagePreview.Source != null;  }
        }


        AppModel Model { get; set; }
        MarkdownDocumentEditor Editor { get; set; }
        MarkdownDocument Document { get; set; }

         StatusBarHelper StatusBar { get;  }


        public PasteImageWindow(MainWindow window)
        {
            InitializeComponent();

            Owner = window;
            DataContext = this;

            mmApp.SetThemeWindowOverride(this);

            Loaded += PasteImage_Loaded;
            SizeChanged += PasteImage_SizeChanged;
            Activated += PasteImage_Activated;
            PreviewKeyDown += PasteImage_PreviewKeyDown;


            Model = window.Model;
            Editor = Model.ActiveEditor;
            Document = Model.ActiveDocument;

            StatusBar = new StatusBarHelper(StatusText, StatusIcon);
        }


        private void PasteImage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isControlKey = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            var clipText = Clipboard.GetText();

            if (isControlKey && e.Key == Key.V && Clipboard.ContainsImage())
                PasteImageFromClipboard();                            
            else if (isControlKey && e.Key == Key.C)
                Button_CopyImage(null, null);
        }

        private void PasteImage_Activated(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Image) && Clipboard.ContainsImage())
                PasteImageFromClipboard();
        }

        private void PasteImage_Loaded(object sender, RoutedEventArgs e)
        {

            PasteCommand = new CommandBase((s, args) =>
            {
                MessageBox.Show("PasteCommand");
            });

            TextImage.Focus();
            if (string.IsNullOrEmpty(Image) && Clipboard.ContainsImage())
            {
                PasteImageFromClipboard();
            }
            else if (string.IsNullOrEmpty(Image) && Clipboard.ContainsText())
            {
                string clip = Clipboard.GetText().ToLower();
                if ((clip.StartsWith("http://",StringComparison.InvariantCultureIgnoreCase) || clip.StartsWith("https://",StringComparison.InvariantCultureIgnoreCase)) &&
                    (clip.Contains(".png",StringComparison.InvariantCultureIgnoreCase) || clip.Contains("jpg",StringComparison.InvariantCultureIgnoreCase)))
                {
                    TextImage.Text = clip;
                    SetImagePreview(clip);
                }
            }
        }

        private void TextImage_LostFocus(object sender, RoutedEventArgs e)
        {

            if (!IsMemoryImage && string.IsNullOrEmpty(TextImage.Text))
            {
                ImagePreview.Source = null;
                return;
            }

            string href = TextImage.Text.ToLower();
            if (href.StartsWith("http://") || href.StartsWith("https://"))
            {
                SetImagePreview(TextImage.Text);
            }
        }

        #region Main Buttons

        /// <summary>
        /// Saves an image loaded from clipboard to disk OR if base64 is checked
        /// creates the base64 content.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_SaveImage(object sender, RoutedEventArgs e)
        {
            string imagePath = null;

            var bitmapSource = ImagePreview.Source as BitmapSource;
            if (bitmapSource == null)
            {
                MessageBox.Show("Unable to convert bitmap source.", "Bitmap conversion error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                    return;
            }

            using (var bitMap = WindowUtilities.BitmapSourceToBitmap(bitmapSource))
            {
                if (bitMap == null)
                    return;

                imagePath = AddinManager.Current.RaiseOnSaveImage(bitMap);

                if (PasteAsBase64Content)
                {
                    Base64EncodeImage(bitMap);
                    IsMemoryImage = false;
                    return;
                }

                if (!string.IsNullOrEmpty(imagePath))
                {
                    TextImage.Text = imagePath;
                    IsMemoryImage = false;
                    return;
                }

                string initialFolder = null;
                string documentFolder = null;
                if (!string.IsNullOrEmpty(Document.Filename) && Document.Filename != "untitled")
                {

                    documentFolder = Path.GetDirectoryName(Document.Filename);
                    if (!string.IsNullOrEmpty(Document.LastImageFolder))
                        initialFolder = Document.LastImageFolder;
                    else
                        initialFolder = documentFolder;
                }

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

                        if (ext == ".jpg" || ext == ".jpeg")
                            ImageUtils.SaveJpeg(bitMap, imagePath, mmApp.Configuration.JpegImageCompressionLevel);

                        else
                        {
                            using (var fileStream = new FileStream(imagePath, FileMode.Create))
                            {
                                BitmapEncoder encoder = null;
                                if (ext == ".png")
                                    encoder = new PngBitmapEncoder();
                                else if (ext == ".gif")
                                    encoder = new GifBitmapEncoder();

                                encoder.Frames.Add(BitmapFrame.Create(ImagePreview.Source as BitmapSource));
                                encoder.Save(fileStream);

                                if (ext == ".png" || ext == ".jpeg")
                                    mmFileUtils.OptimizeImage(sd.FileName); // async
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Couldn't save file: \r\n" + ex.Message, mmApp.ApplicationName);
                        return;
                    }


                    string relPath = Path.GetDirectoryName(sd.FileName);
                    Document.LastImageFolder = relPath;

                    if (documentFolder != null)
                    {
                        try
                        {
                            relPath = FileUtils.GetRelativePath(sd.FileName, documentFolder);
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

                    Image = imagePath;
                    IsMemoryImage = false;

                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowUtilities.FixFocus(this, CheckPasteAsBase64Content);
            if (sender == ButtonCancel)
                DialogResult = false;
            else
                DialogResult = true;

            Close();
        }

        private void Button_PasteImage(object sender, RoutedEventArgs e)
        {
            PasteImageFromClipboard();
        }

        private void CheckPasteAsBase64Content_Checked(object sender, RoutedEventArgs e)
        {
            if (PasteAsBase64Content)
                if (IsMemoryImage)
                {
                    Base64EncodeImage(WindowUtilities.BitmapSourceToBitmap(ImagePreview.Source as BitmapSource));
                    IsMemoryImage = false;
                }
                else
                    Base64EncodeImage(Image);
            else
                Image = null;

        }

        private void Button_EditImage(object sender, RoutedEventArgs e)
        {
            string exe = mmApp.Configuration.ImageEditor;

            if (string.IsNullOrEmpty(Image))
            {
                StatusBar.ShowStatusError("No image selected.");
                return;
            }

            string imageFile = Image;
            if (!imageFile.Contains(":\\") && Document != null)
            {
                imageFile = Path.Combine(Path.GetDirectoryName(Document.Filename),
                    Image);
            }

			if (!mmFileUtils.OpenImageInImageEditor(imageFile))
			{
				MessageBox.Show("Unable to launch image editor " + Path.GetFileName(mmApp.Configuration.ImageEditor) +
				                "\r\n\r\n" +
				                "Most likely the image editor configured in settings is not a valid executable. Please check the 'ImageEditor' key in the Markdown Monster Settings.\r\n\r\n" +
				                "We're opening the settings file for you in the editor now.",
					"Image Launching Error",
					MessageBoxButton.OK, MessageBoxImage.Warning);

				mmApp.Model.Window.OpenTab(Path.Combine(mmApp.Configuration.CommonFolder, "MarkdownMonster.json"));
			}
			else
				StatusBar.ShowStatusSuccess($"Launching editor {exe} with {imageFile}");
        }

        private void Button_ClearImage(object sender, RoutedEventArgs e)
        {
            Image = null;
            ImageText = null;
            ImagePreview.Source = null;
            IsMemoryImage = false;

            StatusBar.ShowStatusSuccess("Image has been cleared.");
        }

        private void Button_CopyImage(object sender, RoutedEventArgs e)
        {
            if (ImagePreview.Source != null)
            {
                var src = ImagePreview.Source as BitmapSource;
                if (src != null)
                {
                    Clipboard.SetImage(src);
                    StatusBar.ShowStatus("Image copied to the Clipboard.");
                }
            }
        }

        private void Button_SelectLocalImageFile_Click(object sender, RoutedEventArgs e)
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

            if (!string.IsNullOrEmpty(Document.LastImageFolder))
                fd.InitialDirectory = Document.LastImageFolder;
            else if (!string.IsNullOrEmpty(MarkdownFile))
                fd.InitialDirectory = Path.GetDirectoryName(MarkdownFile);
            else
                fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            var res = fd.ShowDialog();
            if (res == null || !res.Value)
                return;


            Image = fd.FileName;
            Document.LastImageFolder = Path.GetDirectoryName(fd.FileName);

            if (PasteAsBase64Content)
            {
                var bmi = new BitmapImage(new Uri(fd.FileName))
                {
                    CreateOptions = BitmapCreateOptions.IgnoreImageCache // don't lock file
                };

                Base64EncodeImage(fd.FileName);
                ImagePreview.Source = bmi;
                return;
            }


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
                        string newImageFileName = Path.Combine(mdPath, System.IO.Path.GetFileName(fd.FileName));
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
                                Document.LastImageFolder = Path.GetDirectoryName(sd.FileName);
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
            TextImageText.Focus();
        }



        #endregion

        #region Image Display

        private void PasteImageFromClipboard()
        {
            SetImagePreview(Clipboard.GetImage());
            Image = null;
            IsMemoryImage = true;            
                       

            StatusBar.ShowStatusSuccess("Image pasted from clipboard.");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file"></param>
        public void Base64EncodeImage(string file)
        {
            try
            {
                if (!PasteAsBase64Content ||
                    Image == null ||
                    Image.StartsWith("data:image/") ||
                    file == "Untitled")
                    return;

                file = file.Replace("file:///", "");

                var fullPath = file;
                if (!File.Exists(file))
                    fullPath = Path.Combine(Path.GetDirectoryName(Editor.MarkdownDocument.Filename), file);

                if (File.Exists(fullPath))
                {
                    var bytes = File.ReadAllBytes(fullPath);
                    var bytestring = Convert.ToBase64String(bytes);
                    var mediaFormat = ImageUtils.GetImageMediaTypeFromFilename(fullPath);
                    Image = $"data:{mediaFormat};base64," + bytestring;
                }
            }
            catch (Exception ex)
            {
                StatusBar.ShowStatusError("Image base64 encoding failed: " + ex.GetBaseException().Message);
            }
        }

        public void Base64EncodeImage(Bitmap bmp)
        {
            try
            {
                using (var ms = new MemoryStream(10000))
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    ms.Flush();
                    Image = $"data:image/jpeg;base64,{Convert.ToBase64String(ms.ToArray())}";
                }
            }
            catch (Exception ex)
            {
                StatusBar.ShowStatusError($"Image base64 encoding failed: {ex.GetBaseException().Message}");
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


        /// <summary>
        /// Attempts to resolve the full image filename from the active image
        /// if the image is a file based image with a relative or physical
        /// path but not a URL based image.
        /// </summary>
        /// <returns></returns>
        public string GetFullImageFilename(string filename = null)
        {
            string imageFile = filename ?? Image;

            if (string.IsNullOrEmpty(imageFile))
                return null;

            try
            {
                if (!File.Exists(imageFile))
                    imageFile = Path.Combine(Path.GetDirectoryName(Editor.MarkdownDocument.Filename), imageFile);

                return File.Exists(imageFile) ? imageFile : null;
            }
            catch
            {
                mmApp.Log("Non-fatal error: Invalid image filename: " + imageFile);
                return null;
            }
        }

        public void SetImagePreview(string url = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                url = GetFullImageFilename();
                if (url == null)
                    url = Image;
            }

            try
            {

                var bmi = new BitmapImage();
                bmi.BeginInit();
                //bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.UriSource = new Uri(url);
                bmi.EndInit();

                SetImagePreview(bmi);
                
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
