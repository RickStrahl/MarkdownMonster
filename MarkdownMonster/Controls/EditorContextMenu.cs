using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            ContextMenu.Closed += ContextMenu_Closed;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            Model.ActiveEditor?.SetEditorFocus();
        }

        /// <summary>
        /// Clears all items off the menu
        /// </summary>
        public void ClearMenu()
        {
            ContextMenu?.Items.Clear();
        }

        public void Show()
        {
            
            if (ContextMenu != null)
            {
                Model.ActiveEditor.SetMarkdownMonsterWindowFocus();

                ContextMenu.PlacementTarget = Model.ActiveEditor.WebBrowser;
                ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;

                ContextMenu.Focus();
                ContextMenu.IsOpen = true;
                
                var item = ContextMenu.Items[0] as MenuItem;
                item.Focus();                
            }
        }

        public void ShowContextMenuAll(IEnumerable<string> suggestions, object range)
        {
            ClearMenu();
            var model = Model;

            SpellcheckSuggestions(suggestions, range);
            AddEditorContext();
            AddCopyPaste();

            Show();
        }


        /// <summary>
        /// Adds spell check words to the context menu.
        /// </summary>
        /// <param name="suggestions"></param>
        /// <param name="range"></param>
        public void SpellcheckSuggestions(IEnumerable<string> suggestions, object range)
        {
            if (suggestions == null || range == null)
                return;

            var model = Model;

            bool hasSuggestions = false;
                foreach (var sg in suggestions)
                {
                    var mi = new MenuItem
                    {
                        Header = sg,
                        FontWeight = FontWeights.Bold
                    };
                    mi.Click += (o, args) => model.ActiveEditor.AceEditor.replaceSpellRange(range, sg);
                    ContextMenu.Items.Add(mi);
                    hasSuggestions = true;
                }

            if (hasSuggestions)
            {
                ContextMenu.Items.Add(new Separator());
                var mi2 = new MenuItem()
                {
                    Header = "Add to dictionary",
                    HorizontalContentAlignment = HorizontalAlignment.Right
                };
                mi2.Click += (o, args) => model.ActiveEditor.AceEditor.addWordSpelling(((dynamic)range).misspelled);                
                ContextMenu.Items.Add(mi2);
                ContextMenu.Items.Add(new Separator());
            }
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

        /// <summary>
        /// Adds context sensitive links for Image Links, Hyper Links
        /// </summary>
        public void AddEditorContext()
        {
            var model = Model;
            var line = Model.ActiveEditor.GetCurrentLine();

            var pos = Model.ActiveEditor.GetCursorPosition();
            if (pos.row == -1)
                return;

            CheckForImageLink(line, pos);
            CheckForHyperLink(line, pos);


            if (ContextMenu.Items.Count > 0)            
                ContextMenu.Items.Add(new Separator());                
            
        }


        static readonly Regex HrefRegex = new Regex(@"(?<!\!)\[.*?\]\(.*?\)", RegexOptions.IgnoreCase);

        private bool CheckForHyperLink(string line, AcePosition pos)
        {
            var matches = HrefRegex.Matches(line);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    string val = match.Value;
                    if (match.Index <= pos.column && match.Index + val.Length > pos.column)
                    {                     
                        var mi2 = new MenuItem
                        {
                            Header = "Edit Hyperlink"
                        };
                        mi2.Click += (o, args) =>
                        {
                            Model.ActiveEditor.AceEditor.SetSelectionRange(pos.row, match.Index, pos.row,
                                match.Index + val.Length, pos);
                            Model.ActiveEditor.EditorSelectionOperation("hyperlink", val);
                        };
                        ContextMenu.Items.Add(mi2);

                        var mi = new MenuItem
                        {
                            Header = "Remove Hyperlink"
                        };
                        mi.Click += (o, args) =>
                        {
                            Model.ActiveEditor.AceEditor.SetSelectionRange(pos.row, match.Index, pos.row,
                                match.Index + val.Length, pos);
                            string text = StringUtils.ExtractString(val, "[", "]");
                            Model.ActiveEditor.SetSelection(text);
                        };
                        ContextMenu.Items.Add(mi);

                        return true;
                    }
                }
            }
            return false;
        }

        static readonly Regex ImageRegex = new Regex(@"!\[.*?\]\(.*?\)", RegexOptions.IgnoreCase);

        private bool CheckForImageLink(string line, AcePosition pos)
        {   
            // Check for images ![](imageUrl)
            var matches = ImageRegex.Matches(line);
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
                                Path.GetDirectoryName(Model.ActiveDocument.Filename));
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

                        return true;
                    }

                }
            }

            return false;
        }
    }
}