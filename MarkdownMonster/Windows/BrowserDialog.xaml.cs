using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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

            Browser.LoadCompleted += Browser_LoadCompleted;

            if (!string.IsNullOrEmpty(url))
                Browser.Navigate(url);
        }

        public void Navigate(string url)
        {
            IsLoaded = false;
            Browser.Navigate(url);
        }

        public bool NavigateAndWaitForCompletion(string url, int timeout = 2000)
        {
            IsLoaded = false;
            Browser.Navigate(url);

            //WindowUtilities.DoEvents();

            Dispatcher.Invoke( () =>
            {
                for (int i = 0; i < timeout/10; i++)
                {
                    if (!IsLoaded)
                    {
                        Thread.Sleep(10);
                        WindowUtilities.DoEvents();
                    }
                }
            });

            return IsLoaded;
        }

        private void Browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            IsLoaded = true;            
        }
    }
}
