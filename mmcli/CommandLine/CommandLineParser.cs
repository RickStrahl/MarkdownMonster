using System;
using System.Collections.Generic;

namespace mmcli.CommandLine
{
    /// <summary>
    /// Basic Command Line Parser class that can deal with simple
    /// switch based command line arguments
    ///
    /// supports: FirstParm (first commandline argume
    /// -pString or -p"String"
    /// -f     switch/flag parameters
    /// </summary>
    public abstract class CommandLineParser
    {
        /// <summary>
        /// The Command Line arguments string array.
        /// Note unlike Environment.GetCommandLineArguments() this
        /// holds only the arguments and not the executable
        /// </summary>
        public string[] Arguments { get; set; }

        /// <summary>
        /// The full command line including the executable
        /// </summary>
        public string CommandLine { get; set; }

        /// <summary>
        /// The first argument (if any). Useful for a
        /// command/action parameter
        /// </summary>
        public string FirstParameter { get; set; }

        public CommandLineParser(string[] args = null, string cmdLine = null)
        {
            if (string.IsNullOrEmpty(cmdLine))
                CommandLine = Environment.CommandLine;
            else
                CommandLine = cmdLine;

            if (args == null)
                args = Environment.GetCommandLineArgs();

            List<string> argList = new List<string>(args);

            if (argList.Count > 1)
            {
                FirstParameter = argList[1];

                // argument array contains startup exe - remove
                argList.RemoveAt(0);
                Arguments = argList.ToArray();
            }
            else
            {
                FirstParameter = string.Empty;
                // empty array - not null to match args array
                Arguments = new string[0];
            }
        }

        /// <summary>
        /// Override to provide parse switches\parameter
        /// into object structure
        /// </summary>
        public abstract void Parse();


        /// <summary>
        /// Parses a string Parameter switch in the format of:
        ///
        /// -p"c:\temp files\somefile.txt"
        /// -pc:\somefile.txt
        ///
        /// Note no spaces are allowed between swich and value.
        /// </summary>
        /// <param name="parm">parameter switch key</param>
        /// <param name="nonMatchingValue">Value to return if no match is found</param>
        /// <returns>Match or non-matching value</returns>
        protected string ParseStringParameterSwitch(string parm, string nonMatchingValue = null)
        {
            int at = CommandLine.IndexOf(parm, 0, StringComparison.OrdinalIgnoreCase);

            if (at > -1)
            {
                string rest = CommandLine.Substring(at + parm.Length);

                // next character needs to be a space
                if (!rest.StartsWith(" "))
                    return nonMatchingValue;

				rest = rest.Trim();

                if (rest.StartsWith("\""))
                {
                    // read to end quote
                    at = rest.IndexOf('"', 2);
                    if (at == -1)
                        return CommandLine;

                    return rest.Substring(1, at - 1);
                }
                else
                {
                    // read to next space
                    at = (rest + " ").IndexOf(' ');
                    string stringParm = rest.Substring(0, at);
                    if (!string.IsNullOrEmpty(stringParm))
                        stringParm = stringParm.Trim('"', '\'');

                    return stringParm;
                }
            }

            return nonMatchingValue;
        }

        protected int ParseIntParameterSwitch(string parm, int failedValue = -1)
        {
            string val = ParseStringParameterSwitch(parm);
            int res = failedValue;
            if (!int.TryParse(val, out res))
                res = failedValue;

            return res;
        }

        protected bool ParseParameterSwitch(string parm)
        {
            int at = CommandLine.IndexOf(parm, 0, StringComparison.OrdinalIgnoreCase);

            if (at > -1)
                return true;

            return false;
        }
    }
}
