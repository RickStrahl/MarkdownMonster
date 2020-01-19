using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Westwind.Utilities;

namespace MarkdownMonster.Utilities
{

    /// <summary>
    /// Class that maps Key Bindings to Commands or a JavaScript
    /// handler in Ace Editor to a KeyBindings handler function with
    /// the same name as the command (in camelCase - OpenDocument-> openDocument()
    ///
    /// To use:
    /// * Subclass from this class
    /// * Add keybindings in ctor() and map to Commands/JavaScript handlers
    /// * Instantiate
    /// * call SetKeyBindings() to attach bindings for control
    /// * (optional) call SaveKeyBindings() to save to disk
    /// * (optional) call LoadKeyBindings() to load from disk
    /// </summary>
    public class KeyBindingsManager
    {
        public List<AppKeyBinding> KeyBindings { get; set; }

        public string KeyBindingsFilename { get; set; }

        /// <summary>
        /// The control or window that this manager is bound to
        /// </summary>
        protected Control BindingsControl { get; set; }

        /// <summary>
        /// Initialize - pass in a control - typically a Window - that
        /// the bindings are applied to.
        /// </summary>
        /// <param name="control"></param>
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
                if (kb.Command == null || kb.Key == null)
                    continue;

                KeyBinding keyBinding = CreateKeyboardShortcutBinding(
                    kb.Key,
                    kb.Command, kb.CommandParameter);
                if (keyBinding != null)
                    BindingsControl.InputBindings.Add(keyBinding);
            }
        }


        /// <summary>
        /// Returns the keyboard shortcut for a given command. This is the mapped
        /// command if extended via keyboard bindings.
        /// </summary>
        /// <param name="commandName"></param>
        /// <returns></returns>
        public string GetInputGestureForCommand(string commandName)
        {
            return KeyBindings.FirstOrDefault(kb => kb.CommandName == commandName)?.Key;
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

            // TODO: Fix bug with multiple keybindings to the same Command - need Unique Identifier
            foreach (var kb in keyBindings)
            {
                var keyBinding = KeyBindings.FirstOrDefault(binding => binding.Id == kb.Id);
                if (keyBinding == null)
                {
                    if (kb.CommandName == "EditorCommand")
                    {
                        keyBinding = kb;
                        keyBinding.HasJavaScriptHandler = true;
                        KeyBindings.Add(keyBinding);
                    }
                    continue;
                }

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
