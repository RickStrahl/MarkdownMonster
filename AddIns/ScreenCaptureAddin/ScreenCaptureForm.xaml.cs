using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    public partial class ScreenCaptureForm : MetroWindow
    {        
        public ScreenCaptureConfigurationModel ConfigurationModel;

        public bool IsPreCaptureMode = false;

        #region Startup and Shutdown

        public ScreenCaptureForm()
        {
            ConfigurationModel = new ScreenCaptureConfigurationModel()
            {                
                Configuration = ScreenCaptureConfiguration.Current,                                               
            };

            mmApp.SetTheme(mmApp.Configuration.ApplicationTheme);

            InitializeComponent();


            DataContext = ConfigurationModel;
            
            Loaded += ScreenCaptureConfiguration_Loaded;
            Closing += ScreenCaptureConfiguration_Closing;
        }

        
        private void ScreenCaptureConfiguration_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

            if (IsPreCaptureMode)
            {
                SubmitButtonText.Text = "Capture";
                Title = "Markdown Monster SnagIt Screen Capture";
            }
        }


        private void ScreenCaptureConfiguration_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // save settings
            ScreenCaptureConfiguration.Current.Write();
        }
        #endregion

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (IsPreCaptureMode)
                DialogResult = true;

            if (!ScreenCaptureConfiguration.Current.Write())
                mmApp.Log("Failed to save Screen Capture " + ScreenCaptureConfiguration.Current.ErrorMessage + "\r\n" + 
                    JsonSerializationUtils.Serialize(ScreenCaptureConfiguration.Current));
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {            
            if(IsPreCaptureMode)
                DialogResult = false;

            Close();
        }
    }
}
