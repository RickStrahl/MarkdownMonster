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
using FontAwesome.WPF;
using LibGit2Sharp;
using MahApps.Metro.Controls;
using MarkdownMonster.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for GitCommitDialog.xaml
    /// </summary>
    public partial class GitCommitDialog : MetroWindow
    {
        public GitCommitModel CommitModel { get; set; }

        public AppModel AppModel { get; set; }
        

        public GitCommitDialog(string fileOrPath, bool commitRepo = false)
        {
            InitializeComponent();
            AppModel = mmApp.Model;

            CommitModel = new GitCommitModel(fileOrPath,commitRepo);
            if (!commitRepo)
                CommitModel.CommitMessage = $"Updating documentation in {System.IO.Path.GetFileName(fileOrPath)}";

            CommitModel.CommitWindow = this;

            mmApp.SetThemeWindowOverride(this);

            Owner = AppModel.Window;
            Loaded += GitCommitDialog_Loaded;
        }

        private void GitCommitDialog_Loaded(object sender, RoutedEventArgs e)
        {

            if (mmApp.Configuration.GitCommitBehavior == GitCommitBehaviors.CommitAndPush)
            {
                ButtonCommitAndPush.IsDefault = true;
                ButtonCommitAndPush.FontWeight = FontWeight.FromOpenTypeWeight(600);
            }            
            else
            {
                ButtonCommit.IsDefault = true;
                ButtonCommit.FontWeight = FontWeight.FromOpenTypeWeight(600);
            }
            

            CommitModel.GetRepositoryChanges();

            DataContext = CommitModel;

            TextCommitMessage.Focus();
        }


        #region Button  Handlers

        private void ButtonCommit_Click(object sender, RoutedEventArgs e)
        {
            if (CommitModel.CommitChangesToRepository())
            {
                if (string.IsNullOrEmpty(mmApp.Configuration.GitName))
                {
                    mmApp.Configuration.GitName = CommitModel.GitUsername;
                    mmApp.Configuration.GitEmail = CommitModel.GitEmail;
                }
                Close();

                AppModel.Window.ShowStatus("Files have been committed in the local repository.", mmApp.Configuration.StatusMessageTimeout);
            }
        }

        private void ButtonCommitAndPush_Click(object sender, RoutedEventArgs e)
        {
            if (CommitModel.CommitChangesToRepository(true))
            {
                if (string.IsNullOrEmpty(mmApp.Configuration.GitName))
                {
                    mmApp.Configuration.GitName = CommitModel.GitUsername;
                    mmApp.Configuration.GitEmail = CommitModel.GitEmail;
                }
                Close();

                AppModel.Window.ShowStatus("Files have been committed and pushed to the remote.",mmApp.Configuration.StatusMessageTimeout);
            }            
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void ButtonFileSelection_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void CheckCommitRepository_Click(object sender, RoutedEventArgs e)
        {
            var gh = new GitHelper();
            var repo = gh.OpenRepository(CommitModel.Filename);
            if (repo == null)
            {
                ShowStatus("Couldn't open repository.");
            }

            gh.GetChanges();

        }

        #endregion

        #region StatusBar Display
        
        DebounceDispatcher debounce = new DebounceDispatcher();
        public void ShowStatus(string message = null, int milliSeconds = 0,
            FontAwesomeIcon icon = FontAwesomeIcon.None,
            Color color = default(Color),
            bool spin = false)
        {
            if (icon != FontAwesomeIcon.None)
                SetStatusIcon(icon, color, spin);

            if (message == null)
            {
                message = "Ready";
                SetStatusIcon();
            }

            StatusText.Text = message;

            if (milliSeconds > 0)
            {
                // debounce rather than delay so if something else displays
                // a message the delay timer is 'reset'
                debounce.Debounce(milliSeconds, (win) =>
                {
                    var window = win as MainWindow;
                    window.ShowStatus(null, 0);
                }, this);
            }

            WindowUtilities.DoEvents();
        }

        /// <summary>
        /// Status the statusbar icon on the left bottom to some indicator
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        /// <param name="spin"></param>
        public void SetStatusIcon(FontAwesomeIcon icon, Color color, bool spin = false)
        {
            StatusIcon.Icon = icon;
            StatusIcon.Foreground = new SolidColorBrush(color);
            if (spin)
                StatusIcon.SpinDuration = 1;

            StatusIcon.Spin = spin;
        }

        /// <summary>
        /// Resets the Status bar icon on the left to its default green circle
        /// </summary>
        public void SetStatusIcon()
        {
            StatusIcon.Icon = FontAwesomeIcon.Circle;
            StatusIcon.Foreground = new SolidColorBrush(Colors.Green);
            StatusIcon.Spin = false;
            StatusIcon.SpinDuration = 0;
            StatusIcon.StopSpin();
        }

        #endregion
    }

}
