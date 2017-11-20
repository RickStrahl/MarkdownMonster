using System;
using System.Windows.Controls;
using System.Windows.Media;
using FontAwesome.WPF;

namespace MarkdownMonster.AddIns
{
    /// <summary>
    /// Information about an Addin's menu item that
    /// is displayed on the Addin toolbar and the button
    /// displayed on the Addins installed menu.
    /// </summary>
    public class AddInMenuItem
    {
        
        /// <summary>
        /// By default hook up Execute/ExecuteConfiguration/CanExecute
        /// handlers to the On Handlers in the Addin class.
        /// </summary>
        /// <param name="addin"></param>
        public AddInMenuItem(MarkdownMonsterAddin addin = null)
        {            
            if (addin != null)
            {
                Execute = addin.OnExecute;
                ExecuteConfiguration = addin.OnExecuteConfiguration;
                CanExecute = addin.OnCanExecute;
            }
        }

        /// <summary>
        /// The display name for the Addin on the menu
        /// </summary>
        public string Caption { get; set; }


        /// <summary>
        /// Menu Item name that you can address to find the menu item and manipulate it
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If you want this addin to display on the toolbar choose a FontAwesome icon
        /// for the addin. Anything other than Fontawesome.None will render a toolbar
        /// button wiht this icon. If .none is specified no toolbar button is rendered 
        /// (only the addin menu option).        
        /// </summary>
        public FontAwesomeIcon FontawesomeIcon { get; set; } = FontAwesomeIcon.None;

        /// <summary>
        /// Optional color of the Font AwesomeIcon to use other than the default window
        /// foreground color (default).
        /// </summary>
        public string FontawesomeIconColor { get; set; }


        /// <summary>
        /// An optional ImageSource you can bind as the toolbar icon. You can use **any** image source, but in most
        /// cases you are likely to bind to an ImageSource of an internal image resource contained in your addin's assembly.
        ///
        /// To do this you can use code like the following:
        /// ```cs
        /// menuItem.IconImageSource = 
        ///    new ImageSourceConverter()
        ///        .ConvertFromString("pack://application:,,,/PanDocMarkdownParserAddin;component/icon_22.png") 
        ///         as ImageSource
        /// ```
        ///
        ///  You can also use a customize font awesome icon like this:
        /// 
        /// 
        /// The pack string is in the format of:
        ///
        /// **"pack://application:,,,/assemblyName;component/relativeImagePath.ext"**
        ///
        /// Note the **component/** which is the application root so make sure you start your project relative path from there.
        /// </summary>
        public ImageSource IconImageSource { get; set;  }

        /// <summary>
        /// An optional keyboard shortcut in the 
        /// 
        /// format of Shift+Alt-H, F7, Alt-F1 etc.                
        /// </summary>
        public string KeyboardShortcut { get; set; }

        /// <summary>
        /// Event implementation that passes the button or menu item
        /// that is clicked.
        /// </summary>
        public Action<object> Execute { get; set; }

        /// <summary>
        /// Event implementation fired when the 'Configure' option is activated
        /// Use this to display the configuration API
        /// </summary>
        public Action<object> ExecuteConfiguration { get; set; }

        /// <summary>
        /// Check activation that passes the button or menu item
        /// that is clicked.
        /// </summary>
        public Func<object, bool>  CanExecute { get; set;  }

        /// <summary>
        /// Internally created Command object that hooks up Execute and CanExecute
        /// </summary>
        public CommandBase Command { get; set; }

        /// <summary>
        /// The actual WPF Menu item that was created and you can reference after
        /// creation. Make sure to check for null. Available only after Window has initialized
        /// in OnWindowLoaded() or after.
        /// </summary>
        public Button MenuItemButton { get; set; }

        /// <summary>
        /// The actual WPF Menu item that was created and you can reference after
        /// creation. Make sure to check for null. Available only after Window has initialized
        /// in OnWindowLoaded() or after.
        /// </summary>
        public MenuItem ConfigurationMenuItem { get; set; }
    }
}
