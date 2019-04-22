using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FontAwesome.WPF;
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

            Browser.LoadCompleted += Browser_LoadCompleted;

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


        /// <summary>
        /// Shows the dialog modally and sets focus to the specified button
        /// </summary>
        /// <param name="focusedButton">0-n button to select. -1 to set no focus</param>
        /// <returns></returns>
        public new bool? ShowDialog(int focusedButton = 0)
        {
            if (string.IsNullOrEmpty(TextMessage.Text))
                TextMessage.Visibility = Visibility.Collapsed;
            else
                TextMessage.Visibility = Visibility.Visible;

            if (focusedButton > -1 && focusedButton < PanelButtonContainer.Children.Count)
            {
                var button = PanelButtonContainer.Children[focusedButton] as Button;
                button.Focus();
            }

            return base.ShowDialog();
        }


        /// <summary>
        /// Shows the dialog without waiting for completion
        /// </summary>
        /// <param name="focusedButton"></param>
        public new void Show(int focusedButton = 0)
        {
            if (string.IsNullOrEmpty(TextMessage.Text))
                TextMessage.Visibility = Visibility.Collapsed;
            else
                TextMessage.Visibility = Visibility.Visible;

            if (focusedButton > -1 && focusedButton < PanelButtonContainer.Children.Count)
            {
                var button = PanelButtonContainer.Children[focusedButton] as Button;
                button.Focus();
            }


            base.Show();
        }


        /// <summary>
        /// Default HTML template text used to render HTML
        /// If not specified the Preview Template is used for preview.
        ///
        /// Should include the same template settings as Preview Templates
        /// </summary>
        public string HtmlTemplatePath { get; set; }


        /// <summary>
        /// Renders a Markdown string as HTML using the
        /// currently active Markdown Monster Preview theme.
        /// </summary>
        /// <param name="markdown"></param>
        public void ShowMarkdown(string markdown)
        {
            var doc = new MarkdownDocument();
            doc.CurrentText = markdown;
            doc.RenderHtmlToFile(noBanner: true);

            Navigate(doc.HtmlRenderFilename);
        }

        public void ShowHtml(string html, bool isFullHtmlDocument = false)
        {
            var doc = new MarkdownDocument();
            if (!isFullHtmlDocument)
            {                
                doc.Filename = "test.html";
                doc.CurrentText = html;
                doc.RenderHtmlToFile(noBanner:true);
            }
            else
            {
                var filename = doc.HtmlRenderFilename;
                File.WriteAllText(filename,html);
            }

            Navigate(doc.HtmlRenderFilename);
        }

        /// <summary>
        /// Raw text to display in the 
        /// </summary>
        /// <param name="messageText"></param>
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

        public Button AddButton(string text = null, FontAwesomeIcon icon = FontAwesomeIcon.None, Brush iconColor = null)
        {
            var button = new Button();
            if (iconColor == null)
                iconColor = Brushes.LightGray;

            var sp = new StackPanel();
            sp.Orientation = System.Windows.Controls.Orientation.Horizontal;

            var fa = new FontAwesome.WPF.FontAwesome();
            fa.Icon = icon;
            fa.FontSize = 15;
            fa.FontFamily = new FontFamily(new Uri("pack://application:,,,/"),"./FontAwesome.WPF;component/#FontAwesome");
            fa.Foreground = iconColor;
            fa.Margin = new Thickness(5, 2, 8, 0);

            sp.Children.Add(fa);


            var tb = new TextBlock();
            tb.Text = text;
            tb.Margin = new Thickness(0, 0, 5, 0);

            sp.Children.Add(tb);

            button.Content = sp;
            button.Click += OnButtonClicked;
            
            if (button.MinWidth < 80)
                button.MinWidth = 80;
            PanelButtonContainer.Children.Add(button);
            button.Margin = new Thickness(5,5,5,5);
            button.Height = 40;

            buttonCount++;
            if (button.CommandParameter == null)
                button.CommandParameter = buttonCount;

            button.Tag = this;

            return button;
        }   


        void OnButtonClicked(object sender, RoutedEventArgs ev)
        {
            ButtonResult = sender as Button;

            if (ButtonClickHandler != null)
            {
                if (ButtonClickHandler.Invoke(sender, ev, this))
                    Close();

                return;
            }
            
             Close();
        }
    }
}
