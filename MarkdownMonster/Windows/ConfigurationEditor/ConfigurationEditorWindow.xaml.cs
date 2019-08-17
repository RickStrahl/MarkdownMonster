using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace MarkdownMonster.Windows.ConfigurationEditor
{
    /// <summary>
    /// Interaction logic for ConfigurationEditorWindow.xaml
    /// </summary>
    public partial class ConfigurationEditorWindow 
    {
        public ConfigurationEditorModel Model { get; set; }
        
        public ConfigurationEditorWindow()
        {
            InitializeComponent();

            Model = new ConfigurationEditorModel();
            DataContext = Model;
            Model.EditorWindow = this;

            mmApp.SetThemeWindowOverride(this);

            

            Loaded += ConfigurationEditorWindow_Loaded;
        }

        private async void RefreshPropertyListAsync()
        {
            Dispatcher.InvokeAsync(() => Model.AddConfigurationsAsync(PropertiesPanel), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private async void ConfigurationEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshPropertyListAsync();
        }



        private void ButtonManualSettings_Click(object sender, RoutedEventArgs e)
        {
            Model.AppModel.Commands.SettingsCommand.Execute(null);
            Close();
        }


        private async void Search_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            RefreshPropertyListAsync();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Model.Window.SaveSettings();
            Model.AppModel.ActiveEditor?.RestyleEditor();
            Close();
        }
    }
}
