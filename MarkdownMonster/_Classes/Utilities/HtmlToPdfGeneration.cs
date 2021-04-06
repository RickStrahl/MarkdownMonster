using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MarkdownMonster.Annotations;
using MarkdownMonster.Configuration;
using Westwind.Utilities;

namespace MarkdownMonster
{

    /// <summary>
    /// Class wrapper around the WkPdfToHtml Engine
    /// https://wkhtmltopdf.org/usage/wkhtmltopdf.txt
    /// </summary>
    public class HtmlToPdfGeneration : INotifyPropertyChanged
    {

        /// <summary>
        /// The document title. If null or empty the first 
        /// header is used which is the default..		
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///  The path to the wkpdftohtml executable (optional)
        /// </summary>
        public string ExecutionPath { get; set; }

        /// <summary>
        /// Documents paper size Letter, 
        /// </summary>
        public PdfPageSizes PageSize { get; set; }

        public PdfPageOrientation Orientation { get; set; }

        #region headers and footers

        public string FooterText { get; set; }

        public string FooterHtmlUrl { get; set; }


        public bool ShowFooterLine { get; set; }



        public int FooterFontSize { get; set; }

        public bool ShowFooterPageNumbers { get; set; }

        public string HeaderHtmlUrl { get; set; }

        public string HeaderLeft { get; set; }

        public string HeaderRight { get; set; }

        #endregion

        public int ImageDpi { get; set; } = 300;

        public bool GenerateTableOfContents { get; set; } = true;

        public bool DisplayPdfAfterGeneration { get; set; } = true;




        public string ExecutionOutputText
        {
            get { return _executionOutputText; }
            set
            {
                if (value == _executionOutputText) return;
                _executionOutputText = value;
                OnPropertyChanged();
            }
        }
        private string _executionOutputText;


