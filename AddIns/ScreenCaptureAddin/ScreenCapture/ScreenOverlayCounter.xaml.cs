using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
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
    public partial class ScreenOverlayCounter: System.Windows.Window
    {

        public ScreenOverlayCounter()
        {
            InitializeComponent();
            Loaded += ScreenOverlayCounter_Loaded;
        }

        private void ScreenOverlayCounter_Loaded(object sender, RoutedEventArgs e)
        {
            var screen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
            Left = screen.Bounds.Width - Width - 50;
            Top = screen.Bounds.Height- Height - 50;
        }
       

        public void SetWindowText(string text)
        {
            TextSize.Text = text;
        }

    }    
}
