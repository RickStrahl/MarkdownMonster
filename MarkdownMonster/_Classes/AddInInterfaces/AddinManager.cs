using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Interop;
using FontAwesome.WPF;
using Westwind.Utilities;

namespace MarkdownMonster.AddIns
{
    /// <summary>
    /// This class manages loading of addins and 
    /// raising various application events passed
    /// to all addins that they can respond to
    /// </summary>
    public class AddinManager
    {
        /// <summary>
        /// Singleton to get access to Addin Manager
        /// </summary>
        public static AddinManager Current { get; set; }

        /// <summary>
        /// The full list of add ins registered
        /// </summary>
        public List<MarkdownMonsterAddin> AddIns;

        /// <summary>
        /// Set after addins have completed load
        /// </summary>
        public bool AddinsLoadingComplete { get; set; }

        public string ErrorMessage { get; set; }

        static AddinManager()
        {
            Current = new AddinManager();
        }

        public AddinManager()
        {
            AddIns = new List<MarkdownMonsterAddin>();
        }


        private static bool AddinInterfaceFilter(Type typeObj, Object criteriaObj)
        {
            if (typeObj.ToString() == criteriaObj.ToString())
                return true;

            return false;
        }


        /// <summary>
        /// Loads the add-in menu and toolbar buttons
        /// </summary>
        /// <param name="window"></param>
        public void InitializeAddinsUi(MainWindow window, List<MarkdownMonsterAddin> addins = null)
        {
            if (addins == null)
                addins = this.AddIns;

            foreach (var addin in addins)
            {
                addin.Model = window.Model;                

                foreach (var menuItem in addin.MenuItems)
                {
                    var mitem = new MenuItem()
                    {
                        Header = menuItem.Caption,                        
                    };                    
                    if (menuItem.CanExecute == null)
                        menuItem.Command = new CommandBase((s, c) => menuItem.Execute?.Invoke(mitem));
                    else
                        menuItem.Command = new CommandBase((s, c) => menuItem.Execute?.Invoke(mitem),
                                                           (s, c) => menuItem.CanExecute.Invoke(mitem));                    
                    mitem.Command = menuItem.Command;

                    int menuIndex = addin.Model.Window.MenuAddins.Items.Add(mitem);

                    // if an icon is provided also add to toolbar
                    if (menuItem.FontawesomeIcon != FontAwesomeIcon.None)
                    {
                        var hasConfigMenu = menuItem.ExecuteConfiguration != null;

                        var titem = new Button();

                        titem.Content = new Image()
                        {
                            Source =
                                ImageAwesome.CreateImageSource(menuItem.FontawesomeIcon, addin.Model.Window.Foreground),
                            ToolTip = menuItem.Caption + 
                                        (!string.IsNullOrEmpty(menuItem.KeyboardShortcut) ?
                                            $" ({menuItem.KeyboardShortcut})" :
                                            string.Empty),
                            Height = 16,
                            Width = 16,
                            Margin = new Thickness(5, 0, hasConfigMenu ? 0 : 5, 0)
                        };

                        if (menuItem.Execute != null)
                        {
                            titem.Command = menuItem.Command;
                            AddKeyboardShortcut(menuItem, addin);
                        }
                        
                        addin.Model.Window.ToolbarAddIns.Visibility = Visibility.Visible;
                        int toolIndex = addin.Model.Window.ToolbarAddIns.Items.Add(titem);
                    
                        // Add configuration dropdown if configured
                        if (hasConfigMenu)
                        {
                            var tcitem = new Button
                            {
                                FontSize = 10F,
                                Content = new Image()
                                {
                                    Source =
                                        ImageAwesome.CreateImageSource(FontAwesomeIcon.CaretDown,
                                            addin.Model.Window.Foreground),                                    
                                    Height = 16,
                                    Width = 8,
                                    Margin = new Thickness(0, 0, 0, 0),
                                },
                                ToolTip = menuItem.Caption + " Configuration",                                
                            };

                            var ctxm = new ContextMenu();
                            tcitem.ContextMenu = ctxm;

                            // create context menu and add drop down behavior
                            var behaviors = Interaction.GetBehaviors(tcitem);
                            behaviors.Add(new DropDownButtonBehavior());

                            tcitem.Click += (sender, args) =>
                            {
                                ctxm.Items.Clear();
                                var configMenuItem = new MenuItem()
                                {
                                    Header = menuItem.Caption
                                };
                                configMenuItem.Command = menuItem.Command;                                
                                if (menuItem.CanExecute != null)
                                    configMenuItem.IsEnabled = menuItem.CanExecute.Invoke(sender);
                                ctxm.Items.Add(configMenuItem);

                                configMenuItem = new MenuItem()
                                {
                                    Header = menuItem.Caption + " Configuration",
                                };
                                if (menuItem.ExecuteConfiguration != null)
                                    configMenuItem.Click += (s, e) => menuItem.ExecuteConfiguration?.Invoke(s);
                                
                                ctxm.Items.Add(configMenuItem);                                
                            };

                            addin.Model.Window.ToolbarAddIns.Items.Add(tcitem);
                        };

                        addin.Model.PropertyChanged += (s, arg) =>
                        {
                            if (arg.PropertyName == "ActiveDocument" || arg.PropertyName == "ActiveEditor")
                            {
                                menuItem.Command?.InvalidateCanExecute();

                                // this shouldn't be necessary but it looks if the Command bindings work correctly
                                //var item = addin.Model.Window.ToolbarAddIns.Items[toolIndex] as Button;
                                //if (item != null)
                                //{
                                //    ((CommandBase)item.Command).InvalidateCanExecute();
                                //    if (menuItem.CanExecute != null)
                                //        item.IsEnabled = menuItem.CanExecute.Invoke(null);
                                //}
                            }
                        };                        
                    }
                }
            }
        }