        public string FullExecutionCommand
        {
            get { return _fullExecutionCommand; }
            set
            {
                if (value == _fullExecutionCommand) return;
                _fullExecutionCommand = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasExecutionCommandLine));
            }
        }
        private string _fullExecutionCommand;



        public bool HasExecutionCommandLine
        {
            get { return !string.IsNullOrEmpty(FullExecutionCommand); }
        }
        public bool HasExecutionOutput
        {
            get { return !string.IsNullOrEmpty(FullExecutionCommand); }
        }

        public PdfPageMargins Margins { get; set; } = new PdfPageMargins();


        /// <summary>
        /// High level method that converts HTML to PDF.
        /// </summary>
        /// <param name="sourceHtmlFileOrUri"></param>
        /// <param name="outputPdfFile"></param>
        /// <returns></returns>
        public bool GeneratePdfFromHtml(string sourceHtmlFileOrUri, string outputPdfFile)
        {
            StringBuilder sb = new StringBuilder();

            // options
            sb.Append($"--image-dpi {ImageDpi} ");

            sb.Append($"--page-size {PageSize} ");
            sb.Append($"--orientation {Orientation} ");
            sb.Append("--enable-internal-links ");
            sb.Append("--keep-relative-links ");
            sb.Append("--print-media-type ");
            sb.Append("--encoding \"UTF-8\" ");

            sb.Append($"--footer-font-size {FooterFontSize} ");
            if (ShowFooterLine)
                sb.Append("--footer-line ");

            // precedence: Html, text, page numbers
            if (!string.IsNullOrEmpty(FooterHtmlUrl))
            {
                sb.Append($"--footer-html \"{FooterHtmlUrl}\" ");
            }
            else if (!string.IsNullOrEmpty(FooterText))
            {
                sb.Append($"--footer-right \"{FooterText}\" ");
            }
            else if (ShowFooterPageNumbers)
            {
                sb.Append("--footer-right \"[page] of [topage]\" ");
            }

            if (!string.IsNullOrEmpty(Title))
            {
                sb.Append($"--header-left \"{Title}\" ");
                //sb.Append($"--header-right \"[subsection]\" ");
                sb.Append("--header-spacing 3 ");
            }

            if (Margins.MarginLeft > 0)
                sb.Append($"--margin-left {Margins.MarginLeft} ");
            if (Margins.MarginRight > 0)
                sb.Append($"--margin-right {Margins.MarginRight} ");
            if (Margins.MarginTop > 0)
                sb.Append($"--margin-top {Margins.MarginTop} ");
            if (Margins.MarginBottom > 0)
                sb.Append($"--margin-bottom {Margins.MarginBottom} ");

            if (!GenerateTableOfContents)
            {
                sb.Append($"--no-outline ");
                sb.Append($"--outline-depth 3 ");
            }

            // in and out files
            sb.Append($"\"{sourceHtmlFileOrUri}\" \"{outputPdfFile}\" ");

            string exe = "wkhtmltopdf.exe";
            if (!string.IsNullOrEmpty(ExecutionPath))
                exe = Path.Combine(ExecutionPath, exe);

            FullExecutionCommand = exe + " " + sb;

            try
            {
                File.Delete(outputPdfFile);
            }
            catch
            {
                SetError("Please close the output file first:\r\n" + outputPdfFile);
                return false;
            }


            int exitCode = ExecuteWkProcess(sb.ToString());

            if (exitCode != 0)
            {
                SetError("Unable to create PDF document. Exit code: " + exitCode + " " + ErrorMessage + "\r\n" + sb);
                return false;
            }

            if (exitCode == 0 && DisplayPdfAfterGeneration)
            {
                try
                {
                    Process.Start(outputPdfFile);
                }
                catch { }
            }

            return true;
        }


        public int ExecuteWkProcess(string parms, bool copyCommandLineToClipboard = false)
        {
            string exe = "wkhtmltopdf.exe";
            if (!string.IsNullOrEmpty(ExecutionPath))
                exe = Path.Combine(ExecutionPath, exe);

            StringBuilder output = new StringBuilder();

            FullExecutionCommand = "\"" + exe + "\" " + parms;
            if (copyCommandLineToClipboard)
                ClipboardHelper.SetText(FullExecutionCommand);

            using (Process process = new Process())
            {
                process.StartInfo.FileName = exe;
                process.StartInfo.Arguments = parms;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;

                void OnProcessOnOutputDataReceived(object o, DataReceivedEventArgs e) => output.AppendLine(e.Data);

                process.OutputDataReceived += OnProcessOnOutputDataReceived;
                process.ErrorDataReceived += OnProcessOnOutputDataReceived;

                try
                {
                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    bool result = process.WaitForExit(180000);

                    if (!result)
                        return 1473;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return -1;
                }
                finally
                {
                    process.OutputDataReceived -= OnProcessOnOutputDataReceived;
                    process.ErrorDataReceived -= OnProcessOnOutputDataReceived;
                }

                ExecutionOutputText = output.ToString();

                return process.ExitCode;

            }
        }



        #region Error Message
        public string ErrorMessage { get; set; }

        protected void SetError()
        {
            SetError("CLEAR");
        }

        protected void SetError(string message)
        {
            if (message == null || message == "CLEAR")
            {
                ErrorMessage = string.Empty;
                return;
            }
            ErrorMessage += message;
        }

        protected void SetError(Exception ex, bool checkInner = false)
        {
            if (ex == null)
                ErrorMessage = string.Empty;
            else
            {
                Exception e = ex;
                if (checkInner)
                    e = e.GetBaseException();

                ErrorMessage = e.Message;
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets the configuration settings from the MM configuration
        /// object.
        /// </summary>
        /// <param name="config"></param>
        public void SetFromConfiguration(PdfOutputConfiguration config)
        {
            DataUtils.CopyObjectData(config, this);
        }

        /// <summary>
        /// Updates the MM PDF output Configuration object from current values.
        /// Updates the config object with the values from the dialog.
        /// </summary>
        public void SetConfiguration(PdfOutputConfiguration config)
        {
            DataUtils.CopyObjectData(this, config);
        }
    }

    public class PdfPageMargins
    {
        public int MarginLeft { get; set; }
        public int MarginRight { get; set; }
        public int MarginTop { get; set; }
        public int MarginBottom { get; set; }
    }


    public enum PdfPageSizes
    {
        Letter,
        Legal,
        A4,
        B4,
        A0,
        A1,
        A2,
        A3,
        A5,
        A6,
        A7,
        A8,
        A9,
        B0,
        B1,
        B2,
        B3,
        B5,
        B6,
        B7,
        B8,
        B9,
        B10,
        Comm10E,
        DLE,
        Executive,
        Folio,
        Ledger,
        Tabloid
    }

    public enum PdfPageOrientation
    {
        Portrait,
        Landscape
    }

}
