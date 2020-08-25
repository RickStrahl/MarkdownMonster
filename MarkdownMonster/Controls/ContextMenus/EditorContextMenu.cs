using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Markdig.Parsers;
using MarkdownMonster.Windows;
using MarkdownMonster.Windows.DocumentOutlineSidebar;
using Microsoft.Win32;
using Westwind.Utilities;

namespace MarkdownMonster.Controls.ContextMenus
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
            ContextMenu = new ContextMenu();
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

            bool hasSuggestions = SpellcheckSuggestions(suggestions, range);
            AddEditorContext();

            AddUndoRedo();
            AddCopyPaste();

            // if we dont' have suggestions show the tab context menu
            if (!hasSuggestions)
            {
                ContextMenu.Items.Add(new Separator());
                var tabMenu = new TabContextMenu();
                tabMenu.AddContextMenuItems(ContextMenu);
            }

            Show();
        }


        /// <summary>
        /// Adds spell check words to the context menu.
        /// </summary>
        /// <param name="suggestions"></param>
        /// <param name="range"></param>
        public bool SpellcheckSuggestions(IEnumerable<string> suggestions, object range)
        {
            if (suggestions == null || range == null || range == DBNull.Value)
                return false;

            var model = Model;

            bool hasSuggestions = false;
            foreach (var sg in suggestions)
            {
                var mi = new MenuItem
                {
                    Header = sg,
                    FontWeight = FontWeights.Bold
                };
                mi.Click += (o, args) =>
                {
                    model.ActiveEditor.AceEditor.Invoke("replaceSpellRange",range, sg);
                    model.ActiveEditor.IsDirty();
                    model.ActiveEditor.SpellCheckDocument();
                };
                ContextMenu.Items.Add(mi);
                hasSuggestions = true;
            }

            if (hasSuggestions)
                ContextMenu.Items.Add(new Separator());

            var mi2 = new MenuItem()
            {
                Header = "Add to dictionary",
                HorizontalContentAlignment = HorizontalAlignment.Right
            };


            mi2.Click += (o, args) =>
            {
                if (range == DBNull.Value)
                {
                    model.Window.ShowStatusError("No misspelled word selected. Word wasn't added to dictionary.");
                    return;
                }

                var text = model.ActiveEditor.AceEditor.Get(range, "misspelled") as string;
                model.ActiveEditor.AddWordToDictionary(text);
                model.ActiveEditor.SpellCheckDocument();
                model.Window.ShowStatus("Word added to dictionary.", mmApp.Configuration.StatusMessageTimeout);
            };
            ContextMenu.Items.Add(mi2);
            ContextMenu.Items.Add(new Separator());

            return hasSuggestions;

        }

        /// <summary>
        /// Adds Copy/Cut/Paste Context menu options
        /// </summary>
        public void AddCopyPaste()
        {
            var selText = Model.ActiveEditor?.AceEditor?.GetSelection();
            var model = Model;

            var miCut = new MenuItem { Header = "Cut", InputGestureText="Ctrl-X" };
            miCut.Click += (o, args) => model.ActiveEditor.SetSelection("");
            ContextMenu.Items.Add(miCut);

            var miCopy = new MenuItem() { Header = "Copy", InputGestureText="Ctrl-C" };
            miCopy.Click += (o, args) =>ClipboardHelper.SetText(selText, true);
            ContextMenu.Items.Add(miCopy);

            var miCopyHtml = new MenuItem() { Header = "Copy As Html", InputGestureText=Model.Commands.CopyAsHtmlCommand.KeyboardShortcut };
            miCopyHtml.Command = Model.Commands.CopyAsHtmlCommand;
            ContextMenu.Items.Add(miCopyHtml);

            bool hasClipboardData = false;
            var miPaste = new MenuItem() { Header = "Paste", InputGestureText = "Ctrl-V" };
            if (ClipboardHelper.ContainsImage())
            {
                hasClipboardData = true;
                miPaste.Header = "Paste Image";
                miPaste.Click += (o, args) => model.ActiveEditor?.PasteOperation();
            }
            else
            {
                hasClipboardData = ClipboardHelper.ContainsText();
                miPaste.Click += (s,e) => Model.ActiveEditor.PasteOperation();
            }

            miPaste.IsEnabled = hasClipboardData;

            ContextMenu.Items.Add(miPaste);

            if (string.IsNullOrEmpty(selText))
            {
                miCopy.IsEnabled = false;
                miCopyHtml.IsEnabled = false;
                miCut.IsEnabled = false;
            }
            else {
                if (Model.ActiveEditor?.MarkdownDocument?.EditorSyntax != "markdown")
                    miCopyHtml.IsEnabled = false;
                else
                {
                    ContextMenu.Items.Add(new Separator());
                    var miRemoveFormatting = new MenuItem
                    {
                        Header = "Remove Markdown Formatting",
                        InputGestureText = Model.Commands.RemoveMarkdownFormattingCommand.KeyboardShortcut
                    };
                    miRemoveFormatting.Command = Model.Commands.RemoveMarkdownFormattingCommand;
                    ContextMenu.Items.Add(miRemoveFormatting);
                }
            }

            AddSpeech();
        }

        public void AddUndoRedo()
        {

            var editor = Model.ActiveEditor;

            bool hasUndo = editor.AceEditor.HasUndo();
            bool hasRedo = editor.AceEditor.HasRedo();

            var miUndo = new MenuItem { Header = "Undo", InputGestureText = "Ctrl-Z" };
            if (!hasUndo)
                miUndo.IsEnabled = false;
            miUndo.Click += (o, args) => Model.ActiveEditor.AceEditor.Undo();
            ContextMenu.Items.Add(miUndo);

            var miRedo = new MenuItem() { Header = "Redo", InputGestureText = "Ctrl-Y" };
            if (!hasRedo)
                miRedo.IsEnabled = false;
            miRedo.Click += (o, args) => Model.ActiveEditor.AceEditor.Redo();
            ContextMenu.Items.Add(miRedo);

            ContextMenu.Items.Add(new Separator());
        }

        public void AddSpeech()
        {
            var hasDocumentSelection = !string.IsNullOrEmpty(Model.ActiveEditor?.AceEditor?.GetSelection());
            var hasClipText = Clipboard.ContainsText();

            var mi = new MenuItem() { Header = "Speak" };
            ContextMenu.Items.Add(mi);

            
            var   mi2 = new MenuItem()
                {
                    Header = "Speak _Selection", Command = Model.Commands.Speech.SpeakSelectionCommand,
                    IsEnabled = hasDocumentSelection
                };
                mi.Items.Add(mi2);
            

            mi2 = new MenuItem()
            {
                Header = "Speak _Document",
                Command = Model.Commands.Speech.SpeakDocumentCommand
            };
            mi.Items.Add(mi2);


            mi2 = new MenuItem()
            {
                Header = "Speak Text from _Clipboard",
                Command = Model.Commands.Speech.SpeakFromClipboardCommand,
                IsEnabled = hasClipText
            };
            mi.Items.Add(mi2);

            mi.Items.Add(new Separator());

            mi2 = new MenuItem()
            {
                Header = "Cancel Speaking",
                Command = Model.Commands.Speech.CancelSpeakCommand
            };
            mi.Items.Add(mi2);
        }

        /// <summary>
        /// Adds context sensitive links for Image Links, Hyper Links
        /// </summary>
        public void AddEditorContext()
        {
            var model = Model;
            var lineText = Model.ActiveEditor?.GetCurrentLine();

            var pos = Model.ActiveEditor.GetCursorPosition();
            if (pos.row == -1)
                return;

            var contextCount = ContextMenu.Items.Count;

            if (!string.IsNullOrEmpty(lineText))
            {
                CheckForImageLink(lineText, pos);
                CheckForHyperLink(lineText, pos);
                CheckForTable(lineText, pos);
            }

            if (ContextMenu.Items.Count > contextCount)
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
                        var url = StringUtils.ExtractString(val, "](", ")");
                        var editorSyntax = mmFileUtils.GetEditorSyntaxFromFileType(url);

                        MenuItem mi2;
                        if (url.Contains("(http"))
                        {
                            mi2 = new MenuItem {Header = "Navigate Hyperlink"};
                            mi2.Click += (o, args) =>
                            {
                                if (!string.IsNullOrEmpty(url))
                                {
                                    try
                                    {
                                        ShellUtils.GoUrl(url);
                                    }
                                    catch
                                    {
                                        Model.Window.ShowStatusError("Invalid link: Couldn't navigate to " + url);
                                    }
                                }
                            }; 
                            ContextMenu.Items.Add(mi2);
                        }
                        else if (!string.IsNullOrEmpty(editorSyntax))
                        {
                            mi2 = new MenuItem { Header = "Open Document in new Tab" };
                            mi2.Click += (o, args) =>
                            {
                                if (!string.IsNullOrEmpty(url))
                                {
                                    if (!url.Contains(":/"))
                                        url = Path.Combine(Path.GetDirectoryName(Model.ActiveDocument.Filename), url);

                                    try
                                    {
                                        Model.Window.ActivateTab(url,openIfNotFound: true);
                                    }
                                    catch
                                    {
                                        Model.Window.ShowStatusError("Invalid link: Couldn't open " + url);
                                    }
                                }
                            };
                            ContextMenu.Items.Add(mi2);
                        }


                        mi2 = new MenuItem
                        {
                            Header = "Edit Hyperlink"
                        };
                        mi2.Click += (o, args) =>
                        {
                            Model.ActiveEditor.AceEditor.SetSelectionRange(pos.row, match.Index, pos.row,
                                match.Index + val.Length);
                            Model.ActiveEditor.EditorSelectionOperation("hyperlink", val);
                        };
                        ContextMenu.Items.Add(mi2);

                        if (val.Contains("](#"))
                        {
                            mi2 = new MenuItem
                            {
                                Header = "Jump to Anchor"
                            };
                            mi2.Click += (o, args) =>
                            {
                                var anchor = StringUtils.ExtractString(val, "](#", ")");

                                var docModel = new DocumentOutlineModel();
                                int lineNo =docModel.FindHeaderHeadline(Model.ActiveEditor?.GetMarkdown(), anchor);

                                if (lineNo != -1)
                                    Model.ActiveEditor.GotoLine(lineNo);
                            };
                            ContextMenu.Items.Add(mi2);
                        }

                        var mi = new MenuItem
                        {
                            Header = "Remove Hyperlink"
                        };
                        mi.Click += (o, args) =>
                        {
                            Model.ActiveEditor.AceEditor.SetSelectionRange(pos.row, match.Index, pos.row,
                                match.Index + val.Length);
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

        static readonly Regex ImageRegex = new Regex(@"!\[.*?\]\((.*?)\)", RegexOptions.IgnoreCase);

        /// <summary>
        /// Adds menu options for image editing, embedding and a few other
        /// image operations conditionally.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private bool CheckForImageLink(string line, AcePosition pos)
        {
            // Check for images ![](imageUrl)
            var matches = ImageRegex.Matches(line);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    string val = match.Value;
                    var imageLink = HttpUtility.UrlDecode(match.Groups[1].Value);
                    
                    
                    if (match.Index <= pos.column && match.Index + val.Length > pos.column)
                    {
                        bool isWebLink = imageLink.StartsWith("http");

                        if (!isWebLink)
                        {
                            var mi = new MenuItem {Header = "Edit in Image Editor"};
                            mi.Click += (o, args) =>
                            {
                                imageLink = mmFileUtils.NormalizeFilenameWithBasePath(imageLink,
                                    Path.GetDirectoryName(Model.ActiveDocument.Filename));
                                mmFileUtils.OpenImageInImageEditor(imageLink);
                            };
                            ContextMenu.Items.Add(mi);
                        }

                        var mi2 = new MenuItem
                        {
                            Header = "Edit Image Link"
                        };
                        mi2.Click += (o, args) =>
                        {
                            Model.ActiveEditor.AceEditor.SetSelectionRange(pos.row, match.Index, pos.row,
                                match.Index + val.Length);
                            Model.ActiveEditor.EditorSelectionOperation("image", val);
                        };
                        ContextMenu.Items.Add(mi2);

                        if (isWebLink)
                        {
                            var mi = new MenuItem {Header = "Download and embed as Local Image"};
                            mi.Click += async (o, args) =>
                            {
                                var sd = new SaveFileDialog
                                {
                                    Filter = "Image files (*.png;*.jpg;*.gif;)|*.png;*.jpg;*.jpeg;*.gif|" +
                                             "All Files (*.*)|*.*",
                                    FilterIndex = 1,
                                    Title = "Save Image from URL as",
                                    CheckFileExists = false,
                                    OverwritePrompt = true,
                                    CheckPathExists = true,
                                    RestoreDirectory = true,
                                    ValidateNames = true,
                                    FileName = Path.GetFileName(imageLink)
                                };
                                var doc = Model.ActiveDocument;
                                var editor = Model.ActiveEditor;

                                if (!string.IsNullOrEmpty(doc.LastImageFolder))
                                    sd.InitialDirectory = doc.LastImageFolder;
                                else if (!string.IsNullOrEmpty(doc.Filename) &&
                                         !doc.Filename.Equals("untitled", StringComparison.OrdinalIgnoreCase))
                                    sd.InitialDirectory = Path.GetDirectoryName(doc.Filename);
                                else
                                    sd.InitialDirectory =
                                        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                                sd.ShowDialog();

                                if (string.IsNullOrEmpty(sd.FileName))
                                    return;

                                try
                                {
                                    var wc = new WebClient();
                                    await wc.DownloadFileTaskAsync(new Uri(imageLink), sd.FileName);

                                    string filename;
                                    if (doc.Filename.Equals("untitled", StringComparison.OrdinalIgnoreCase))
                                        filename = doc.LastImageFolder; // let doc figure out the path
                                    else
                                        filename = FileUtils.GetRelativePath(sd.FileName,
                                            Path.GetDirectoryName(doc.Filename));

                                    // group hold just the filename
                                    var group = match.Groups[1];
                                    editor.SetSelectionRange(pos.row, group.Index, pos.row,
                                        group.Index + group.Value.Length);
                                    editor.SetSelection(WebUtility.UrlEncode(filename));
                                }
                                catch (Exception ex)
                                {
                                    Model.Window.ShowStatusError($"Image failed to download: {ex.Message}");
                                }


                            };
                            ContextMenu.Items.Add(mi);
                        }


                        return true;
                    }

                }
            }

            return false;
        }

        private bool CheckForTable(string line, AcePosition pos)
        {
            if (string.IsNullOrEmpty(line))
                return false;

            line = line.Trim();

            if (line.Contains("|")  ||
                line.StartsWith("+-") ||
                line.StartsWith("+="))
            {
                
                var mi = new MenuItem
                {
                    Header = "Edit Table"
                };
                mi.Click += (o, args) =>
                {
                    var tableMarkdown = SelectPipeAndGridTableMarkdown();
                    Model.ActiveEditor.EditorSelectionOperation("table", tableMarkdown);
                };
                ContextMenu.Items.Add(mi);

                mi = new MenuItem
                {
                    Header = "Format Table"
                };
                mi.Click += (o, args) =>
                {
                    string mdTableHtml = SelectPipeAndGridTableMarkdown();
                    if (string.IsNullOrEmpty(mdTableHtml))
                        return;

                    var parser = new TableParser();
                    var formatted = parser.FormatMarkdownTable(mdTableHtml);
                    if (formatted == null)
                        return;

                    Model.ActiveEditor.SetSelectionAndFocus(formatted);
                    Model.ActiveEditor.PreviewMarkdownCallback();
                };
                ContextMenu.Items.Add(mi);

                return true;
            }
            else if (line.StartsWith("<td",StringComparison.InvariantCultureIgnoreCase)  ||
                     line.StartsWith("<tr", StringComparison.InvariantCultureIgnoreCase) ||
                     line.StartsWith("<th", StringComparison.InvariantCultureIgnoreCase) ||
                     line.StartsWith("<table>", StringComparison.InvariantCultureIgnoreCase) ||
                     line.StartsWith("<table ", StringComparison.InvariantCultureIgnoreCase) ||
                     line.Equals("<thead>", StringComparison.InvariantCultureIgnoreCase) ||
                     line.Equals("<tbody>", StringComparison.InvariantCultureIgnoreCase) )
            {
                StringBuilder sbTableMarkdown = new StringBuilder();

                var mi = new MenuItem
                {
                    Header = "Edit Table"
                };
                mi.Click += (o, args) =>
                {
                    string mdTableHtml = SelectHtmlTableMarkdown();
                    if (string.IsNullOrEmpty(mdTableHtml))
                        return;
                    
                    Model.ActiveEditor.EditorSelectionOperation("table", mdTableHtml);
                };
                ContextMenu.Items.Add(mi);
                mi = new MenuItem
                {
                    Header = "Format Table"
                };
                mi.Click += (o, args) =>
                {
                    string mdTableHtml = SelectHtmlTableMarkdown();
                    if (string.IsNullOrEmpty(mdTableHtml))
                        return;

                    var parser = new TableParser();
                    var formatted = parser.FormatMarkdownTable(mdTableHtml);
                    if (formatted == null)
                        return;

                    Model.ActiveEditor.SetSelectionAndFocus(formatted);
                    Model.ActiveEditor.PreviewMarkdownCallback();
                };
                
                ContextMenu.Items.Add(mi);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to select an HTML table in the editor and returns
        /// the table HTML as a string
        /// </summary>
        /// <returns></returns>
        private string SelectHtmlTableMarkdown(bool noSelection = false)
        {
            StringBuilder sbTableMarkdown = new StringBuilder();
            var editor = Model.ActiveEditor;

            var lineText = editor.GetCurrentLine();

            var startPos = editor.GetCursorPosition();
            var row = startPos.row;
            int startRow = -1;

            for (int i = row; i > -1; i--)
            {
                lineText = editor.GetLine(i);
                if (lineText.Trim().StartsWith("<table", StringComparison.InvariantCultureIgnoreCase))
                {
                    startRow = i;
                    break;
                }
            }

            if (startRow == -1)
                return null;

            int endRow = startRow;
            for (int i = row ; i < 99999999; i++)
            {
                lineText = editor.GetLine(i);
                if (lineText.Trim().EndsWith("</table>", StringComparison.InvariantCultureIgnoreCase))
                {
                    endRow = i;
                    break;
                }
            }

            if (endRow == startRow)
                return null;

            for (int i = startRow; i <= endRow; i++)
            {
                sbTableMarkdown.AppendLine(editor.GetLine(i));
            }

            if(!noSelection)
                // select the entire table
                Model.ActiveEditor.AceEditor.SetSelectionRange(startRow - 1, 0, endRow + 1, 0);

            return sbTableMarkdown.ToString();
        }

        private string SelectPipeAndGridTableMarkdown(bool noSelection = false)
        {
            var editor = Model.ActiveEditor;

            var lineText = editor?.GetCurrentLine()?.Trim();
            if (string.IsNullOrEmpty(lineText) ||
                !lineText.Contains("|") && !lineText.StartsWith("+") && !lineText.EndsWith("+") )
                return null;

            var startPos = editor.GetCursorPosition();
            var row = startPos.row;
            var startRow = row;
            for (int i = row - 1; i > -1; i--)
            {
                lineText = editor.GetLine(i);
                if (!lineText.Contains("|") &&
                   !(lineText.Trim().StartsWith("+") && lineText.Trim().EndsWith("+")))
                {
                    startRow = i + 1;
                    break;
                }

                if (i == 0)
                {
                    startRow = 0;
                    break;
                }
            }

            var endRow = startPos.row;
            for (int i = row + 1; i < 99999999; i++)
            {
                lineText = editor.GetLine(i);
                if (!lineText.Contains("|") &&
                    !(lineText.Trim().StartsWith("+") && lineText.Trim().EndsWith("+")))
                {
                    endRow = i - 1;
                    break;
                }
            }

            if (endRow == startRow || endRow < startRow + 2)
                return null;

            StringBuilder sb = new StringBuilder();
            for (int i = startRow; i <= endRow; i++)
            {
                sb.AppendLine(editor.GetLine(i));
            }

            if (!noSelection)
                // select the entire table
                Model.ActiveEditor.AceEditor.SetSelectionRange(startRow, 0, endRow + 1, 0);

            return sb.ToString();
        }
    }
}
