using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    public class InputBox
    {

        public string HeaderText { get; set; } = "Header";

        public string DescriptionText { get; set; } = "Description";

        /// <summary>
        /// Input text entered by the user. Can be set on entry
        /// </summary>
        public string InputText { get; set; }

        /// <summary>
        /// Optional place holder text to display
        /// </summary>
        public string InputPlaceholderText { get; set; }

        public string Image { get; set; }

        /// <summary>
        /// Number of buttons to put on the input form.
        /// Buttons are number from left to right.
        /// </summary>
        public int ButtonCount { get; set; } = 2;

        /// <summary>
        /// Text for Button 1 - typically the OK button
        /// </summary>
        public string Button1Text { get; set; } = "OK";

        /// <summary>
        /// Text for Button 2 - typically for Cancel button
        /// </summary>
        public string Button2Text { get; set; } = "Cancel";


        public string Button1FontAwesomeIcon { get; set; } = "Check";
        public string Button2FontAwesomeIcon { get; set; } = "Remove";


        /// <summary>
        /// The width of the dialog
        /// </summary>
        public int DialogWidth { get; set; } = 550;

        /// <summary>
        /// The height of the dialog
        /// </summary>
        public int DialogHeight { get; set; } = 280;

        /// <summary>
        /// The parent window which ensures the dialog
        /// is centered in the parent window
        /// </summary>
        public Window ParentWindow { get; set; }

        /// <summary>
        /// Result Value you can set in the Action that is fired in response
        /// to a button click.
        /// </summary>
        public object Result { get; set; }

        public bool Cancelled { get; set; }

        /// <summary>
        /// On Click Handler fired when one of the buttons is clicked.
        /// Passed the button, the window and the button index (1-3)
        /// </summary>
        public Func<Button,Window,int, object> OnClickHandler;



        public string Show()
        {
            var form = new InputBoxForm(this);
            form.Owner = mmApp.Model.Window;
            form.ShowDialog();

            return InputText;
        }

    }

    /// <summary>
    /// Interaction logic for RegistrationForm.xaml
    /// </summary>
    public partial class InputBoxForm : MetroWindow
    {
        public InputBoxForm(InputBox inputBox)
        {
            InputBox = inputBox;

            // Binding for some reason doesn't work right so explictly setting width and height
            Height = InputBox.DialogHeight;
            Width = InputBox.DialogWidth;

            InitializeComponent();

            mmApp.SetThemeWindowOverride(this);

            DataContext = InputBox;
            Loaded += OnLoaded;
        }

        /// <summary>
        /// The model that is used to render the form and controls
        /// </summary>
        public InputBox InputBox { get; set; }


        private void OnLoaded(object s, RoutedEventArgs e)
        {

            if (InputBox.ButtonCount > 1)
                Button2.Visibility = Visibility.Visible;

            TextInputText.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int buttonId = 0;
            string result = null;

            if (sender == Button1)
            {
                buttonId = 1;
            }
            else if (sender == Button2)
            {
                buttonId = 2;
                InputBox.Cancelled = true;
            }

            if (InputBox.OnClickHandler != null)
               InputBox.Result =  InputBox.OnClickHandler.Invoke(sender as Button,this, buttonId);

            Close();
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            InputBox.InputText = null;
            Close();

            InputBox.Cancelled = true;
        }
    }
}
