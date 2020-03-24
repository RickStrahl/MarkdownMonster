using System;
using MarkdownMonster;

namespace mmcli.CommandLine
{
    public class MarkdownToPdfCommandLineParser : CommandLineParser
    {
        /// <summary>
        /// The operation performed
        /// </summary>
        public string Action {get; set;}


        /// <summary>
        /// Input file for processing
        /// </summary>
        public string InputFile { get; set; }

        /// <summary>
        /// Output file for processing
        /// </summary>
        public string OutputFile { get; set; }
        
        public bool OpenOutputFile {get; set;}
        


        /// <summary>
        /// Determines how HTML output is rendered
        ///
        /// html, packagedhtml, zip
        /// </summary>
        public string HtmlRenderMode { get; set; }

        public string Theme { get; set; }


        public PdfPageSizes PageSize { get; set; } = PdfPageSizes.Letter;

        public PdfPageOrientation Orientation { get; set; } = PdfPageOrientation.Portrait;


        /// <summary>
        /// Determines whether verbose messages will be written
        /// as each file is processed.
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// When true displays the result HTML in the default
        /// browser.        
        /// </summary>
        public bool DisplayHtml { get; set; }
        
        public MarkdownToPdfCommandLineParser(string[] args = null, string cmdLine = null)
            : base(args, cmdLine)
        {
        }

        public override void Parse()
        {
            Action = (ParseStringParameterSwitch("--action") ?? Args[0])?.ToLower();

            InputFile = ParseStringParameterSwitch("--input") ?? ParseStringParameterSwitch("-i");
            OutputFile = ParseStringParameterSwitch("-output") ?? ParseStringParameterSwitch("-o");
            OpenOutputFile = ParseParameterSwitch("-open");
            Theme = ParseStringParameterSwitch("--theme");
            string workValue = ParseStringParameterSwitch("--page-size");
            if (!string.IsNullOrEmpty(workValue))
            {
                if (Enum.TryParse<PdfPageSizes>(workValue, out PdfPageSizes pgSize))
                    PageSize = pgSize;
            }

            workValue = ParseStringParameterSwitch("--orientation");
            if (!string.IsNullOrEmpty(workValue))
            {
                if (Enum.TryParse<PdfPageOrientation>(workValue, out PdfPageOrientation orientation))
                    Orientation = orientation;
            }

        }

    }
}
