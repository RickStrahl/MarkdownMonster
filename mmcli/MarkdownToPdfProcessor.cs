using System;
using System.IO;
using System.Windows.Forms;
using MarkdownMonster;
using Microsoft.Win32;
using mmcli.CommandLine;

namespace mmcli
{
    public class MarkdownToPdfProcessor
    {
        public CommandLineProcessor Processor { get; }

        public MarkdownToPdfCommandLineParser Arguments {get; }

        public MarkdownToPdfProcessor(CommandLineProcessor processor)
        {
            Processor = processor;
            Arguments = new MarkdownToPdfCommandLineParser();
            Arguments.Parse();
        }

        /// <summary>
        /// mm markdowntopdf [inputfile] [outputFile] -open
        /// </summary>
        public void MarkdownToPdf()
        {
            Processor.ConsoleHeader();

            string inputFile = Arguments.InputFile;
            string outputFile = Arguments.OutputFile;
            
            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
            {
                var fd = new System.Windows.Forms.OpenFileDialog
                {
                    DefaultExt = ".md",
                    Filter = "HTML files (*.md, *.markdown, *.mdcrypt)|*.md;*.markdown,*.mdcrypt|" +
                             "All files (*.*)|*.*",
                    CheckFileExists = true,
                    RestoreDirectory = true,
                    Title = "Open Markdown File",
                    InitialDirectory = Environment.CurrentDirectory
                };
                var res = fd.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.Cancel || res == System.Windows.Forms.DialogResult.Abort)
                    return;
                inputFile = fd.FileName;
            }

            if (string.IsNullOrEmpty(outputFile) || !File.Exists(outputFile))
            {
                var fd = new System.Windows.Forms.SaveFileDialog
                {
                    DefaultExt = ".pdf",
                    Filter = "Pdf files (*.pdf)|*.pdf|" +
                             "All files (*.*)|*.*",
                    CheckFileExists = false,
                    RestoreDirectory = true,
                    Title = "Save as Pdf File",
                    InitialDirectory = Path.GetDirectoryName(inputFile),
                    FileName = Path.ChangeExtension(Path.GetFileName(inputFile), "pdf")
                };
                var res = fd.ShowDialog();
                if (res == DialogResult.Cancel || res == DialogResult.Abort)
                    return;
                outputFile = fd.FileName;
            }

            var doc = new MarkdownDocument();
            if(!string.IsNullOrEmpty(Arguments.Theme))
                mmApp.Configuration.PreviewTheme = Arguments.Theme;

            try
            {
                if (!doc.Load(inputFile))
                    throw new AccessViolationException();
            }
            catch
            {
                ColorConsole.WriteError("Failed: Couldn't read input file.");
                Processor.ConsoleFooter();
                return;
            }

            string htmlFilename = System.IO.Path.ChangeExtension(doc.Filename, "html");
            var pdfGenerator = new HtmlToPdfGeneration();
            pdfGenerator.PageSize = Arguments.PageSize;
            pdfGenerator.Orientation = Arguments.Orientation;
            pdfGenerator.DisplayPdfAfterGeneration = Arguments.OpenOutputFile;

            try
            {
                // render the document with template and return only as string (no output yet)
                doc.RenderHtmlToFile(filename: htmlFilename, removeBaseTag: true);

                if (!pdfGenerator.GeneratePdfFromHtml(htmlFilename, outputFile))
                    throw new InvalidOperationException(pdfGenerator.ErrorMessage);
            }
            catch (Exception ex)
            {
                ColorConsole.WriteError("Failed: PDF output generation failed: " + ex.Message);
                Processor.ConsoleFooter();
                return;
            }
            finally
            {
                File.Delete(htmlFilename);
            }

            ColorConsole.WriteSuccess("PDF file generated: " + outputFile);
            Processor.ConsoleFooter();
        }
    }
}
