using System;
using FontAwesome.WPF;

namespace MarkdownMonster.AddIns
{
    public class AddInMenuItem
    {
        
        public AddInMenuItem(MarkdownMonsterAddin addin = null)
        {            
            if (addin != null)
            {
                Execute = addin.OnExecute;
                ExecuteConfiguration = addin.OnExecuteConfiguration;
                CanExecute = addin.OnCanExecute;
            }
        }

        public string Caption { get; set; }

        /// <summary>
        /// If you want this addin to display on the toolbar choose a FontAwesome icon
        /// for the addin. Anything other than Fontawesome.None will render a toolbar
        /// button wiht this icon. If .none is specified no toolbar button is rendered 
        /// (only the addin menu option).        
        /// </summary>
        public FontAwesomeIcon FontawesomeIcon { get; set; } = FontAwesomeIcon.None;

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