        private static void AddKeyboardShortcut(AddInMenuItem menuItem, MarkdownMonsterAddin addin)
        {
            if (!string.IsNullOrEmpty(menuItem.KeyboardShortcut))
            {
                var ksc = menuItem.KeyboardShortcut.ToLower();
                KeyBinding kb = new KeyBinding();

                if (ksc.Contains("alt"))
                    kb.Modifiers = ModifierKeys.Alt;
                if (ksc.Contains("shift"))
                    kb.Modifiers |= ModifierKeys.Shift;                
                if (ksc.Contains("ctrl") || ksc.Contains("ctl"))
                    kb.Modifiers |= ModifierKeys.Control;
                if (ksc.Contains("win"))
                    kb.Modifiers |= ModifierKeys.Windows;

                string key =
                    ksc.Replace("+", "")
                        .Replace("-", "")
                        .Replace("_", "")
                        .Replace(" ", "")
                        .Replace("alt", "")
                        .Replace("shift", "")
                        .Replace("ctrl", "")
                        .Replace("ctl", "");

                key = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key);
                if (!string.IsNullOrEmpty(key))
                {
                    KeyConverter k = new KeyConverter();
                    kb.Key = (Key) k.ConvertFromString(key);
                }

                // Whatever command you need to bind to
                kb.Command = menuItem.Command;
                addin.Model.Window.InputBindings.Add(kb);
            }
        }

