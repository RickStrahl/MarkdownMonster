using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
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
        public Mutex Mutex;
        string filesToOpen = null;

        public App()
        {
            
            if (mmApp.Configuration.UseSingleWindow)
            {
                bool isOnlyInstance = false;
                Mutex = new Mutex(true, @"MarkdownMonster", out isOnlyInstance);
                if (!isOnlyInstance)
                {
                    string filesToOpen = " ";
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

                    //File.WriteAllText(mmApp.Configuration.FileWatcherOpenFilePath, filesToOpen);

                    var manager = new NamedPipeManager("MarkdownMonster");
                    manager.Write(filesToOpen);

                    Mutex.Dispose();
                    mmApp.Configuration = null;

                    GC.SuppressFinalize(manager);

                    Environment.Exit(0);
                }            
            }

            SplashScreen splashScreen = new SplashScreen("assets/markdownmonstersplash.png");
            splashScreen.Show(true);

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalErrorHandler);
        }


        /// TODO: Handle global errors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static void GlobalErrorHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            MessageBox.Show("Global Error: "  + e.Message);
            Console.WriteLine("MyHandler caught : " + e.Message);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            var dir = Assembly.GetExecutingAssembly().Location;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(dir));            
            mmApp.SetTheme(mmApp.Configuration.ApplicationTheme, App.Current.MainWindow as MetroWindow);

            AddinManager.Current.LoadAddins();
            AddinManager.Current.RaiseOnApplicationStart();            
        }
        
    }
}
