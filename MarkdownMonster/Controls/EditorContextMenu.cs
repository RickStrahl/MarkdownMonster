using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using MarkdownMonster.Windows;
using Westwind.Utilities;

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
        public void ShowSpellcheckSuggestions(IEnumerable<string> suggestions, object range)
        {
            if (suggestions == null)
                return;

            ClearMenu();
            var model = Model;

            foreach (var sg in suggestions)
            {
                var mi = new MenuItem
                {
                    Header = sg,
                    FontWeight = FontWeights.Bold
                };
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
            AddEditorContext();
            AddCopyPaste();

            Show();
        }

        /// <summary>
        /// Adds Copy/Cut/Paste Context menu options
        /// </summary>
        public void AddCopyPaste()
        {
            var selText = Model.ActiveEditor?.AceEditor?.getselection(false);
            var model = Model;
            
            var miCut = new MenuItem { Header = "Cut" };
            miCut.Click += (o, args) => model.ActiveEditor.SetSelection("");
            ContextMenu.Items.Add(miCut);

            var miCopy = new MenuItem() {Header = "Copy"};
            miCopy.Click += (o, args) => Clipboard.SetText(selText);
            ContextMenu.Items.Add(miCopy);

            var miCopyHtml = new MenuItem() { Header = "Copy As Html" };          
            miCopyHtml.Command = Model.CopyAsHtmlCommand;            
            ContextMenu.Items.Add(miCopyHtml);
   
            var miPaste = new MenuItem() {Header = "Paste"};
            miPaste.Click += (o, args) => model.ActiveEditor?.SetSelection(Clipboard.GetText());
            ContextMenu.Items.Add(miPaste);

            if (string.IsNullOrEmpty(selText))
            {
                miCopy.IsEnabled = false;
                miCopyHtml.IsEnabled = false;
                miCut.IsEnabled = false;
            }
            if (Model.ActiveEditor?.EditorSyntax != "markdown")
                miCopyHtml.IsEnabled = false;

            if (!Clipboard.ContainsText())
                miPaste.IsEnabled = false;

            if (ContextMenu.Items.Count > 0)
                ContextMenu.Items.Add(new Separator());
        }

        public void AddEditorContext()
        {
            var model = Model;
            var line = Model.ActiveEditor.GetCurrentLine();

            var pos = Model.ActiveEditor.GetCursorPosition();
            if (pos.row == -1)
                return;

            // Check for images ![](imageUrl)
            var matches = Regex.Matches(line, @"!\[.*?\]\(.*?\)", RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    string val = match.Value;
                    if (match.Index <= pos.column && match.Index + val.Length > pos.column)
                    {
                        var mi = new MenuItem
                        {
                            Header = "Edit in Image Editor"
                        };
                        mi.Click += (o, args) =>
                        {
                            var image = StringUtils.ExtractString(val, "(", ")");
                            image = mmFileUtils.NormalizeFilename(image,
                                Path.GetDirectoryName(model.ActiveDocument.Filename));
                            mmFileUtils.OpenImageInImageEditor(image);
                        };
                        ContextMenu.Items.Add(mi);

                        var mi2 = new MenuItem
                        {
                            Header = "Edit Image Link"
                        };
                        mi2.Click += (o, args) =>
                        {
                            Model.ActiveEditor.AceEditor.SetSelectionRange(pos.row, match.Index, pos.row,
                                match.Index + val.Length, pos);
                            Model.ActiveEditor.EditorSelectionOperation("image", val);
                        };
                        ContextMenu.Items.Add(mi2);
                    }

                    if (ContextMenu.Items.Count > 0)
                        ContextMenu.Items.Add(new Separator());

                    return;
                }
            }

        }


    }
}