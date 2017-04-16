using System;
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
        /// If you want this addin to display on the toolbar choose a FontAwesome icon
        /// for the addin. Anything other than Fontawesome.None will render a toolbar
        /// button wiht this icon. If .none is specified no toolbar button is rendered 
        /// (only the addin menu option).        
        /// </summary>
        public FontAwesomeIcon FontawesomeIcon { get; set; } = FontAwesomeIcon.None;


        /// <summary>
        /// Optional Pack Image Icon resource string:
        /// Example syntax: "pack://application:,,,/PandocMarkdownParserAddin;component/icon.png"
        ///                 "pack://application:,,,/assemblyName;component/relativeImagePath.ext"
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
    }
}