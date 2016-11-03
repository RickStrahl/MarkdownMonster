using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using FontAwesome.WPF;
using MahApps.Metro.Controls;
using MarkdownMonster;
using ScreenCaptureAddin;
using Westwind.Utilities;

namespace SnagItAddin
{    
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class ScreenOverlayDesktop: System.Windows.Window
    {

        Bitmap Bitmap = null;
        private ScreenCaptureForm CaptureForm;

        public ScreenOverlayDesktop(ScreenCaptureForm screenCaptureForm)
        {
            CaptureForm = screenCaptureForm;

            InitializeComponent();

            Closing += ScreenOverlayDesktop_Closing;
        }

        private void ScreenOverlayDesktop_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Bitmap?.Dispose();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
        }

        public void SetDesktop()
        {
            var handle = ScreenCapture.GetDesktopWindow();
            Bitmap = ScreenCapture.CaptureWindowBitmap(handle);

            Left = 0;
            Top = 0;
            Width = Bitmap.Width;
            Height = Bitmap.Height;

            Debug.WriteLine("Screen: " + Bitmap.Width + "x" + Bitmap.Height);

            DesktopImage.Source = ScreenCapture.BitmapToBitmapSource(Bitmap);
        }

        public new void Hide()
        {
            Bitmap?.Dispose();
            base.Hide();            
        }

        //private void DesktopImage_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    Debug.WriteLine("Mouse Button Clicked...");
        //    MessageBox.Show("Stop Capture - Left Mouse on Desktop Window");
        //    CaptureForm.StopCapture();
        //}
    }    
    
}
