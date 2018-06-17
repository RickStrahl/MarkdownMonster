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

	    public StatusBarHelper StatusBar { get; set; }

        public GeneratePdfWindow()
		{
			InitializeComponent();
			Loaded += GeneratePdfWindow_Loaded;

		    Model = mmApp.Model;

		    DataContext = this;
            initialHeight = Height;

		    StatusBar = new StatusBarHelper(StatusText, StatusIcon);
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

			StatusBar.ShowStatusProgress("Generating PDF document...");			
			ButtonGeneratePdf.IsEnabled = false;

			WindowUtilities.DoEvents();

		    string htmlFilename = System.IO.Path.ChangeExtension(document.Filename, "html");

			// render the document with template and return only as string (no output yet)
		    document.RenderHtmlToFile(filename: htmlFilename, removeBaseTag: true); //, noFileWrite: true);

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

                StatusBar.ShowStatusError("PDF document was not created.");

				return;
			}

			StatusBar.ShowStatusSuccess("PDF document created.",mmApp.Configuration.StatusMessageTimeout);
			
		}

	    private void ButtonCopyLastCommandToClipboard_Click(object sender, RoutedEventArgs e)
	    {
	        if(ClipboardHelper.SetText(PdfGenerator.FullExecutionCommand))
	            StatusBar.ShowStatusSuccess("Command line has been copied to the clipboard");
	    }

	    private bool SaveFile()
		{
			var document = mmApp.Model.ActiveDocument;
		    if (document == null)
		    {
		        StatusBar.ShowStatusError("No document open. Please open a Markdown Document first to generate a PDF.");
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
