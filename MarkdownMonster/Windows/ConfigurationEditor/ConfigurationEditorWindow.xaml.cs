using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using MarkdownMonster.Utilities;
using Microsoft.Win32;
using Westwind.Utilities;

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

        private void ConfigurationEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshPropertyListAsync();
            TextSearch.Focus();
        }

        private void RefreshPropertyListAsync()
        {
            var t = Model.AddConfigurationsAsync(PropertiesPanel);
        }


        private void Search_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            RefreshPropertyListAsync();
        }

        #region Toolbar Buttons

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonManualSettings_Click(object sender, RoutedEventArgs e)
        {
            Model.AppModel.Commands.SettingsCommand.Execute(null);
            Close();
        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            ShellUtils.GoUrl("https://markdownmonster.west-wind.com/docs/_4nk01yq6q.htm");
        }
        #endregion

        #region Backup Operations

        private void ButtonBackup_Click(object sender, RoutedEventArgs e)
        {
            ButtonBackup.ContextMenu.IsOpen = true;
        }

        private void Button_BackupToZip(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sd = new SaveFileDialog
            {
                FilterIndex = 1,
                InitialDirectory = Model.AppModel.Configuration.CommonFolder,
                FileName = $"Markdown-Monster-Configuration-Backup-{DateTime.Now.ToString("yyyy-MM-dd")}.zip",
                CheckFileExists = false,
                OverwritePrompt = true,
                CheckPathExists = true,
                RestoreDirectory = true
            };
            sd.Filter =
                "Zip files (*.zip)|*.zip|All files (*.*)|*.*";

            bool? result = null;
            result = sd.ShowDialog();
            if (result == null || !result.Value)
                return;

            var outputZipFile = sd.FileName;

            var bu = new BackupManager();
            if (!bu.BackupToZip(sd.FileName))
                StatusBar.ShowStatusError("Backup failed. Files have not been backed up.");
            else
            {
                StatusBar.ShowStatusSuccess("Backup completed.");
                ShellUtils.OpenFileInExplorer(sd.FileName);
            }

        }

        private void Button_BackupToFolder(object sender, RoutedEventArgs e)
        {
            var folder = mmWindowsUtils.ShowFolderDialog(Path.GetTempPath(),"Backup Configuration Files");

            if (string.IsNullOrEmpty(folder))
                return;

            
            var bu = new BackupManager();
            if (!bu.BackupToFolder(folder))
                StatusBar.ShowStatusError("Backup failed. Files have not been backed up to a Zip file.");
            else
            {
                StatusBar.ShowStatusSuccess("Backup to Folder completed.");
                ShellUtils.GoUrl(folder);
            }

        }
        #endregion
    }
}
