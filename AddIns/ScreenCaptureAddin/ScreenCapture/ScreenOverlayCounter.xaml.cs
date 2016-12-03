using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

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
            Left = System.Windows.SystemParameters.WorkArea.Right - Width - 50;
            Top = System.Windows.SystemParameters.WorkArea.Bottom - Height - 50;
        }
       

        public void SetWindowText(string text)
        {
            TextSize.Text = text;
        }

    }    
}
