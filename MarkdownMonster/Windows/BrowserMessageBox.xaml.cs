using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Button = System.Windows.Controls.Button;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// A generic Browser based message box with generic buttons
    /// that can be customized and added to display custom
    /// messages.
    /// </summary>
    public partial class BrowserMessageBox : MetroWindow
    {
        public new bool IsLoaded { get; set; }

        /// <summary>
        /// Hold the button that was clicked to close this form
        /// </summary>
        public Button ButtonResult { get; set;  }
       

        public BrowserMessageBox()
        {
            InitializeComponent();

            this.Browser.LoadCompleted += Browser_LoadCompleted;

            mmApp.SetThemeWindowOverride(this);

        }

        /// <summary>
        /// Provide a function that passes a control RoutedEventArgs and returns true
        /// if the form should close or false if it shouldn't.
        /// </summary>
        public Func<object, RoutedEventArgs, BrowserMessageBox,bool> ButtonClickHandler { get; set; }

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

        public new bool? ShowDialog()
        {
            if (string.IsNullOrEmpty(TextMessage.Text))
                TextMessage.Visibility = Visibility.Collapsed;
            else
                TextMessage.Visibility = Visibility.Visible;

            return base.ShowDialog();
        }

        public new void Show()
        {
            if (string.IsNullOrEmpty(TextMessage.Text))
                TextMessage.Visibility = Visibility.Collapsed;
            else
                TextMessage.Visibility = Visibility.Visible;
            base.Show();
        }


        /// <summary>
        /// Default HTML template text used to render HTML
        /// If not specified the Preview Template is used for preview.
        ///
        /// Should include the same template settings as Preview Templates
        /// </summary>
        public string HtmlTemplatePath { get; set; }

        public void ShowMarkdown(string markdown)
        {
            var doc = new MarkdownDocument();
            doc.CurrentText = markdown;
            doc.RenderHtmlToFile();

            Navigate(doc.HtmlRenderFilename);
        }

        public void ShowHtml(string html, bool isFullHtmlDocument)
        {
            throw new NotImplementedException();
        }

        public void SetMessage(string messageText)
        {
            TextMessage.Text = messageText;
        }



        public void ClearButtons()
        {
            PanelButtonContainer.Children.Clear();
            buttonCount = 0;
        }

        private int buttonCount = 0;

        public void AddButton(Button button, string text = null, ImageSource icon = null, Colors color = default(Colors))
        {
            if (button.MinWidth < 100)
                button.MinWidth = 100;
            PanelButtonContainer.Children.Add(button);
            button.Margin = new Thickness(5, 0, 5, 0);

            buttonCount++;
            if (button.CommandParameter == null)
                button.CommandParameter = buttonCount;

            button.Tag = this;
        }


        void OnButtonClicked(object sender, RoutedEventArgs ev)
        {
            ButtonResult = sender as Button;
            if (ButtonClickHandler != null)
                if (ButtonClickHandler.Invoke(sender, ev, this))
                    Close();
            else
            {                
                Close();
            }
        }
    }
}
