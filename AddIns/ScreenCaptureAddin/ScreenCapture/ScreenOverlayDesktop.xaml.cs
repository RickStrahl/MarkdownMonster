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
using MarkdownMonster.Windows;
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
            WindowUtilities.MakeWindowCompletelyTransparent(hwnd);
        }

        public void SetDesktop(bool includeCursor = false)
        {
            var handle = ScreenCapture.GetDesktopWindow();
            Bitmap = ScreenCapture.CaptureWindowBitmap(handle);

            if (includeCursor)
            {
                var MousePointerInfo = ScreenCapture.GetMousePointerInfo();
                ScreenCapture.DrawMousePointer(MousePointerInfo, Bitmap);
            }

            var dpiRatio = (double)WindowUtilities.GetDpiRatio(handle);
            Left = 0;
            Top = 0;
            Width = Bitmap.Width / dpiRatio;
            Height = Bitmap.Height / dpiRatio;

            DesktopImage.Source = ScreenCapture.BitmapToBitmapSource(Bitmap);
        }

        public new void Hide()
        {
            Bitmap?.Dispose();
            base.Hide();            
        }
    }    
    
}
