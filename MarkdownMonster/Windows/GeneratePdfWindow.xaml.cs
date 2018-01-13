using System;
using System.Collections.Generic;
using System.ComponentModel;
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

			// render the document to the normal output location
			document.RenderHtmlToFile();

            
		    bool result = await Task.Run(() =>
		    {
		        PdfGenerator.DisplayPdfAfterGeneration = true;
		        bool res = PdfGenerator.GeneratePdfFromHtml(document.HtmlRenderFilename, OutputFile);
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

                ShowStatus("PDF document was not created.", 6000);
				SetStatusIcon(FontAwesomeIcon.Warning,Colors.Firebrick);
				return;
			}

			ShowStatus("PDF document created.",6000);
			SetStatusIcon();
		}

	    private void ButtonCopyLastCommandToClipboard_Click(object sender, RoutedEventArgs e)
	    {
	        Clipboard.SetText(PdfGenerator.FullExecutionCommand);
	        ShowStatus("Command line has been copied to the clipboard", 6000);
	    }

	    private bool SaveFile()
		{
			var document = mmApp.Model.ActiveDocument;

			string initialFolder = null;
			if (!string.IsNullOrEmpty(document.Filename) && document.Filename != "untitled")
				initialFolder = System.IO.Path.GetDirectoryName(document.Filename);


			var sd = new SaveFileDialog
			{
				Filter = "PDF files (*.pdf)|*.pdf|All Files (*.*)|*.*",
				FilterIndex = 1,
				Title = "Save output to PDF file",
				InitialDirectory = initialFolder,
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

		public void ShowStatus(string message = null, int milliSeconds = 0)
		{
			if (message == null)
			{
				message = "Ready";
				SetStatusIcon();
			}

			StatusText.Text = message;

			if (milliSeconds > 0)
			{
				Dispatcher.DelayWithPriority(milliSeconds, (win) =>
				{
					ShowStatus(null, 0);
					SetStatusIcon();
				}, this);
			}
			WindowUtilities.DoEvents();
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
