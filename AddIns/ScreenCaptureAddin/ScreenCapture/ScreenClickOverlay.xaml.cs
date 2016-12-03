using System;
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
using Westwind.Utilities;

namespace SnagItAddin
{    
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class ScreenClickOverlay: System.Windows.Window
    {
        ScreenCaptureForm ScreenCaptureForm { get; set; }

        public ScreenClickOverlay(ScreenCaptureForm form)
        {
            ScreenCaptureForm = form;
            InitializeComponent();
        }


        //protected override void OnSourceInitialized(EventArgs e)
        //{
        //    base.OnSourceInitialized(e);

        //    var hwnd = new WindowInteropHelper(this).Handle;
        //    WindowsServices.SetWindowExTransparent(hwnd);
        //}

        public void SetWindowText(string text)
        {
            TextSize.Text = text;
        }
        
        private void TextSize_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ScreenCaptureForm.StopCapture();
        }
    }


    
}
