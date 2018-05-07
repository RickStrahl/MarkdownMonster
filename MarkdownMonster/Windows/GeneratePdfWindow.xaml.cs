using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MarkdownMonster.Annotations;
using Microsoft.Win32;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
	/// <summary>
	/// Interaction logic for GeneratePdfWindow.xaml
	/// </summary>
	public partial class GeneratePdfWindow : INotifyPropertyChanged
	{
		public string OutputFile { get; set; }


        public AppModel Model {  get;  }

	    private double initialHeight = 0;



        public HtmlToPdfGeneration PdfGenerator
		{
			get => _pdfGenerator;
			set
			{
				if (Equals(value, _pdfGenerator)) return;
				_pdfGenerator = value;
				OnPropertyChanged();
			}
		}
		private HtmlToPdfGeneration _pdfGenerator = new HtmlToPdfGeneration();


		public GeneratePdfWindow()
		{
			InitializeComponent();
			Loaded += GeneratePdfWindow_Loaded;

		    Model = mmApp.Model;

		    DataContext = this;
            initialHeight = Height;

        }

        private void GeneratePdfWindow_Loaded(object sender, RoutedEventArgs e)
		{
			TextPageSize.ItemsSource = Enum.GetValues(typeof(PdfPageSizes));
			TextPageOrientation.ItemsSource = Enum.GetValues(typeof(PdfPageOrientation));
            TextTitle.Focus();
		}

		private async void ButtonGeneratePdf_Click(object sender, RoutedEventArgs e)
		{
		    WindowUtilities.FixFocus(this,TextMessage);

            PdfGenerator.ExecutionOutputText = string.Empty;
		    TextMessage.Background = Brushes.Transparent;
            TextMessage.Text = string.Empty;
            Height = initialHeight;

		    WindowUtilities.DoEvents();

			var document = mmApp.Model.ActiveDocument;

			if (!SaveFile())
				return;

			ShowStatus("Generating PDF document...");
			SetStatusIcon(FontAwesomeIcon.Spinner, Colors.Goldenrod, true);
			ButtonGeneratePdf.IsEnabled = false;

			WindowUtilities.DoEvents();

		    string htmlFilename = System.IO.Path.ChangeExtension(document.Filename, "html");

			// render the document with template and return only as string (no output yet)
		    document.RenderHtmlToFile(filename: htmlFilename, removeBaseTag: true); //, noFileWrite: true);

            //// strip <base> tag
            //var extracted = StringUtils.ExtractString(html, "<base href=\"", "/>", false, false, true);
            //if (!string.IsNullOrEmpty(extracted))
            //    html = html.Replace(extracted, "");

            //// now write out the file
            //File.WriteAllText(htmlFilename, html);

            bool result = await Task.Run(() =>
		    {
		        bool res = PdfGenerator.GeneratePdfFromHtml(htmlFilename, OutputFile);
		        File.Delete(htmlFilename);
		        return res;
		    });

			ButtonGeneratePdf.IsEnabled = true;


            if (!result)
            {

                if (string.IsNullOrEmpty(PdfGenerator.ExecutionOutputText))
                {
                    TextMessage.Background = Brushes.Firebrick;
                    if (Height < 600)
                        Height = 660;

			        TextMessage.Text = "Failed to create PDF document.\r\n\r\n" + PdfGenerator.ErrorMessage;
			    }


                //MessageBox.Show("Failed to create PDF document.\r\n\r\n" + PdfGenerator.ErrorMessage,
                //	"PDF Generator Error", MessageBoxButton.OK, MessageBoxImage.Warning);

                ShowStatusError("PDF document was not created.");

				return;
			}

			ShowStatus("PDF document created.",mmApp.Configuration.StatusMessageTimeout);
			SetStatusIcon();
		}

	    private void ButtonCopyLastCommandToClipboard_Click(object sender, RoutedEventArgs e)
	    {
	        Clipboard.SetText(PdfGenerator.FullExecutionCommand);
	        ShowStatus("Command line has been copied to the clipboard", mmApp.Configuration.StatusMessageTimeout);
	    }

	    private bool SaveFile()
		{
			var document = mmApp.Model.ActiveDocument;
		    if (document == null)
		    {
		        ShowStatus("No document open. Please open a Markdown Document first to generate a PDF.");
                return false;
		    }

			string initialFolder = null;
			if (!string.IsNullOrEmpty(document.Filename) && document.Filename != "untitled")
				initialFolder = System.IO.Path.GetDirectoryName(document.Filename);

		    string filename = System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(mmApp.Model.ActiveDocument.Filename), "pdf");

			var sd = new SaveFileDialog
			{
				Filter = "PDF files (*.pdf)|*.pdf|All Files (*.*)|*.*",
				FilterIndex = 1,
				Title = "Save output to PDF file",
				InitialDirectory = initialFolder,
                FileName = filename,
				CheckFileExists = false,
				OverwritePrompt = true,
				CheckPathExists = true,
				RestoreDirectory = true
			};
			var result = sd.ShowDialog();
			if (result == null || !result.Value)
			{
				OutputFile = "";
				return false;
			}

			OutputFile = sd.FileName;

			return true;
		}

	    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}


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
	                var window = win as GeneratePdfWindow;
	                window.ShowStatus(null, 0);
	            }, this);
	        }

	        WindowUtilities.DoEvents();
	    }


        /// <summary>
        /// Displays an error message using common defaults
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="timeout">optional timeout</param>
        /// <param name="icon">optional icon (warning)</param>
        /// <param name="color">optional color (firebrick)</param>
        public void ShowStatusError(string message, int timeout = -1, FontAwesomeIcon icon = FontAwesomeIcon.Warning, Color color = default(Color))
	    {
	        if (timeout == -1)
	            timeout = mmApp.Configuration.StatusMessageTimeout;

	        if (color == default(Color))
	            color = Colors.Firebrick;

	        ShowStatus(message, timeout, icon, color);
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
				StatusIcon.SpinDuration = 3;

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

		/// <summary>
		/// Helper routine to show a Metro Dialog. Note this dialog popup is fully async!
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="style"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public async Task<MessageDialogResult> ShowMessageOverlayAsync(string title, string message,
			MessageDialogStyle style = MessageDialogStyle.Affirmative,
			MetroDialogSettings settings = null)
		{
			return await this.ShowMessageAsync(title, message, style, settings);
		}

		#endregion

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
