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
using System.Windows.Threading;
using MahApps.Metro.Controls;

namespace MarkdownMonster.Windows.ConfigurationEditor
{
    /// <summary>
    /// Interaction logic for ConfigurationEditorWindow.xaml
    /// </summary>
    public partial class ConfigurationEditorWindow 
    {
        public ConfigurationEditorModel Model { get; set; }

        public StatusBarHelper StatusBar { get; set; }
        
        public ConfigurationEditorWindow()
        {
            InitializeComponent();

            Model = new ConfigurationEditorModel();
            DataContext = Model;
            Model.EditorWindow = this;
            mmApp.SetThemeWindowOverride(this);

            StatusBar = new StatusBarHelper(StatusText, StatusIcon);

            Loaded += ConfigurationEditorWindow_Loaded;

            Model.PropertyChanged += Model_PropertyChanged;
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SearchText" || e.PropertyName == "SectionName")
            {
                RefreshPropertyListAsync();
                PropertiesScrollContainer.ScrollToHome();
            }
        }

        private async void RefreshPropertyListAsync()
        {
            Model.AddConfigurationsAsync(PropertiesPanel);
        }

        private async void ConfigurationEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshPropertyListAsync();
            TextSearch.Focus();
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
        
    }
}
