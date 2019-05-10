using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for RegistrationForm.xaml
    /// </summary>
    public partial class RegistrationForm : Window
    {
        public RegistrationForm()
        {
            InitializeComponent();

            DataContext = this;

            if (UnlockKey.IsRegistered())
            {
                LabelIsRegistered.Text = "This copy is already registered. Only enter key to clear or reset.";
                LabelIsRegistered.FontWeight = FontWeights.DemiBold;
                LabelIsRegistered.Foreground = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                LabelIsRegistered.Text = "This copy is not registered.";
                LabelIsRegistered.FontWeight = FontWeights.Normal;
            }

            Loaded += (s, e) =>
            {
                TextRegKey.Focus();
            };
        }

        private void Register_Click(object sender, RoutedEventArgs routedEventArgs)
        {

            bool registered = UnlockKey.Register(TextRegKey.Password);

            mmApp.Model.Window.SetWindowTitle();
            WindowUtilities.DoEvents();

            if (registered)
            {
                MessageBox.Show("Thank you for your registration.", mmApp.ApplicationName,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Close();

                UnlockKey.Shutdown();
            }
            else
            {
                MessageBox.Show("Sorry, that's not the right key.\r\nMake sure you entered the value exactly as it\r\n" +
                    "appears in your confirmation.\r\n\r\n", "Software Registration",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                LabelIsRegistered.Text = "This copy is not registered.";
                LabelIsRegistered.Foreground = new SolidColorBrush(Colors.Red);
                UnlockKey.UnRegister();
            }

            
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Purchase_Click(object sender, RoutedEventArgs e)
        {
            ShellUtils.GoUrl("https://store.west-wind.com/product/order/markdown_monster");
        }
    }
}
