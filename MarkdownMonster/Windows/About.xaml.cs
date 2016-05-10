using System;
using System.Reflection;
using MahApps.Metro.Controls;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : MetroWindow
    {
        public About()
        {           
            InitializeComponent();
            
            mmApp.SetThemeWindowOverride(this);
            
            string bitNess = Environment.Is64BitProcess ? "64 bit" : "32 bit";
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            VersionLabel.Content = "Version " + v.Major + "." + v.Minor + " " + bitNess;
        }

        private void WestWindIcon_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShellUtils.GoUrl("http://weblog.west-wind.com");
        }
    }
}
