using System;
using System.IO;
using MarkdownMonster;
using Microsoft.Win32;
using Westwind.HtmlPackager;
using Westwind.Utilities;

namespace mmcli
{
    public class HtmltoMarkdownCommandline
    {
        public CommandLineProcessor Processor { get; }

        public HtmltoMarkdownCommandline(CommandLineProcessor processor)
        {
            Processor = processor;
        }

        /// <summary>
        ///
        /// mm htmltomarkdown [inputfile] [outputFile] -open
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="openOutputFile"></param>
        public void HtmlToMarkdown(string inputFile = null, string outputFile= null, bool openOutputFile = false)
        {
            Processor.ConsoleHeader();

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
            {
                var fd = new OpenFileDialog
                {
                    DefaultExt = ".md",
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

            if (string.IsNullOrEmpty(outputFile) || !File.Exists(outputFile))
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

            string html = null;
            string md = null;
            try
            {
                html = File.ReadAllText(inputFile);
                md = MarkdownUtilities.HtmlToMarkdown(html, true);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed: Couldn't read input file.");
                Processor.ConsoleFooter();
                return;
            }


            if (!string.IsNullOrEmpty(outputFile))
            {
                try
                {
                    File.WriteAllText(outputFile, md);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed: Couldn't write output file.");
                    Processor.ConsoleFooter();
                    return;
                }

                if (openOutputFile)
                    ShellUtils.ExecuteProcess("markdownmonster.exe", $"'{outputFile}'");

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
        public void MarkdownToHtml(string inputFile = null, string outputFile = null, bool openOutputFile = false, string renderMode = "html")
        {
            Processor.ConsoleHeader();

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
            {
                var fd = new OpenFileDialog
                {
                    DefaultExt = ".html",
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
                    DefaultExt = ".md",
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

            string html = null;
            string md = null;
            var doc = new MarkdownDocument();

            try
            {
                if (!doc.Load(inputFile))
                    throw new AccessViolationException();
            }
            catch
            {
                ConsoleHelper.WriteLine("Failed: Couldn't read input file.",ConsoleColor.Red);
                Processor.ConsoleFooter();
                return;
            }

            try
            {
                if (doc.RenderHtmlToFile(filename: outputFile) == null)
                    throw new AccessViolationException();

                if (renderMode == "packagedhtml")
                {
                    var packager = new HtmlPackager();
                    string outputHtml = packager.PackageHtml(outputFile);
                    try
                    {
                        File.WriteAllText(outputFile, outputHtml);
                    }
                    catch
                    {
                        ConsoleHelper.WriteLine("Failed: Couldn't write output file.",ConsoleColor.Red);
                        Processor.ConsoleFooter();
                        return;
                    }
                }
                else if (renderMode == "htmlfiles")
                {
                    var packager = new HtmlPackager();
                    if (!packager.PackageHtmlToFolder(outputFile, outputFile))
                        ConsoleHelper.WriteLine("Failed: Create output folder.",ConsoleColor.Red);
                }
                else if (renderMode == "zip")
                {
                    var packager = new HtmlPackager();
                    if (!packager.PackageHtmlToZipFile(Path.ChangeExtension(outputFile,"html"), Path.ChangeExtension(outputFile,"zip")))
                        ConsoleHelper.WriteLine("Failed: Couldn't create Packaged HTML Zip files.",ConsoleColor.Red);

                    try
                    {
                        File.Delete(Path.ChangeExtension(outputFile, "html"));
                    }catch{}
                }

            }

            catch (Exception ex)
            {
                ConsoleHelper.WriteLine("Failed: Couldn't write output file.",ConsoleColor.Red);
                Processor.ConsoleFooter();
                return;
            }

            if (openOutputFile)
                ShellUtils.GoUrl($"{outputFile}");

            ConsoleHelper.WriteLine($"Created file: {outputFile}", ConsoleColor.Green);
            ConsoleHelper.WriteLine($" Output Mode: {renderMode}", ConsoleColor.Green);

            Processor.ConsoleFooter();
        }
    }
}
