using System;
using System.IO;
using System.Text;
using MarkdownMonster;
using Microsoft.Win32;
using mmcli.CommandLine;
using Westwind.HtmlPackager;
using Westwind.Utilities;

namespace mmcli
{
    public class HtmltoMarkdownProcessor
    {
        public CommandLineProcessor Processor { get; }

        public HtmlConversionCommandLineParser Arguments { get; }

        public HtmltoMarkdownProcessor(CommandLineProcessor processor,
                                       HtmlConversionCommandLineParser arguments = null)
        {
            Processor = processor;
            if (arguments != null)
                Arguments = arguments;
            else
            {
                Arguments = new HtmlConversionCommandLineParser();
                Arguments.Parse();
            }
        }


        /// <summary>
        ///
        /// mm htmltomarkdown [inputfile] [outputFile] -open
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="openOutputFile"></param>
        public void HtmlToMarkdown()
        {
            Processor.ConsoleHeader();
            string inputFile = Arguments.InputFile;
            string outputFile = Arguments.OutputFile;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
            {
                var fd = new OpenFileDialog
                {
                    DefaultExt = ".html",
                    Filter = "HTML files (*.html, *.htm)|*.html;*.htm|" +
                             "All files (*.*)|*.*",
                    CheckFileExists = true,
                    RestoreDirectory = true,
                    Title = "Open HTML File",
                    InitialDirectory = Environment.CurrentDirectory
                };
                var res = fd.ShowDialog();
                if (res == null)
                    return;
                inputFile = fd.FileName;
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var fd = new SaveFileDialog
                {
                    DefaultExt = ".md",
                    Filter = "Markdown files (*.md,*.markdown,*.mdcrypt)|*.md;*.markdown;*.mdcrypt|" +
                             "All files (*.*)|*.*",
                    CheckFileExists = false,
                    RestoreDirectory = true,
                    Title = "Save as Markdown File",
                    InitialDirectory = Path.GetDirectoryName(inputFile),
                    FileName = Path.ChangeExtension(Path.GetFileName(inputFile), "md")
                };
                var res = fd.ShowDialog();
                if (res == null)
                    return;
                outputFile = fd.FileName;
            }

            
            string md;
            try
            {
                var html = File.ReadAllText(inputFile);
                md = MarkdownUtilities.HtmlToMarkdown(html, true);
            }
            catch 
            {
                ColorConsole.WriteError("Failed: Couldn't read input file.");
                Processor.ConsoleFooter();
                return;
            }


            if (!string.IsNullOrEmpty(outputFile))
            {
                try
                {
                    File.WriteAllText(outputFile, md);
                }
                catch 
                {
                    ColorConsole.WriteError("Failed: Couldn't write output file.");
                    Processor.ConsoleFooter();
                    return;
                }

                if (Arguments.OpenOutputFile)
                    ShellUtils.ExecuteProcess("markdownmonster.exe", $"'{outputFile}'");


                ColorConsole.WriteSuccess($"Created Markdown file: {outputFile}");

                Processor.ConsoleFooter();
            }
        }

        /// <summary>
        ///
        /// mm markdowntohtml <inputfile> <outputFile> <rendermode> -open
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="openOutputFile"></param>
        /// <param name="fileMode">html,packagedhtml,zip</param>
        public void MarkdownToHtml()
        {
            Processor.ConsoleHeader();

            string inputFile = Arguments.InputFile;
            string outputFile = Arguments.OutputFile;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
            {
                var fd = new OpenFileDialog
                {
                    DefaultExt = ".md",
                    Filter = "Markdown files (*.md,*.markdown)|*.md;*.markdown|" +
                             "All files (*.*)|*.*",
                    CheckFileExists = true,
                    RestoreDirectory = true,
                    Title = "Open Markdown File",
                    InitialDirectory = Environment.CurrentDirectory
                };
                var res = fd.ShowDialog();
                if (res == null)
                    return;
                inputFile = fd.FileName;
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var fd = new SaveFileDialog
                {
                    DefaultExt = ".html",
                    Filter = "HTML files (*.html,*.htm)|*.html;*.htm|" +
                             "All files (*.*)|*.*",
                    CheckFileExists = false,
                    RestoreDirectory = true,
                    Title = "Save as HTML File",
                    InitialDirectory = Path.GetDirectoryName(inputFile),
                    FileName = Path.ChangeExtension(Path.GetFileName(inputFile), "html")
                };
                var res = fd.ShowDialog();
                if (res == null)
                    return;
                outputFile = fd.FileName;
            }

            string html;
            var doc = new MarkdownDocument();
            if (!string.IsNullOrEmpty(Arguments.Theme))
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

            try
            {
                string renderMode = Arguments.HtmlRenderMode?.ToLower();

                if (renderMode == "fragment" || renderMode == "raw")
                {
                    try
                    {
                        var parser = new MarkdownParserMarkdig();
                        html = parser.Parse(doc.CurrentText);

                        File.WriteAllText(outputFile, html, new UTF8Encoding(false));
                    }
                    catch
                    {
                        ColorConsole.WriteError("Failed: Couldn't convert Markdown document or generate output file.");
                        Processor.ConsoleFooter();
                        return;
                    }
                }
                else
                {
                    if (doc.RenderHtmlToFile(filename: outputFile) == null)
                        throw new AccessViolationException();
                }
                
                if (renderMode == "packagedhtml")
                {
                    var packager = new Westwind.HtmlPackager.HtmlPackager();
                    string outputHtml = packager.PackageHtml(outputFile);
                    try
                    {
                        File.WriteAllText(outputFile, outputHtml);
                    }
                    catch
                    {
                        ColorConsole.WriteError("Failed: Couldn't write output file.");
                        Processor.ConsoleFooter();
                        return;
                    }
                }
                else if (renderMode == "htmlfiles")
                {
                    var packager = new Westwind.HtmlPackager.HtmlPackager();
                    if (!packager.PackageHtmlToFolder(outputFile, outputFile))
                        ColorConsole.WriteLine("Failed: Create output folder.",ConsoleColor.Red);
                }
                else if (renderMode == "zip")
                {
                    var packager = new Westwind.HtmlPackager.HtmlPackager();
                    if (!packager.PackageHtmlToZipFile(Path.ChangeExtension(outputFile,"html"), Path.ChangeExtension(outputFile,"zip")))
                        ColorConsole.WriteError("Failed: Couldn't create Packaged HTML Zip files.");

                    try
                    {
                        File.Delete(Path.ChangeExtension(outputFile, "html"));
                    }catch{}
                }

            }

            catch
            {
                ColorConsole.WriteError("Failed: Couldn't write output file.");
                Processor.ConsoleFooter();
                return;
            }

            if (Arguments.OpenOutputFile)
                ShellUtils.GoUrl($"{outputFile}");

            ColorConsole.WriteSuccess($"Created file: {outputFile}");
            ColorConsole.WriteSuccess($" Output Mode: {Arguments.HtmlRenderMode}");

            Processor.ConsoleFooter();
        }
    }
}
