#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 04/28/2016
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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
        public static Mutex Mutex { get; set; }

        public static string initialStartDirectory;

        public static string[] commandArgs;

        static App()
        {
            //try
            //{   // Multi-Monitor DPI awareness for screen captures
            //    // requires [assembly: DisableDpiAwareness] set in assemblyinfo
            //    bool res = WindowUtilities.SetPerMonitorDpiAwareness(ProcessDpiAwareness.Process_Per_Monitor_DPI_Aware);                
            //}
            //catch {  /* fails not supported on Windows 7 and older */ }
        }


        public App()
        {

            SplashScreen splashScreen = new SplashScreen("assets/markdownmonstersplash.png");
            splashScreen.Show(true);

	        initialStartDirectory = Environment.CurrentDirectory;

			// Singleton launch marshalls subsequent launches to the singleton instance
			// via named pipes communication
	        if (mmApp.Configuration.UseSingleWindow)
		        CheckForSingletonLaunch(splashScreen);

            // We have to manage assembly loading for Addins 
	        AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
#if !DEBUG
            //AppDomain currentDomain = AppDomain.CurrentDomain;
            //currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalErrorHandler);
            DispatcherUnhandledException += App_DispatcherUnhandledException;
#endif
           			
            mmApp.ApplicationStart();
        }


		/// <summary>
		/// Checks to see if app is already running and if it is pushes
		/// parameters via NamedPipes to existing running application
		/// and exits this instance.
		/// 
		/// Otherwise app just continues
		/// </summary>
		/// <param name="splashScreen"></param>
	    private static void CheckForSingletonLaunch(SplashScreen splashScreen)
	    {
            // fix up the startup path
	        string filesToOpen = " ";
	        var args = Environment.GetCommandLineArgs();
	        if (args != null && args.Length > 1)
	        {
	            StringBuilder sb = new StringBuilder();
	            for (int i = 1; i < args.Length; i++)
	            {
	                string file = args[i];
	                if (string.IsNullOrEmpty(file))
	                    continue;

	                file = file.TrimEnd('\\');
	                file = Path.GetFullPath(file);
                    sb.AppendLine(file);
	                
                    // write fixed up path arguments
                    args[i] = file;
	            }
	            filesToOpen = sb.ToString();
	        }
            commandArgs = args;


            bool isOnlyInstance;
		    Mutex = new Mutex(true, @"MarkdownMonster", out isOnlyInstance);
		    if (isOnlyInstance)
			    return;
		    
		    var manager = new NamedPipeManager("MarkdownMonster");
		    manager.Write(filesToOpen);
	        

            splashScreen.Close(TimeSpan.MinValue);

		    // Shut down application
		    Environment.Exit(0);
	    }

	    private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {            
            // missing resources are... missing
            if (args.Name.Contains(".resources"))
                return null;

            // check for assemblies already loaded
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            // NOTE: this doesn't account for special search paths but then that never
            //           worked before either.
            string filename = args.Name.Split(',')[0] + ".dll".ToLower();

            
            // try load the assembly from install path (this should always be automatic)
            try
            {
                // this allows addins to load before form has loaded
                return Assembly.LoadFrom(filename);
            }
            catch
            { }
            
            // try load from install addins folder
            string asmFile = FindFileInPath(filename, ".\\Addins");
            if (!string.IsNullOrEmpty(asmFile))
            {
                try
                {
                    return Assembly.LoadFrom(asmFile);
                }
                catch
                { }
            }

            return null;
        }

        /// <summary>
        /// Looks for the first match in a file structure
        /// </summary>
        /// <param name="filename">The filename only to look for</param>
        /// <param name="path">Path to start with</param>
        /// <returns>Fully qualified path of the file found or NULL</returns>
        private string FindFileInPath(string filename, string path)
        {
            filename = filename.ToLower();

            foreach (var fullFile in Directory.GetFiles(path))
            {
                var file = Path.GetFileName(fullFile).ToLower();
                if (file == filename)
                    return fullFile;

            }
            foreach (var dir in Directory.GetDirectories(path))
            {
                var file = FindFileInPath(filename, dir);
                if (!string.IsNullOrEmpty(file))
                    return file;
            }

            return null;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (!mmApp.HandleApplicationException(e.Exception as Exception))
                Environment.Exit(1);
            
            e.Handled = true;
        }

        
        public static string UserDataPath { get; internal set; }
        public static string VersionCheckUrl { get; internal set; }


      

        protected override void OnStartup(StartupEventArgs e)
        {
            var dotnetVersion = ComputerInfo.GetDotnetVersion();
            if (String.Compare(dotnetVersion, "4.6", StringComparison.Ordinal) < 0)
            {
                new TaskFactory().StartNew(() => MessageBox.Show("Markdown Monster requires .NET 4.6 or later to run.\r\n\r\n" +
                                                                       "Please download and install the latest version of .NET version from:\r\n" +
                                                                       "https://www.microsoft.com/net/download/framework\r\n\r\n" +
                                                                       "Exiting application and navigating to .NET Runtime Downloads page.",
                    "Markdown Monster",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                ));

                Thread.Sleep(10000);
                ShellUtils.GoUrl("https://www.microsoft.com/net/download/framework");
                Environment.Exit(0);
            }

            new TaskFactory().StartNew(LoadAddins);

            if (mmApp.Configuration.DisableHardwareAcceleration)
                RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

            var dir = Assembly.GetExecutingAssembly().Location;

            Directory.SetCurrentDirectory(Path.GetDirectoryName(dir));

            ThemeCustomizations();

            if (!mmApp.Configuration.DisableAddins)
            {
                new TaskFactory().StartNew(() =>
                {
                    ComputerInfo.EnsureBrowserEmulationEnabled("MarkdownMonster.exe");
                    ComputerInfo.EnsureSystemPath();

                    if (!Directory.Exists(mmApp.Configuration.InternalCommonFolder))
                        Directory.CreateDirectory(mmApp.Configuration.InternalCommonFolder);
                });
            }
        }

        private void ThemeCustomizations()
        {
            // Custom MahApps Light Theme based on Blue
            ThemeManager.AddAccent("MahLight", new Uri("Styles/MahLightAccents.xaml", UriKind.RelativeOrAbsolute));

            // Add Dark Menu Customizations
            if (mmApp.Configuration.ApplicationTheme == Themes.Dark)
            {
                var menuCustomizations = new Uri("Styles/MahMenuCustomizations.xaml", UriKind.RelativeOrAbsolute);
                Application.Current.Resources.MergedDictionaries.Add(
                    new ResourceDictionary() {Source = menuCustomizations});                
            }
            else
            {
                var dragablzLightStyles = new Uri("Styles/DragablzGenericLight.xaml", UriKind.RelativeOrAbsolute);
                Application.Current.Resources.MergedDictionaries.Add(
                    new ResourceDictionary() { Source = dragablzLightStyles });

                Resources["HeadlineColor"] = new SolidColorBrush(Colors.SteelBlue);
                Resources["BlueItem"] = new SolidColorBrush(Colors.SteelBlue);
            }
            mmApp.SetTheme(mmApp.Configuration.ApplicationTheme, App.Current.MainWindow as MetroWindow);

        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);
        }

        /// <summary>
        /// Loads all addins asynchronously without loading the
        /// addin UI  -handled in Window Load to ensure Window is up)
        /// </summary>
        private void LoadAddins()
        {
            try
            {
                AddinManager.Current.LoadAddins(Path.Combine(Environment.CurrentDirectory, "AddIns"));
                AddinManager.Current.LoadAddins(mmApp.Configuration.AddinsFolder);
                AddinManager.Current.AddinsLoadingComplete = true;
                //Model.OnPropertyChanged(nameof(AppModel.MarkdownParserNames));
                //Model.OnPropertyChanged(nameof(AppModel.MarkdownParserColumnWidth));
                try
                {
                    AddinManager.Current.RaiseOnApplicationStart();
                }
                catch (Exception ex)
                {
                    mmApp.Log("Addin loading failed", ex);
                }
            }
            catch (Exception ex)
            {
                mmApp.Log("Addin loading failed", ex);
            }
        }
    }
    
}
