using System;
using System.IO;
using System.Reflection;

namespace mmcli.CommandLine
{
    public class HtmlConversionCommandLineParser : CommandLineParser
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


        /// <summary>
        /// When generating HTML output determines which theme is used.
        /// If not set the currently selected configuration theme is used
        /// </summary>
        public string Theme {get; set;}


        public HtmlConversionCommandLineParser(string[] args = null, string cmdLine = null)
            : base(args, cmdLine)
        {
        }

        public override void Parse()
        {
            Action = (ParseStringParameterSwitch("--action") ?? Arguments[0])?.ToLower();

            InputFile = ParseStringParameterSwitch("--input") ?? ParseStringParameterSwitch("-i");
            OutputFile = ParseStringParameterSwitch("--output") ?? ParseStringParameterSwitch("-o");
            HtmlRenderMode = ParseStringParameterSwitch("-rm") ?? ParseStringParameterSwitch("--rendermode");
            OpenOutputFile = ParseParameterSwitch("-open");
            Theme = ParseStringParameterSwitch("--theme");

            if (!string.IsNullOrEmpty(InputFile))
                InputFile = Path.GetFullPath(InputFile);
            if(!string.IsNullOrEmpty(OutputFile))
                InputFile = Path.GetFullPath(OutputFile);
        }

    }
}
