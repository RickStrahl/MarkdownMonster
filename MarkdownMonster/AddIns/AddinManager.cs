using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using FontAwesome.WPF;

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
        public List<IMarkdownMonsterAddin> AddIns;
        
        static AddinManager()
        {
            Current = new AddinManager();
        }

        public AddinManager()
        {
            AddIns = new List<IMarkdownMonsterAddin>();
        }

        /// <summary>
        /// Loads add-ins into the application from the add-ins folder
        /// </summary>
        public void LoadAddins()
        {
            string addinPath = Path.Combine(Environment.CurrentDirectory, "AddIns");
            if (!Directory.Exists(addinPath))
                return;

            // we need to make sure already loaded dependencies are not loaded again
            // when probing for add-ins
            var assemblyFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.dll");

            var files = Directory.GetFiles(addinPath, "*.dll");
           
            foreach (var file in files)
            {
                // don't allow assemblies the main app loads to load
                string fname = Path.GetFileName(file).ToLower();
                bool isLoaded = assemblyFiles.Any(f => fname == Path.GetFileName(f).ToLower());

                if (!isLoaded)
                {                    
                    LoadAddinClasses(file);
                    Trace.WriteLine("Loaded add-ins from: " + file);
                }
            }
        }
        
        /// <summary>
        /// Load all add in classes in an assembly
        /// </summary>
        /// <param name="assemblyFile"></param>
        private void LoadAddinClasses(string assemblyFile)
        {

            Assembly asm = null;
            Type[] types = null;

            try
            {
                asm = Assembly.LoadFile(assemblyFile);
                types = asm.GetTypes();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load add-in assembly: " + Path.GetFileNameWithoutExtension(assemblyFile));
                return;
            }

            foreach (var type in types)
            {
                var typeList = type.FindInterfaces(AddinInterfaceFilter, typeof(IMarkdownMonsterAddin));
                if (typeList.Length > 0)
                {
                    var ai = Activator.CreateInstance(type) as IMarkdownMonsterAddin;
                    this.AddIns.Add(ai);
                }
            }
        }


        private static bool AddinInterfaceFilter(Type typeObj, Object criteriaObj)
        {
            if (typeObj.ToString() == criteriaObj.ToString())
                return true;
            else
                return false;
        }


        /// <summary>
        /// Loads the add-in menu and toolbar buttons
        /// </summary>
        /// <param name="window"></param>
        public void InitializeAddinsUi(MainWindow window)
        {
            foreach (var addin in AddIns)
            {
                addin.Model = window.Model;

                
                foreach (var menuItem in addin.MenuItems)
                {
                    var mitem = new MenuItem()
                    {
                        Header = menuItem.Caption
                        
                    };
                    mitem.Click += (sender, e) =>
                    {
                        if (menuItem.CanExecute != null &&
                            !menuItem.CanExecute.Invoke(mitem))
                            return;

                        menuItem.Execute?.Invoke(mitem);
                    };
                    addin.Model.Window.MenuAddins.Items.Add(mitem);
                    
                    // if an icon is provide also add to toolbar
                    if (menuItem.FontawesomeIcon != FontAwesomeIcon.None)
                    {
                        var hasConfigMenu = menuItem.ExecuteConfiguration != null;

                        var titem = new Button();
                        titem.Content = new Image()
                        {
                            Source =
                                ImageAwesome.CreateImageSource(menuItem.FontawesomeIcon, addin.Model.Window.Foreground),
                            ToolTip = menuItem.Caption,
                            Height = 16,
                            Width = 16,
                            Margin = new Thickness(5, 0, hasConfigMenu ? 0 : 5, 0)
                        };


                        titem.Click += (sender, e) =>
                        {
                            if (menuItem.CanExecute != null &&
                                !menuItem.CanExecute.Invoke(titem))
                                return;

                            menuItem.Execute?.Invoke(titem);
                        };

                        addin.Model.Window.ToolbarAddIns.Visibility = System.Windows.Visibility.Visible;
                        addin.Model.Window.ToolbarAddIns.Items.Add(titem);

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
                                    ToolTip = menuItem.Caption + " Configuration",
                                    Height = 16,
                                    Width = 8,                                                                                                            
                                    Margin = new Thickness(0, 0, 0, 0),                                    
                                }
                            };                            
                            tcitem.Click += (sender, e) =>
                            {
                                menuItem.ExecuteConfiguration?.Invoke(sender);
                            };
                            addin.Model.Window.ToolbarAddIns.Items.Add(tcitem);
                        }
                    }
                }
            }
        }

        public void RaiseOnApplicationStart()
        {
            foreach (var addin in AddIns)
                addin?.OnApplicationStart();
        }

        public void RaiseOnApplicationShutdown()
        {
            foreach (var addin in AddIns)
                addin?.OnApplicationShutdown();
        }
        
    }
}
