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
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for RegisterDialog.xaml
    /// </summary>
    public partial class RegisterDialog : Window
    {
        public RegisterDialog()
        {
            InitializeComponent();
            var accessCount = mmApp.Configuration.ApplicationUpdates.AccessCount;
            RunUsage.Text = $"Started {accessCount} times.";

            if (accessCount > 200)
                RunUsage.Foreground = Brushes.LightCoral;
            else if (accessCount > 120)
                RunUsage.Foreground = Brushes.Firebrick;
            else if (accessCount > 50)
                RunUsage.Foreground = Brushes.Orange;
            else if (accessCount > 10)
                RunUsage.Foreground = Brushes.Green;
            
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Register_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            ShellUtils.GoUrl("https://store.west-wind.com/product/MARKDOWN_MONSTER");
        }  
    }
}
