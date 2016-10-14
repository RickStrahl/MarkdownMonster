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

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for BrowserDialog.xaml
    /// </summary>
    public partial class BrowserDialog : Window
    {
        public new bool IsLoaded { get; set; }

        public BrowserDialog(string url = null)
        {
            InitializeComponent();

            this.Browser.LoadCompleted += Browser_LoadCompleted;

            if (!string.IsNullOrEmpty(url))
                Browser.Navigate(url);
        }

        public void Navigate(string url)
        {
            IsLoaded = false;
            Browser.Navigate(url);
        }

        public bool NavigateAndWaitForCompletion(string url)
        {
            IsLoaded = false;
            Browser.Navigate(url);

            WindowUtilities.DoEvents();

            for (int i = 0; i < 200; i++)
            {
                dynamic doc = Browser.Document;

                if (!IsLoaded)
                {
                    Task.Delay(10);
                    WindowUtilities.DoEvents();
                }
            }

            return IsLoaded;
        }

        private void Browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            IsLoaded = true;            
        }
    }
}
