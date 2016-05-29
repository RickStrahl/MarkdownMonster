using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
    public partial class ScreenCaptureConfigurationForm : MetroWindow
    {
        //public SnageblogAddinModel Model { get; set;  }


        #region Startup and Shutdown

        public ScreenCaptureConfigurationForm()
        {            
            mmApp.SetTheme(mmApp.Configuration.ApplicationTheme);

            InitializeComponent();

            //mmApp.SetThemeWindowOverride(this);         

            //DataContext = Model;

            
            Loaded += SnagItConfiguration_Loaded;
            Closing += SnagItConfiguration_Closing;
        }

        private void SnagItConfiguration_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }


        private void SnagItConfiguration_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // save settings
            //WeblogApp.Configuration.Write();
        }

        #endregion
    }
}
