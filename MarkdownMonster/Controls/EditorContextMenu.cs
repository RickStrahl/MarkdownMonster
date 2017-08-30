using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MarkdownMonster
{
    /// <summary>
    /// Class that handles display and execution of the editors
    /// context menu.
    /// </summary>
    public class EditorContextMenu
    {
        private ContextMenu ContextMenu;
        private AppModel Model;

        public EditorContextMenu()
        {
            Model = mmApp.Model;
            ContextMenu = Model.Window.EditorContextMenu;
        }

        /// <summary>
        /// Clears all items off the menu
        /// </summary>
        public void ClearMenu()
        {
            if (ContextMenu != null)
            {
                MenuItem t;
                ContextMenu.Items.Clear();
            }
        }

        public void Show()
        {
            if (ContextMenu != null)
            {
                ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                ContextMenu.PlacementTarget = Model.Window;
                ContextMenu.IsOpen = true;
            }
        }

        /// <summary>
        /// Adds spell check words to the context menu.
        /// </summary>
        /// <param name="suggestions"></param>
        /// <param name="range"></param>
        public void ShowSpellcheckWords(IEnumerable<string> suggestions, object range)
        {
            if (suggestions == null)
                return;

            ClearMenu();
            var model = Model;

            foreach (var sg in suggestions)
            {
                var mi = new MenuItem { Header = sg };
                mi.Click += (o, args) => model.ActiveEditor.AceEditor.replaceSpellRange(range, sg);
                ContextMenu.Items.Add(mi);
            }

            ContextMenu.Items.Add(new Separator());
            var mi2 = new MenuItem()
            {
                Header = "Add to dictionary",
                HorizontalContentAlignment = HorizontalAlignment.Right
            };
            mi2.Click += (o, args) =>
            {
                model.ActiveEditor.AceEditor.addWordSpelling(((dynamic) range).misspelled);
            };
            ContextMenu.Items.Add(mi2);

            ContextMenu.Items.Add(new Separator());
            AddCopyPaste();

            Show();
        }

        public void AddCopyPaste()
        {
            var selText = Model.ActiveEditor?.AceEditor?.getselection(false);
            var model = Model;

            var miCopy = new MenuItem() { Header = "Copy" };
            miCopy.Click += (o, args) => Clipboard.SetText(selText);
            ContextMenu.Items.Add(miCopy);

            var miCut = new MenuItem { Header = "Cut" };
            miCut.Click += (o, args) => model.ActiveEditor.SetSelection("");
            ContextMenu.Items.Add(miCut);

            var miPaste = new MenuItem() { Header = "Paste" };
            miPaste.Click += (o, args) => model.ActiveEditor?.SetSelection(Clipboard.GetText());
            ContextMenu.Items.Add(miPaste);

            if (string.IsNullOrEmpty(selText))
            {
                miCopy.IsEnabled = false;
                miCut.IsEnabled = false;
            }

            if (!Clipboard.ContainsText())
                miPaste.IsEnabled = false;

            if (ContextMenu.Items.Count > 0)
                ContextMenu.Items.Add(new Separator());
        }
    }
}