using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FontAwesome.WPF;

namespace MarkdownMonster.AddIns
{
    public class AddinManager
    {
        public static AddinManager Current { get; set; }

        public List<IMarkdownMonsterAddin> AddIns;
        
        static AddinManager()
        {
            Current = new AddinManager();
        }

        public AddinManager()
        {
            AddIns = new List<IMarkdownMonsterAddin>();
        }

        public void LoadAddins()
        {
            string addinPath = Path.Combine(Environment.CurrentDirectory, "AddIns");
            if (!Directory.Exists(addinPath))
                return;
            
            var files = Directory.GetFiles(addinPath,"*.dll");
            foreach (var file in files)
            {
                LoadAddinClasses(file);
            }
        }



        private void LoadAddinClasses(string assemblyFile)
        {

            Assembly asm = null;
            Type[] types = null;
            try
            {
                asm = Assembly.LoadFile(assemblyFile);
                types = asm.GetTypes();
            }
            catch(Exception ex)
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
                        var titem = new Button();
                        titem.Content = new Image()
                        {
                            Source =
                                ImageAwesome.CreateImageSource(menuItem.FontawesomeIcon, addin.Model.Window.Foreground),
                            ToolTip = menuItem.Caption,
                            Height = 16,
                            Width = 16,
                            Margin = new Thickness(5, 0, 5, 0)
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
