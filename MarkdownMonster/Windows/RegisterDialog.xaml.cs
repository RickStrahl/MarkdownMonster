using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MarkdownMonster.Annotations;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for RegisterDialog.xaml
    /// </summary>
    public partial class RegisterDialog : Window, INotifyPropertyChanged
    {

        public RegisterDialog()
        {
            InitializeComponent();
            var accessCount = mmApp.Configuration.ApplicationUpdates.AccessCount;
            RunUsage.Text = $"{accessCount} times";

            if (accessCount > 200)
                RunUsage.Foreground = Brushes.Red;
            else if (accessCount > 120)
                RunUsage.Foreground = Brushes.Firebrick;
            else if (accessCount > 50)
                RunUsage.Foreground = Brushes.Orange;
            else if (accessCount > 10)
                RunUsage.Foreground = Brushes.Green;

            DataContext = this;

        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            var accessCount = mmApp.Configuration.ApplicationUpdates.AccessCount;
            if (accessCount > 70)
            {
                Register_Click(null, null);

                //int w = accessCount / 20;
                //if (w > 10)
                //    w = 10;
                //for (int i = 0; i < w; i++)
                //{                    
                //    ShutdownTimer.Text = $"Shutdown Timer: {w - i} seconds left";
                //    WindowUtilities.DoEvents();
                //    Thread.Sleep(1000);
                //}                
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
