using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MarkdownMonster.Utilities;
using Microsoft.Win32;
using Westwind.HtmlPackager;
using Westwind.Utilities;

namespace MarkdownMonster
{
    /// <summary>
    /// This class has handles the 'console' like command line
    /// operations for Markdown Monster.
    /// </summary>
    public class MainAppCommandLineProcessor
    {
        public static void HandleCommandLineArguments(App App)
        {
            var arg0 = App.CommandArgs[0].ToLower().TrimStart('-');
            if (App.CommandArgs[0] == "-")
                arg0 = "-";

            if (Environment.CommandLine.Contains("-presentation"))
                App.StartInPresentationMode = true;

            if (Environment.CommandLine.Contains("-newwindow", StringComparison.InvariantCultureIgnoreCase))
                App.ForceNewWindow = true;

            if (Environment.CommandLine.Contains("-nosplash", StringComparison.InvariantCultureIgnoreCase))
                App.NoSplash = true;

            if (Environment.CommandLine.Contains("-delay", StringComparison.InvariantCultureIgnoreCase))
            {
                for (int i = 0; i < 150; i++)
                {
                    Thread.Sleep(10);
                }
            }

            // Open an empty document with base64 decoded text
            if (arg0 == "base64text" && App.CommandArgs.Length > 1)
            {
                // Set Startup text which will be picked up by the command line file opener
                App.CommandArgs[0] = CommandLineTextEncoder.CreateEncodedCommandLineFilename(
                    App.CommandArgs[1],
                    CommandLineTextEncodingFormats.PreEncodedBase64);

                // trim other args
                App.CommandArgs = App.CommandArgs.Take(1).ToArray();
            }

            if (arg0 == "stdin")
            {
                string stdin = null;
                if (Console.IsInputRedirected)
                {
                    using (var stream = Console.OpenStandardInput())
                    {
                        byte[] buffer = new byte[1000]; // Use whatever size you want
                        var builder = new StringBuilder();
                        int read = -1;
                        while (true)
                        {
                            var gotInput = new AutoResetEvent(false);
                            var inputThread = new Thread(() =>
                            {
                                try
                                {
                                    read = stream.Read(buffer, 0, buffer.Length);
                                    gotInput.Set();
                                }
                                catch (ThreadAbortException)
                                {
                                    Thread.ResetAbort();
                                }
                            }) {IsBackground = true};

                            inputThread.Start();

                            // Timeout expired?
                            if (!gotInput.WaitOne(100))
                            {
                                inputThread.Abort();
                                break;
                            }

                            // End of stream?
                            if (read == 0)
                            {
                                stdin = builder.ToString();
                                break;
                            }

                            // Got data
                            builder.Append(Console.InputEncoding.GetString(buffer, 0, read));
                        }


                        App.CommandArgs[0] =
                            CommandLineTextEncoder.CreateEncodedCommandLineFilename(builder.ToString());


                    }
                }
            }
        }



        /// <summary>
        /// Uninstall registry and configuration settings
        /// </summary>
        private void UninstallSettings()
        {
            mmFileUtils.EnsureBrowserEmulationEnabled("MarkdownMonster.exe", uninstall: true);
            mmFileUtils.EnsureSystemPath(uninstall: true);
            mmFileUtils.EnsureAssociations(uninstall: true);

            MessageBox.Show("Permanent Markdown Monster settings have been uninstalled from the registry.", "Markdown Monster Uninstall Settings");

            App._noStart = true;
            Environment.Exit(0);
        }
    }

}
