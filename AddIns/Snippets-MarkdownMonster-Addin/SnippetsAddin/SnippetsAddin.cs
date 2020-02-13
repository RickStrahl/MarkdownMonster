using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using Westwind.Scripting;

namespace SnippetsAddin
{
    public class SnippetsAddin : MarkdownMonster.AddIns.MarkdownMonsterAddin
    {
        private SnippetsWindow snippetsWindow;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            Id = "Snippets";


            // by passing in the add in you automatically
            // hook up OnExecute/OnExecuteConfiguration/OnCanExecute
            var menuItem = new AddInMenuItem(this)
            {
                FontawesomeIcon = FontAwesomeIcon.PencilSquareOutline,
                Caption = "Snippets Template Expansions",
                KeyboardShortcut = SnippetsAddinConfiguration.Current.KeyboardShortcut
            };
            try
            {
                menuItem.IconImageSource = new ImageSourceConverter()
                    .ConvertFromString("pack://application:,,,/SnippetsAddin;component/icon_22.png") as ImageSource;
            }
            catch { }

            // if you don't want to display config or main menu item clear handler
            //menuItem.ExecuteConfiguration = null;

            // Must add the menu to the collection to display menu and toolbar items
            this.MenuItems.Add(menuItem);
        }

        public override void OnWindowLoaded()
        {
            bool requiresCompiler = false;
            foreach (var snippet in SnippetsAddinConfiguration.Current.Snippets)
            {
                if (!requiresCompiler && snippet.SnippetText.Contains("{{") || snippet.SnippetText.Contains("@"))
                    requiresCompiler = true;


                if (!string.IsNullOrEmpty(snippet.KeyboardShortcut))
                {
                    var ksc = snippet.KeyboardShortcut.ToLower();
                    KeyBinding kb = new KeyBinding();

                    if (ksc.Contains("alt"))
                        kb.Modifiers = ModifierKeys.Alt;
                    if (ksc.Contains("shift"))
                        kb.Modifiers |= ModifierKeys.Shift;
                    if (ksc.Contains("ctrl") || ksc.Contains("ctl"))
                        kb.Modifiers |= ModifierKeys.Control;

                    string key =
                        ksc.Replace("+", "")
                            .Replace("-", "")
                            .Replace("_", "")
                            .Replace(" ", "")
                            .Replace("alt", "")
                            .Replace("shift", "")
                            .Replace("ctrl", "")
                            .Replace("ctl", "");

                    key =   CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key);
                    if (!string.IsNullOrEmpty(key))
                    {
                        KeyConverter k = new KeyConverter();
                        kb.Key = (Key)k.ConvertFromString(key);
                    }

                    // Whatever command you need to bind to
                    kb.Command = new CommandBase((s, e) => InsertSnippet(snippet),
                                                 (s,e) => Model.IsEditorActive);

                    Model.Window.InputBindings.Add(kb);
                }
            }

            if (requiresCompiler)
            {
                RoslynLifetimeManager.WarmupRoslyn();
            }
        }

        public override void OnExecute(object sender)
        {
            if (snippetsWindow == null || !snippetsWindow.IsLoaded)
            {

                if (!UnlockKey.IsUnlockedPremium)
                {
                    UnlockKey.ShowPremiumDialog("Snippet Template Expansion",
                        "https://github.com/RickStrahl/Snippets-MarkdownMonster-Addin");
                    return;
                }

                snippetsWindow = new SnippetsWindow(this);
            }
            snippetsWindow.Show();
            snippetsWindow.Activate();
        }

        public override void OnExecuteConfiguration(object sender)
        {
            Model.Window.OpenTab(Path.Combine(mmApp.Configuration.CommonFolder, "snippetsaddin.json"));
        }

        public override bool OnCanExecute(object sender)
        {
            return Model.IsEditorActive;
        }

        public override void OnApplicationShutdown()
        {
            // if Roslyn is running shut it down
            RoslynLifetimeManager.ShutdownRoslyn();
            snippetsWindow?.Close();
        }


        /// <summary>
        /// Helper function that returns the snippet text that's to be inserted
        /// </summary>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public string GetEvaluatedSnippetText(Snippet snippet)
        {
            string snippetText = snippet.SnippetText;

            try
            {
                var parser = new ScriptParser();

                if (snippet.ScriptMode == ScriptModes.CSharpExpressions && snippetText.Contains("{{"))
                    snippetText = parser.EvaluateScript(snippetText, Model);
                else if (snippet.ScriptMode == ScriptModes.Razor && snippetText.Contains("@"))
                    snippetText = parser.EvaluateRazorScript(snippetText, Model);

                if (snippetText == null)
                {
                    MessageBox.Show(parser.ErrorMessage, "Snippet Execution failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("We were unable to run this snippet.\r\nError:\r\n\r\n" +
                                ex.GetBaseException().Message,
                    "Snippet Execution Failed");
                return null;
            }

            return snippetText;
        }

        /// <summary>
        /// Inserts a selected snippet into the current document
        /// </summary>
        /// <param name="snippet"></param>
        public void InsertSnippet(Snippet snippet)
        {
            var snippetText = GetEvaluatedSnippetText(snippet);
            if (string.IsNullOrEmpty(snippetText))
                return;

            int idx = snippetText.IndexOf("~");
            snippetText = snippetText.Replace("~", "");

            SetSelection(snippetText);

            if (idx > -1)
            {
                var snipRemain = snippetText.Substring(idx);
                int move = snipRemain.Replace("\r","").Length;

                // older versions don't have these APIs
                try
                {
                    var editor = GetMarkdownEditor();
                    editor.AceEditor.MoveCursorLeft(move);
                }
                catch { }
            }

        }

        public override void OnDocumentUpdated()
        {
            var editor = GetMarkdownEditor();
            string line = editor.GetCurrentLine();
            if (string.IsNullOrEmpty(line))
                return;

            var snippet = SnippetsAddinConfiguration.Current.Snippets
                                                    .FirstOrDefault(sn => !string.IsNullOrEmpty(sn.Shortcut) && line.Trim().EndsWith(sn.Shortcut));
            if (snippet != null)
            {
                editor.FindAndReplaceTextInCurrentLine(snippet.Shortcut, "");
                InsertSnippet(snippet);
            }
        }
    }
}
