using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MarkdownMonster.AddIns;
using Westwind.Utilities;

namespace MarkdownMonster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static Mutex Mutex;
        string filesToOpen = null;

        public App()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {

            Exception e = (Exception)args.ExceptionObject;
            MessageBox.Show("Global Error: "  + e.Message);
            Console.WriteLine("MyHandler caught : " + e.Message);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
        }
    

        protected override void OnStartup(StartupEventArgs e)
        {
            if (mmApp.Configuration.UseSingleWindow)
            {
                bool isOnlyInstance = false;
                Mutex = new Mutex(true, @"MarkdownMonster", out isOnlyInstance);
                if (!isOnlyInstance)
                {
                    filesToOpen = " ";
                    var args = Environment.GetCommandLineArgs();
                    if (args != null && args.Length > 1)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 1; i < args.Length; i++)
                        {
                            sb.AppendLine(args[i]);
                        } 
                        filesToOpen = sb.ToString();
                    }

                    File.WriteAllText(mmApp.Configuration.FileWatcherOpenFilePath, filesToOpen);

                    Mutex.Dispose();
                    mmApp.Configuration = null;

                    Environment.Exit(0);


                    // This blows up when writing files and file watcher watching
                    // No idea why - Environment.Exit() works with no issue
                    //ShutdownMode = ShutdownMode.OnMainWindowClose;
                    //App.Current.Shutdown();

                    return;
                }
            }

            var dir = Assembly.GetExecutingAssembly().Location;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(dir));            
            mmApp.SetTheme(mmApp.Configuration.ApplicationTheme, App.Current.MainWindow as MetroWindow);

            AddinManager.Current.LoadAddins();
            AddinManager.Current.RaiseOnApplicationStart();            
        }
        
    }
}