        private void ConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Tcitem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            throw new NotImplementedException();
        }

        #region Raise Events
        public void RaiseOnApplicationStart()
        {
            foreach (var addin in AddIns)
            {
                try
                {
                    addin?.OnApplicationStart();
                }
                catch (Exception ex)
                {
                    mmApp.Log(addin.Id + "::AddIn::OnApplicationStart Error: " + ex.GetBaseException().Message);
                }
            }
        }

        public void RaiseOnWindowLoaded()
        {
            foreach (var addin in AddIns)
            {
                try
                {
                    addin?.OnWindowLoaded();
                }
                catch (Exception ex)
                {
                    mmApp.Log(addin.Id + "::AddIn::OnApplicationStart Error: " + ex.GetBaseException().Message);
                }
            }
        }


        public void RaiseOnApplicationShutdown()
        {
            foreach (var addin in AddIns)
            {
                try
                {
                    addin?.OnApplicationShutdown();
                }
                catch (Exception ex)
                {

                    mmApp.Log(addin.Id + "::AddIn::OnApplicationShutdown Error: " + ex.GetBaseException().Message);
                }

            }
        }

        public bool RaiseOnBeforeOpenDocument(string filename)
        {
            foreach (var addin in AddIns)
            {
                if (addin == null)
                    continue;
                try
                {
                    if (!addin.OnBeforeOpenDocument(filename))
                        return false;
                }
                catch (Exception ex)
                {

                    mmApp.Log(addin.Id + "::AddIn::OnBeforeOpenDocument Error: " + ex.GetBaseException().Message);
                }
            }

            return true;
        }


        public void RaiseOnAfterOpenDocument(MarkdownDocument doc)
        {
            foreach (var addin in AddIns)
            {
                try
                {
                    addin?.OnAfterOpenDocument(doc);
                }
                catch (Exception ex)
                {
                    mmApp.Log(addin.Id + "::AddIn::nAfterOpenDocument Error: " + ex.GetBaseException().Message);
                }
            }
        }

        public bool RaiseOnBeforeSaveDocument(MarkdownDocument doc)
        {
            foreach (var addin in AddIns)
            {
                if (addin == null)
                    continue;
                try
                {
                    if (!addin.OnBeforeSaveDocument(doc))
                        return false;
                }
                catch (Exception ex)
                {
                    mmApp.Log(addin.Id + "::AddIn::OnBeforeSaveDocument Error: " + ex.GetBaseException().Message);
                }
            }

            return true;
        }


        public void RaiseOnAfterSaveDocument(MarkdownDocument doc)
        {
            foreach (var addin in AddIns)
            {
                try
                {
                    addin?.OnAfterSaveDocument(doc);
                }
                catch (Exception ex)
                {
                    mmApp.Log(addin.Id + "::AddIn::OnAfterSaveDocument Error: " + ex.GetBaseException().Message);
                }
            }
        }

        public string RaiseOnSaveImage(object image)
        {
            string url = null;

            foreach (var addin in AddIns)
            {
                try
                {
                    url = addin?.OnSaveImage(image);
                }
                catch (Exception ex)
                {
                    mmApp.Log(addin.Id + "::AddIn::OnAfterSaveDocument Error: " + ex.GetBaseException().Message);
                }
            }

            return url;
        }

        public void RaiseOnDocumentActivated(MarkdownDocument doc)
        {
            foreach (var addin in AddIns)
            {
                try
                {
                    addin?.OnDocumentActivated(doc);
                }
                catch (Exception ex)
                {
                    mmApp.Log(addin.Id + "::AddIn::OnDocumentActivated Error: " + ex.GetBaseException().Message);
                }
            }
        }

        public void RaiseOnNotifyAddin(string command, object parameter)
        {
            foreach (var addin in AddIns)
            {
                try
                {
                    addin?.OnNotifyAddin(command, parameter);
                }
                catch (Exception ex)
                {
                    mmApp.Log(addin.Id + "::AddIn::OnNotifyAddin Error: " + ex.GetBaseException().Message);
                }
            }
        }

        public string RaiseOnEditorCommand(string action, string input)
        {
            foreach (var addin in AddIns)
            {
                try
                {
                    string html = addin?.OnEditorCommand(action, input);
                    if (string.IsNullOrEmpty(html))
                        return html;
                }
                catch (Exception ex)
                {
                    mmApp.Log(addin.Id + "::AddIn::OnDocumentActivated Error: " + ex.GetBaseException().Message);
                }
            }

            return null;
        }

        public void RaiseOnDocumentChanged()
        {
            foreach (var addin in AddIns)
            {
                try
                {                    
                    addin?.OnDocumentUpdated();                    
                }
                catch (Exception ex)
                {
                    mmApp.Log(addin.Id + "::AddIn::OnDocumentUpdated Error: " + ex.GetBaseException().Message);
                }
            }

        }        

        #endregion

        #region Addin Manager

        /// <summary>
        /// Loads add-ins into the application from the add-ins folder.
        /// 
        /// Note: This method is called twice: Once for the install
        /// Addins folder for built-in addins and once for the
        /// %AppData% folder for user installed addins.
        /// </summary>
        internal void LoadAddins(string addinPath)
        {            
            if (!Directory.Exists(addinPath))
                return;

            try
            {
                var delDirs = Directory.GetDirectories(".\\Addins");
                foreach (string delDir in delDirs)
                {
                    if (!delDir.EndsWith("ScreenCapture") && !delDir.EndsWith("Weblog"))
                    {
                        var targetFolder = Path.Combine(mmApp.Configuration.AddinsFolder, Path.GetFileName(delDir));
                        if (!Directory.Exists(targetFolder))
                            Directory.Move(delDir, targetFolder);
                        else
                            Directory.Delete(delDir, true);
                    }                        
                }
            }
            catch
            { }
            // END TODO: Remove after a few months

            // Check for Addins to install
            try
            {
                if (Directory.Exists(addinPath + "\\Install"))
                    InstallAddinFiles(addinPath + "\\Install\\");
            }
            catch (Exception ex)
            {
                mmApp.Log($"Addin Update failed: {ex.Message}");
            }

            var dirs = Directory.GetDirectories(addinPath);            
            foreach (var dir in dirs)
            {
                var files = Directory.GetFiles(dir, "*.dll");
                foreach (var file in files)
                {
                    string fname = Path.GetFileName(file).ToLower();
                    if (fname.EndsWith("addin.dll"))
                        LoadAddinClasses(file);
                }
            }
        }

        public bool InstallAddin(string addinId)
        {
            string addinPath = Path.Combine(mmApp.Configuration.CommonFolder, "Addins", addinId);
            if (!Directory.Exists(addinPath))
                return false;

            var files = Directory.GetFiles(addinPath, "*.dll");
            foreach (var file in files)
            {
                string fname = Path.GetFileName(file).ToLower();
                if (fname.EndsWith("addin.dll"))
                    LoadAddinClasses(file,addinId);
            }

            return true;
        }


        /// <summary>
        /// Load all add in classes in an assembly
        /// </summary>
        /// <param name="assemblyFile"></param>
        public void LoadAddinClasses(string assemblyFile,string addinId = null)
        {
            Assembly asm = null;
            Type[] types = null;

            try
            {
                asm = Assembly.LoadFrom(assemblyFile);
                types = asm.GetTypes();
            }
            catch(Exception ex)
            {
                var msg = $"Unable to load add-in: {Path.GetFileNameWithoutExtension(assemblyFile)}\r\n\r\n" +                          
                          "Try updating the add-in and Markdown Monster to the latest versions or uninstall the add-in.";

                mmApp.Log(msg, ex);
                MessageBox.Show(msg,"Assembly Load Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);                
                return;
            }

            foreach (var type in types)
            {
                var typeList = type.FindInterfaces(AddinInterfaceFilter, typeof(IMarkdownMonsterAddin));
                if (typeList.Length > 0)
                {
                    var ai = Activator.CreateInstance(type) as MarkdownMonsterAddin;
                    if (addinId != null)
                        ai.Id = addinId;
                    AddIns.Add(ai);
                }
            }
        }

        /// <summary>
        /// Retrieves a list of addins from the addin repository. Note this list
        /// is retrieved in chunks - first the summary list is retrieved and the
        /// remaining data is filled in later from individual repos.
        /// </summary>        
        /// <returns></returns>
        public List<AddinItem> GetAddinList()
        {
            const string addinListRepoUrl =
                "https://raw.githubusercontent.com/RickStrahl/MarkdownMonsterAddinsRegistry/master/MarkdownMonsterAddinRegistry.json";

            var settings = new HttpRequestSettings
            {
                Url = addinListRepoUrl,
                Timeout = 5000
            };

            List<AddinItem> addinList;
            try
            {
                addinList = HttpUtils.JsonRequest<List<AddinItem>>(settings);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
                return null;
            }

            addinList
                .AsParallel()
                .ForAll(ai =>
                {
                    try
                    {
                        var dl = HttpUtils.JsonRequest<AddinItem>(new HttpRequestSettings
                        {
                            Url = ai.gitVersionUrl
                        });
                        DataUtils.CopyObjectData(dl, ai, "id,name,gitVersionUrl,gitUrl");

                        if (Directory.Exists(".\\Addins\\" + ai.id) ||
                            Directory.Exists(".\\Addins\\Installs\\" + ai.id))
                            ai.isInstalled = true;

                        if (File.Exists(".\\Addins\\Installs\\" + ai.id + ".delete"))
                            ai.isInstalled = false;
                    }
                    catch { /* ignore error */}
                });



            return addinList;
        }


        /// <summary>
        /// Retrieves an initial minimal list of addins which is supplemented later
        /// with data from individual repos.
        /// </summary>
        /// <returns></returns>
        public async Task<List<AddinItem>> GetInitialAddinListAsync()
        {
            const string addinListRepoUrl =
                "https://raw.githubusercontent.com/RickStrahl/MarkdownMonsterAddinsRegistry/master/MarkdownMonsterAddinRegistry.json";

            var settings = new HttpRequestSettings
            {
                Url = addinListRepoUrl,
                Timeout = 5000
            };

            List<AddinItem> addinList;
            try
            {
                addinList = await HttpUtils.JsonRequestAsync<List<AddinItem>>(settings);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return null;
            }

            return addinList;
        }


        /// <summary>
        /// Retrieves a list of addins from the addin repository. Note this list
        /// is retrieved in chunks - first the summary list is retrieved and the
        /// remaining data is filled in later from individual repos.
        /// </summary>
        /// <param name="addinList"></param>
        /// <returns></returns>
        public async Task<List<AddinItem>> GetAddinListAsync(List<AddinItem> addinList = null)
        {
      
            if (addinList == null)
                addinList = await GetInitialAddinListAsync();

            if (addinList == null)
                return null;
      
            //foreach (var ai in addinList)

            Parallel.ForEach(addinList,
                new ParallelOptions {MaxDegreeOfParallelism = 20},
                ai =>                
                    {
                        try
                        {
                            // not using async here so we can wait for final list result          
                            // before returning
                            Debug.WriteLine(ai.gitVersionUrl);
                            var dl = HttpUtils.JsonRequest<AddinItem>(new HttpRequestSettings
                            {
                                Url = ai.gitVersionUrl
                            });
                            
                            DataUtils.CopyObjectData(dl, ai, "id,name,gitVersionUrl,gitUrl");

                            string addinFolder = mmApp.Configuration.AddinsFolder;

                            if (Directory.Exists(Path.Combine(addinFolder, ai.id)) ||
                                Directory.Exists(Path.Combine(addinFolder,"Install", ai.id)))
                            {
                                ai.isInstalled = true;                              
                            }

                            try
                            {
                                var versionFile = Path.Combine(addinFolder, ai.id, "version.json");
                                if (File.Exists(versionFile))
                                {
                                    var addinItem = JsonSerializationUtils.DeserializeFromFile(
                                            versionFile, typeof(AddinItem), false)
                                        as AddinItem;

                                    ai.installedVersion = addinItem.version;                                                                        
                                    if (addinItem != null && addinItem.version.CompareTo(ai.version) < 0)
                                    {
                                        ai.updateAvailable = true;                                        
                                    }
                                }
                            }
                            catch { }


                            if (File.Exists(".\\Addins\\Install\\" + ai.id + ".delete"))
                                ai.isInstalled = false;
                        }
                        catch (Exception ex)
                        {
                            mmApp.Log($"Addin {ai.name} version failed", ex);
                        }
                    });
            

            return addinList
                .Where(ai => ai.updated > new DateTime(2016, 1, 1))
                .OrderBy(ai => ai.isInstalled ? 0 : 1)  
                .ThenByDescending(ai => ai.updated)
                .ToList();
        }

        /// <summary>
        /// This downloads and installs a single addin to the Addins folder.
        /// Note the addin still needs to be in initialized with:
        /// OnApplicationStart() and InializeAddinUi()
        /// 
        /// The addin-loader then moves the files.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="targetFolder">Addins folder</param>
        /// <param name="addin"></param>
        /// <returns></returns>
        public DownloadAndInstallResult DownloadAndInstallAddin(string url, string targetFolder, AddinItem addin)
        {

            var result = new DownloadAndInstallResult() {ExistingAddin = false};

            if (string.IsNullOrEmpty(targetFolder))
            {
                ErrorMessage = "No target folder provided";
                result.IsError = true;
                return result;
            }

            string ver = mmApp.GetVersion();
            if (ver.CompareTo(addin.minVersion) < 0)
            {
                ErrorMessage = "This addin requires v" + addin.minVersion + " of Markdown Monster to run. You are on v" +  ver + ".\r\n\r\nPlease update to the latest version of Markdown Monster if you want to install this addin.";
                result.IsError = true;
                return result;
            }

            var addinName = Path.GetFileName(targetFolder);
            if (!Directory.Exists(Path.Combine(targetFolder, addin.id))) 
                targetFolder = Path.Combine(targetFolder, addin.id);
            else
            {
                result.ExistingAddin = true;
                targetFolder = Path.Combine(targetFolder, "Install", addin.id);
            }

            string file = Path.GetTempFileName();
            file = Path.ChangeExtension(file, "zip");

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, file);
                }
                if (Directory.Exists(targetFolder))
                    Directory.Delete(targetFolder, true);
                
                using (ZipArchive archive = ZipFile.OpenRead(file))
                {
                    foreach (ZipArchiveEntry zipfile in archive.Entries)
                    {
                        string fullName = Path.Combine(targetFolder, zipfile.FullName);

                        //Calculates what the new full path for the unzipped file should be
                        string fullPath = Path.GetDirectoryName(fullName);

                        //Creates the directory (if it doesn't exist) for the new path
                        Directory.CreateDirectory(fullPath);

                        //Extracts the file to (potentially new) path
                        zipfile.ExtractToFile(fullName, true);                    
                    }
                }

                
                return result;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
                result.IsError = true;
                return result;
            }

        }

        public class DownloadAndInstallResult
        {
            public bool IsError = false;
            public bool ExistingAddin = true;

        }

        public bool UninstallAddin(string addinId, string addinPath = null)
        {
            if (string.IsNullOrEmpty(addinPath))
                addinPath = mmApp.Configuration.AddinsFolder;

            var directory = Directory.GetDirectories(addinPath).FirstOrDefault(dir => Path.GetFileName(dir) == addinId);
            if (!string.IsNullOrEmpty(directory))
            {
                if (!Directory.Exists(Path.Combine(addinPath, "Install")))
                    Directory.CreateDirectory(Path.Combine(addinPath, "Install"));

                File.WriteAllText(Path.Combine(addinPath,"Install", addinId + ".delete"),"to be deleted");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Installs pending Addins from the Install folder into the Addins folder
        /// This is required because addins can be already loaded and can't be copied
        /// over.
        /// </summary>
        /// <param name="path">Temporary install path</param>
        public bool InstallAddinFiles(string path = ".\\Addins\\Install")
        {
            string dirName = null;

            try
            {
                // delete addins flagged for deletion
                var files = Directory.GetFiles(path, "*.delete");
                foreach (var file in files)
                {
                    var deleteFolder = Path.Combine(path + "..\\", Path.GetFileNameWithoutExtension(file));
                    if (Directory.Exists(deleteFolder))
                        Directory.Delete(deleteFolder, true);

                    File.Delete(file);
                }

                // install new addins
                var dirs = Directory.GetDirectories(path);
                foreach (var addinInstallFolder in dirs)
                {
                    dirName = Path.GetFileName(addinInstallFolder); // folder name
                    var addinPath = Path.Combine(addinInstallFolder, "..\\..", dirName);
                    if (Directory.Exists(addinPath))
                        Directory.Delete(addinPath, true);

                    Directory.Move(addinInstallFolder, addinPath);
                }

                Directory.Delete(path);
            }
            catch (Exception ex)
            {
                mmApp.Log("Addin installation failed for " + dirName, ex);
                return false;
            }

            

            return true;
        }

        public IEnumerable<MarkdownMonsterAddin> GetMarkDownParserAddins()
        {
            return AddIns.Where(ai => ai.GetMarkdownParser() != null);
        }
        #endregion
    }
}
