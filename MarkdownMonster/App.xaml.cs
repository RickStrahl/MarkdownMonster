#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 04/28/2016
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using MahApps.Metro.Controls;
using MarkdownMonster.AddIns;

namespace MarkdownMonster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static Mutex Mutex { get; set; }

        public static string initialStartDirectory;

        public App()
        {
            initialStartDirectory = Environment.CurrentDirectory;

            SplashScreen splashScreen = new SplashScreen("assets/markdownmonstersplash.png");
            splashScreen.Show(true);

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
                            string file = args[i];

                            // check if file exists and fully qualify to 
                            // pass to named pipe
                            if (!File.Exists(file))
                            {
                                file = Path.Combine(initialStartDirectory, file);
                                if (!File.Exists(file))
                                    file = null;                                
                            }

                            if (!string.IsNullOrEmpty(file))                                                            
                                sb.AppendLine(Path.GetFullPath(file));
                            
                        }
                        filesToOpen = sb.ToString();
                    }
                    var manager = new NamedPipeManager("MarkdownMonster");
                    manager.Write(filesToOpen);

                    splashScreen.Close(TimeSpan.MinValue);

                    // this exits the application                    
                    Environment.Exit(0);
                }
            }

            //AppDomain currentDomain = AppDomain.CurrentDomain;
            //currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalErrorHandler);

            DispatcherUnhandledException += App_DispatcherUnhandledException;

            mmApp.Started = DateTime.UtcNow;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            
            Exception ex = e.Exception;
            mmApp.Log(ex);

            var msg = string.Format("Yikes! Something went wrong...\r\n\r\n{0}\r\n\r\n" +
                "The error has been recorded and written to a log file and you can\r\n" +
                "review the details or report the error via Help | Show Error Log\r\n\r\n" +
                "Do you want to continue?", ex.Message);

            var res = MessageBox.Show(msg, mmApp.ApplicationName + " Error",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Error);
            if (res.HasFlag(MessageBoxResult.No))
                this.Shutdown(0);
            else
                e.Handled = true;


        }

        public static string InstallerDownloadUrl { get; internal set; }
        public static string UserDataPath { get; internal set; }
        public static string VersionCheckUrl { get; internal set; }


        /// TODO: Handle global errors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static void GlobalErrorHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception ex = (Exception)args.ExceptionObject;            
            mmApp.Log(ex);

            var msg = string.Format("Yikes! Something went wrong...\r\n\r\n{0}\r\n\r\n" +
                "The error has been recorded and written to a log file and you can\r\n" +
                "review the details or report the error via Help | Show Error Log\r\n\r\n" +
                "Do you want to continue?", ex.Message);

            var res = MessageBox.Show(msg, mmApp.ApplicationName + " Error",
                                                MessageBoxButton.YesNo, 
                                                MessageBoxImage.Error);
            if (res.HasFlag(MessageBoxResult.No))
                Environment.Exit(0);            
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
