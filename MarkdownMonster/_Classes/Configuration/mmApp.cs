using System;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace MarkdownMonster
{
    public class mmApp
    {
        public static ApplicationConfiguration Configuration { get; set;  }

        public static string ApplicationName { get; set; } = "Markdown Monster";


        static mmApp()
        {
            Configuration = new ApplicationConfiguration();
            Configuration.Initialize();
        }

        public static void SetTheme(Themes theme = Themes.Default,MetroWindow window = null)
        {
            if (theme == Themes.Default)
                theme = mmApp.Configuration.ApplicationTheme;

            if (theme == Themes.Light)
            {
                // get the current app style (theme and accent) from the application
                // you can then use the current theme and custom accent instead set a new theme
                Tuple<AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);

                // now set the Green accent and dark theme
                ThemeManager.ChangeAppStyle(Application.Current,
                    ThemeManager.GetAccent("Blue"),
                    ThemeManager.GetAppTheme("BaseLight")); // or appStyle.Item1                
            }
            else
            {
                // get the current app style (theme and accent) from the application
                // you can then use the current theme and custom accent instead set a new theme
                Tuple<AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);

                // now set the highlight accent and dark theme
                ThemeManager.ChangeAppStyle(Application.Current,
                    ThemeManager.GetAccent("Blue"),
                    ThemeManager.GetAppTheme("BaseDark")); // or appStyle.Item1      
                                
            }

            if (window != null)
                SetThemeWindowOverride(window);            

        }

        public static void SetThemeWindowOverride(MetroWindow window)
        {
            if (mmApp.Configuration.ApplicationTheme == Themes.Dark)
            {
                if (window != null)
                {
                    window.WindowTitleBrush = (SolidColorBrush) (new BrushConverter().ConvertFrom("#333333"));
                    window.NonActiveWindowTitleBrush = (Brush) window.FindResource("WhiteBrush");

                    var brush = App.Current.Resources["MenuSeparatorBorderBrush"] as SolidColorBrush;
                    App.Current.Resources["MenuSeparatorBorderBrush"] = (SolidColorBrush) new BrushConverter().ConvertFrom("#333333");
                    brush = App.Current.Resources["MenuSeparatorBorderBrush"] as SolidColorBrush;
                }
            }
            else
            {
                if (window != null)
                {
                    // Need to fix this to show the accent color when switching
                    //window.WindowTitleBrush = (Brush)window.FindResource("WhiteBrush");
                    //window.NonActiveWindowTitleBrush = (Brush)window.FindResource("WhiteBrush");
                }
            }
        }


        internal static string EncryptionMachineKey { get; } = "42331333#1Ae@rTo*dOO-002" + Environment.MachineName;
        internal static string ProKey { get; } = "Kupua_100";
        public static string InstallerDownloadUrl { get; internal set; }
        public static string UpdateCheckUrl { get; internal set; }
    }

    public enum Themes
    {
        Dark,
        Light,
        Default
    }

}