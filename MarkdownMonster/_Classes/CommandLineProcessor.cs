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
using Westwind.Utilities;

namespace MarkdownMonster
{
    /// <summary>
    /// This class has handles the 'console' like command line
    /// operations for Markdown Monster.
    /// </summary>
    public class CommandLineProcessor
    {
        [DllImport("Kernel32.dll")]
        private static extern bool AttachConsole(int processId);

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();


        private App App { get; }


        public CommandLineProcessor(App app)
        {
            App = app;
        }

        public void HandleCommandLineArguments()
        {
            var arg0 = App.CommandArgs[0].ToLower().TrimStart('-');
            if (App.CommandArgs[0] == "-")
                arg0 = "-";

            if (Environment.CommandLine.Contains("-presentation"))
                App.StartInPresentationMode = true;

            if (Environment.CommandLine.Contains("-newwindow", StringComparison.InvariantCultureIgnoreCase))
                App.ForceNewWindow = true;

            if (Environment.CommandLine.Contains("-nosplash",StringComparison.InvariantCultureIgnoreCase))
                App.NoSplash = true;

            switch (arg0)
            {
                case "version":
                    // just display the header
                    ConsoleHeader();
                    ConsoleFooter();
                    break;
                case "uninstall":
                    App._noStart = true;
                    UninstallSettings();

                    ConsoleHeader();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Markdown Monster Machine Wide Settings uninstalled.");
                    ConsoleFooter();

                    break;
                case "reset":
                    // load old config and backup
                    mmApp.Configuration.Backup();
                    mmApp.Configuration.Reset(); // forces exit

                    ConsoleHeader();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Markdown Monster Settings reset to defaults.");
                    ConsoleFooter();

                    break;
                case "setportable":
                    ConsoleHeader();

                    // Note: Startup logic to handle portable startup is in AppConfiguration::FindCommonFolder
                    try
                    {
                        string portableSettingsFolder = Path.Combine(App.InitialStartDirectory, "PortableSettings");
                        bool exists = Directory.Exists(portableSettingsFolder);
                        string oldCommonFolder = mmApp.Configuration.CommonFolder;

                        File.WriteAllText("_IsPortable",
                            @"forces the settings to be read from .\PortableSettings rather than %appdata%");

                        if (!exists &&
                            Directory.Exists(oldCommonFolder) &&
                            MessageBox.Show(
                                "Portable mode set. Do you want to copy settings from:\r\n\r\n" +
                                oldCommonFolder + "\r\n\r\nto the PortableSettings folder?",
                                "Markdown MonsterPortable Mode",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            FileUtils.CopyDirectory(oldCommonFolder,
                                portableSettingsFolder, deepCopy: true);

                            mmApp.Configuration.CommonFolder = portableSettingsFolder;
                            mmApp.Configuration.Read();
                        }


                        mmApp.Configuration.CommonFolder = portableSettingsFolder;
                        mmApp.Configuration.Write();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unable to set portable mode: " + ex.Message);
                    }

                    ConsoleFooter();
                    break;
                case "unsetportable":
                    ConsoleHeader();
                    try
                    {
                        File.Delete("_IsPortable");
                        mmApp.Configuration.InternalCommonFolder = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Markdown Monster");
                        mmApp.Configuration.CommonFolder = mmApp.Configuration.InternalCommonFolder;
                        mmApp.Configuration.Write();

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Removed Portable settings for this installation. Use `mm SetPortable` to reenable.");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Unable to delete portable settings switch file\r\n_IsPortable\r\n\r\n{ex.Message}");
                    }

                    break;
                case "register":
                    ConsoleHeader();
                    if (App.CommandArgs.Length < 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Missing registration code. Please pass a registration code.");
                    }
                    else
                    {
                        if (!UnlockKey.Register(App.CommandArgs[1]))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Invalid registration code. Please pass a valid registration code.");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Registration succeeded. Thank your for playing fair.");
                        }
                    }
                    ConsoleFooter();
                    break;
                // Standard In Re-Routing
                case "stdin":
                    string stdin = null;
                    if (Console.IsInputRedirected)
                    {
                        using (var stream = Console.OpenStandardInput())
                        {
                            byte[] buffer = new byte[1000];  // Use whatever size you want
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
                                })
                                {
                                    IsBackground = true
                                };

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

                            if (builder.Length > 0)
                            {
                                var tempFile = Path.ChangeExtension(Path.GetTempFileName(), "md");
                                File.WriteAllText(tempFile, builder.ToString());
                                App.CommandArgs[0] = tempFile;
                            }
                            else
                            {
                                App.CommandArgs[0] = null;
                            }
                        }
                    }

                    break;
            }
        }



        /// <summary>
        /// Method used to set up the header for Console operation
        /// </summary>
        static void ConsoleHeader()
        {
            AttachConsole(-1);

            var arg0 = App.CommandArgs[0].ToLower().TrimStart('-');
            //Console.Clear();
            Console.WriteLine(" ");
            var title = "Markdown Monster Console v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            Console.WriteLine(title);
            Console.WriteLine(StringUtils.Replicate("-", title.Length));
            Console.WriteLine("  Command: " + arg0);
            if (App.CommandArgs.Length > 1 && arg0 != "register")
            {
                Console.Write("Arguments: ");
                foreach (var arg in App.CommandArgs.Skip(1))
                    Console.Write("\"" + arg + "\" ");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// Resets console and exits
        /// </summary>
        void ConsoleFooter()
        {
            Console.ResetColor();
            FreeConsole();
            Environment.Exit(0);
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
