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
using MarkdownMonster;
using Westwind.Utilities;

namespace mmcli
{
    /// <summary>
    /// This class has handles the 'console' like command line
    /// operations for Markdown Monster.
    /// </summary>
    public class CommandLineProcessor
    {
        public string[] CommandArgs { get; }

        public CommandLineProcessor()
        {
            CommandArgs =Environment.GetCommandLineArgs().Skip(1).ToArray(); 
        }

        


        public void HandleCommandLineArguments()
        {
            

            var arg0 = CommandArgs[0].ToLower().TrimStart('-');
            if (CommandArgs[0] == "-")
                arg0 = "-";


            HtmltoMarkdownCommandline converter;
            switch (arg0)
            {
                case "version":
                    // just display the header
                    ConsoleHeader();
                    ConsoleFooter();
                    break;
                case "uninstall":
                    UninstallSettings();

                    ConsoleHeader();
                    ConsoleHelper.WriteLine("Markdown Monster Machine Wide Settings uninstalled.",ConsoleColor.Green);
                    ConsoleFooter();

                    break;
                case "reset":
                    // load old config and backup
                    mmApp.Configuration.Backup();
                    mmApp.Configuration.Reset(); // forces exit

                    ConsoleHeader();
                    ConsoleHelper.WriteLine("Markdown Monster Settings reset to defaults.",ConsoleColor.Green);
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
                        ConsoleHelper.WriteLine("Unable to set portable mode: " + ex.Message,ConsoleColor.Red);
                    }

                    ConsoleFooter();
                    break;
                case "unsetportable":
                    ConsoleHeader();
                    try
                    {
                        File.Delete("_IsPortable");
                        var internalFolder  = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Markdown Monster");
                        ReflectionUtils.SetProperty(mmApp.Configuration, "InternalCommonFolder", internalFolder);
                        mmApp.Configuration.CommonFolder = internalFolder; 
                        mmApp.Configuration.Write();

                        ConsoleHelper.WriteLine("Removed Portable settings for this installation. Use `mm SetPortable` to reenable.",ConsoleColor.Green);
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.WriteLine($"Unable to delete portable settings switch file\r\n_IsPortable\r\n\r\n{ex.Message}",ConsoleColor.Red);
                    }

                    break;
                case "register":
                    ConsoleHeader();
                    if (CommandArgs.Length < 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Missing registration code. Please pass a registration code.");
                    }
                    else
                    {
                        if (! UnlockKey.Register(CommandArgs[1]))
                        {
                            ConsoleHelper.WriteLine("Invalid registration code. Please pass a valid registration code.",ConsoleColor.Red);
                        }
                        else
                        {
                            ConsoleHelper.WriteLine("Markdown Monster Registration successful.",ConsoleColor.Green);
                            ConsoleHelper.WriteLine("Thank you for playing fair!",ConsoleColor.Green);
                        }
                    }

                    ConsoleFooter();
                    break;
               
                case "markdowntohtml":
                {
                    int parmCount = CommandArgs.Length;
                    string inputFile = parmCount > 1 ? CommandArgs[1] : null;
                    string outputFile = parmCount > 2 ? CommandArgs[2] : null;
                    string renderMode = parmCount > 3 ? CommandArgs[3] : null;   // html,packagedhtml,zip

                    if (outputFile.StartsWith("-"))
                        outputFile = null;
                    if (inputFile.StartsWith("-"))
                        inputFile = null;
                    if (renderMode.StartsWith("-"))
                        renderMode = null;

                    bool openOutputFile =
                        Environment.CommandLine.Contains("-open", StringComparison.OrdinalIgnoreCase) ||
                        string.IsNullOrEmpty(outputFile);

                    converter = new HtmltoMarkdownCommandline(this);
                    converter.MarkdownToHtml(inputFile, outputFile,openOutputFile,renderMode = renderMode);
                    break;
                }
                case "htmltomarkdown":
                {
                    converter = new HtmltoMarkdownCommandline(this);

                    int parmCount = CommandArgs.Length;
                    string inputFile = parmCount > 0 ? CommandArgs[1] : null;
                    string outputFile = parmCount > 1 ? CommandArgs[2] : null;
                    if (outputFile.StartsWith("-"))
                            outputFile = null;
                    if (inputFile.StartsWith("-"))
                        inputFile = null;

                    bool openOutputFile =
                        Environment.CommandLine.Contains("-open", StringComparison.OrdinalIgnoreCase) ||
                        string.IsNullOrEmpty(outputFile);

                    converter.HtmlToMarkdown(inputFile, outputFile, openOutputFile);
                    break;
                }
            }
        }




        /// <summary>
        /// Method used to set up the header for Console operation
        /// </summary>
        public void ConsoleHeader()
        {
            var arg0 = CommandArgs[0].ToLower().TrimStart('-');
            //Console.Clear();
            Console.WriteLine(" ");
            var title = "Markdown Monster Console v" + typeof(MarkdownMonster.mmApp).Assembly.GetName().Version.ToString(3);
            Console.WriteLine(StringUtils.Replicate("-", title.Length));
            ConsoleHelper.WriteLine(title,ConsoleColor.Yellow);
            Console.WriteLine(StringUtils.Replicate("-", title.Length));
            Console.WriteLine("Command: " + arg0);
            Console.WriteLine();
        }

        /// <summary>
        /// Resets console and exits
        /// </summary>
        public void ConsoleFooter(bool noExit = false)
        {
            Console.ResetColor();
            Console.WriteLine();
        }


        /// <summary>
        /// Uninstall registry and configuration settings
        /// </summary>
        private void UninstallSettings()
        {
            ConsoleHeader();

            mmFileUtils.EnsureBrowserEmulationEnabled("MarkdownMonster.exe", uninstall: true);
            mmFileUtils.EnsureSystemPath(uninstall: true);
            mmFileUtils.EnsureAssociations(uninstall: true);

            ConsoleHelper.WriteLine("Permanent Markdown Monster settings have been uninstalled from the registry.",ConsoleColor.Green);

            ConsoleFooter();
        }
    }

    public static class ConsoleHelper
    {
        public static void WriteLine(string text, ConsoleColor color = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }

    }
}
