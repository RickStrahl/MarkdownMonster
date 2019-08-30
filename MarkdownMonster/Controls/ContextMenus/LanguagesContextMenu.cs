using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MarkdownMonster.Utilities;
using MarkdownMonster.Windows;

namespace MarkdownMonster.Controls.ContextMenus
{

    /// <summary>
    /// Pops a language drop down from the Control Box menu
    /// </summary>
    public class LanguagesContextMenu
    {
        private MainWindow Window { get; }


        public LanguagesContextMenu(MainWindow window)
        {
            Window = window;
        }

        public void OpenContextMenu()
        {
            var Model = Window.Model;
            var ctx = new ContextMenu();

            foreach (var lang in SpellChecker.DictionaryDownloads)
            {
                var fname = Path.Combine(SpellChecker.InternalDictionaryFolder, lang.Code + ".dic");
                bool exists = File.Exists(fname);
                if (!exists)
                {
                    fname = Path.Combine(SpellChecker.ExternalDictionaryFolder, lang.Code + ".dic");
                    exists = File.Exists(fname);
                }

                string header = lang.Name;
                if (!exists)
                    header = header + " ↆ";

                var menuItem = new MenuItem()
                {
                    Header = header,
                    Tag = fname,
                    Command = Model.Commands.SetDictionaryCommand,
                    CommandParameter = lang.Code
                };
                if (lang.Code.Equals(Model.Configuration.Editor.Dictionary, StringComparison.InvariantCultureIgnoreCase))
                {
                    menuItem.IsCheckable = true;
                    menuItem.IsChecked = true;
                }

                ctx.Items.Add(menuItem);
            }

            ctx.Items.Add(new Separator());
            ctx.Items.Add(new MenuItem
            {
                Header = "Remove downloaded Dictionaries",
                Command = Model.Commands.SetDictionaryCommand,
                CommandParameter = "REMOVE-DICTIONARIES"
            });

            ctx.MaxHeight = 800;
            ctx.IsOpen = true;
            WindowUtilities.DoEvents();
        }
    }
}
