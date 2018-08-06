using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarkdownMonster.Windows;
using Newtonsoft.Json;
using Westwind.Utilities;

namespace MarkdownMonster.Utilities
{

    /// <summary>
    /// Class that maps Key Bindings to Commands or a JavaScript
    /// handler in Ace Editor to a KeyBindings handler function with
    /// the same name as the command (in camelCase - OpenDocument-> openDocument()
    /// </summary>
    public class KeyBindingsManager
    {
        public List<AppKeyBinding> KeyBindings { get; set; }

        public string KeyBindingsFilename { get; set; }

        /// <summary>
        /// The control or window that this manager is bound to
        /// </summary>
        protected Control BindingsControl { get; set; }

        public KeyBindingsManager(Control control)
        {
            KeyBindingsFilename = Path.Combine(mmApp.Model.Configuration.CommonFolder,
                "MarkdownMonster-KeyBindings.json");
        }

        public void SetKeyBindings()
        {
            foreach (var kb in KeyBindings)
            {
                // ignore JavaScript only shortcuts for binding
                if (kb.Command == null)
                    continue;

                KeyBinding keyBinding = CreateKeyboardShortcutBinding(
                    kb.Key,
                    kb.Command, kb.CommandParameter);
                if (kb != null)
                    BindingsControl.InputBindings.Add(keyBinding);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool LoadKeyBindings(string filename = null)
        {
            if (filename == null)
                filename = KeyBindingsFilename;

            List<AppKeyBinding> keyBindings;
            keyBindings = JsonSerializationUtils.DeserializeFromFile(filename, typeof(List<AppKeyBinding>))
                    as List<AppKeyBinding>;
            
            if (keyBindings == null || keyBindings.Count < 1)
                return false;

            foreach (var kb in keyBindings)
            {
                var keyBinding = KeyBindings.FirstOrDefault(binding => binding.CommandName == kb.CommandName);

                keyBinding.Key = kb.Key;
                if (keyBinding.Command != null)
                    keyBinding.Command.KeyboardShortcut = kb.Key;
            }

            return true;

        }

        /// <summary>
        /// Saves key bindings to a file
        /// </summary>
        /// <returns></returns>
        public bool SaveKeyBindings(string filename = null)
        {
            if (filename == null)
                filename = KeyBindingsFilename;

            return JsonSerializationUtils.SerializeToFile(KeyBindings, filename, false, true);
        }

        /// <summary>
        /// Creates a keyboard shortcut from a 
        /// </summary>
        /// <param name="ksc"></param>
        /// <param name="command"></param>
        /// <returns>KeyBinding - Window.InputBindings.Add(keyBinding)</returns>
        public static KeyBinding CreateKeyboardShortcutBinding(string ksc, ICommand command,
            object commandParameter = null)
        {
            if (string.IsNullOrEmpty(ksc))
                return null;

            try
            {
                KeyBinding kb = new KeyBinding();
                ksc = ksc.ToLower();

                if (ksc.Contains("alt"))
                    kb.Modifiers = ModifierKeys.Alt;
                if (ksc.Contains("shift"))
                    kb.Modifiers |= ModifierKeys.Shift;
                if (ksc.Contains("ctrl") || ksc.Contains("ctl"))
                    kb.Modifiers |= ModifierKeys.Control;
                if (ksc.Contains("win"))
                    kb.Modifiers |= ModifierKeys.Windows;

                string key =
                    ksc.Replace("+", "")
                        .Replace("-", "")
                        .Replace("_", "")
                        .Replace(" ", "")
                        .Replace("alt", "")
                        .Replace("shift", "")
                        .Replace("ctrl", "")
                        .Replace("ctl", "");

                key = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key);
                if (!string.IsNullOrEmpty(key))
                {
                    KeyConverter k = new KeyConverter();
                    kb.Key = (Key) k.ConvertFromString(key);
                }

                // Whatever command you need to bind to
                kb.Command = command;
                if (commandParameter != null)
                    kb.CommandParameter = commandParameter;

                return kb;
            }
            // deal with invalid bindings - ignore them
            catch (Exception ex)
            {
                mmApp.Log("Unable to assign key binding: " + ksc, ex);
                return null;
            }
        }

    }
}

//<Window.InputBindings>
  //      <KeyBinding Modifiers = "Alt+Shift"
  //                  Key="Return"
  //                  Command="{Binding Commands.DistractionFreeModeCommand}" CommandParameter="Toggle" />
  //      <KeyBinding Key = "F11" Command="{Binding Commands.PresentationModeCommand}" CommandParameter="Toggle" />
  //      <KeyBinding Modifiers = "Ctrl" Key="P" Command="{Binding Commands.PrintPreviewCommand}" />

//      <KeyBinding Modifiers = "Ctrl+Shift" Key="S" Command="{Binding Commands.SaveAsCommand}" />
//      <KeyBinding Modifiers = "Alt+Shift" Key="S" Command="{Binding Commands.SaveAllCommand}" />
//      <KeyBinding Modifiers = "Ctrl" Key="N" Command="{Binding Commands.NewDocumentCommand }"  />
//      <KeyBinding Modifiers = "Ctrl" Key="O" Command="{Binding Commands.OpenDocumentCommand }"    />
//      <KeyBinding Modifiers = "Ctrl" Key="Q" Command="{Binding Commands.ToolbarInsertMarkdownCommand }"  CommandParameter="quote"  />
//      <KeyBinding Modifiers = "Ctrl" Key="W" Command="{Binding  Commands.CloseActiveDocumentCommand}" />
//      <KeyBinding Modifiers = "Ctrl" Key="F4" Command="{Binding Commands.CloseActiveDocumentCommand}" />
//      <KeyBinding Modifiers = "Alt" Key="Z" Command="{Binding Commands.WordWrapCommand}" />
//      <KeyBinding Key = "F1" Command="{Binding Commands.HelpCommand}" />

//      <!-- Commands that are handled in the Markdown Editor because they are not working here -->
//      <!--<KeyBinding Modifiers = "Ctrl+Shift" Key="Z" Command="{Binding Commands.RemoveMarkdownFormattingCommand}" />-->        
//  </Window.InputBindings>
