using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for RegisterDialog.xaml
    /// </summary>
    public partial class RegisterDialog : Window, INotifyPropertyChanged
    {

        public RegisterDialog(bool closeOwner = false)
        {
            InitializeComponent();
            var accessCount = mmApp.Configuration.ApplicationUpdates.AccessCount;
            RunUsage.Text = $"{accessCount} times";

            Owner = mmApp.Model.Window;

            if (accessCount > 200)
                RunUsage.Foreground = Brushes.Red;
            else if (accessCount > 120)
                RunUsage.Foreground = Brushes.Firebrick;
            else if (accessCount > 50)
                RunUsage.Foreground = Brushes.Orange;
            else if (accessCount > 10)
                RunUsage.Foreground = Brushes.Green;

            DataContext = this;

            Topmost = true;
        }



        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            // hide the MainWindow when showing the reminder window
            // have to do this here or else the window won't activate in the right place
            Owner.Top -= 10000;

            Topmost = false;
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            var accessCount = mmApp.Configuration.ApplicationUpdates.AccessCount;
            if (accessCount > 70)
            {
                ShellUtils.GoUrl(mmApp.Urls.RegistrationUrl);
            }

            Close();
        }

        private void Register_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            ShellUtils.GoUrl(mmApp.Urls.RegistrationUrl);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
