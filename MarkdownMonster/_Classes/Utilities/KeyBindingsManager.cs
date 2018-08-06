using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MarkdownMonster.Windows;
using Newtonsoft.Json;
using Westwind.Utilities;

namespace MarkdownMonster.Utilities
{
    public class KeyBindingsManager
    {
        public List<mmKeyBinding> KeyBindings { get; set; }
        public string KeyBindingsFilename { get; set; }

        public KeyBindingsManager()
        {
            var model = mmApp.Model;

            KeyBindingsFilename = Path.Combine(mmApp.Model.Configuration.CommonFolder,
                "MarkdownMonster-KeyBindings.json");

            KeyBindings = new List<mmKeyBinding>
            {
                new mmKeyBinding
                {
                    Key = "alt+shift+return",
                    CommandName = "DistractionFreeMode",
                    Command = model.Commands.DistractionFreeModeCommand,
                    CommandParameter = "Toggle"
                },
                new mmKeyBinding
                {
                    Key = "f11",
                    CommandName = "PresentationMode",
                    Command = model.Commands.PresentationModeCommand
                },
                new mmKeyBinding
                {
                    Key = "ctrl-n",
                    CommandName = "NewDocument",
                    Command = model.Commands.NewDocumentCommand,
                    HasJavaScriptHandler = true
                },
                new mmKeyBinding
                {
                    Key = "ctrl-o",
                    CommandName = "OpenDocument",
                    Command = model.Commands.OpenDocumentCommand,
                    HasJavaScriptHandler = true
                },
                new mmKeyBinding
                {
                    Key = "ctrl-s",
                    CommandName = "SaveDocument",
                    Command = model.Commands.SaveCommand,
                    HasJavaScriptHandler = true
                },
                new mmKeyBinding
                {
                    Key = "ctrl-shift-s",
                    CommandName = "SaveAs",
                    Command = model.Commands.SaveAsCommand
                },
                new mmKeyBinding
                {
                    Key = "alt-shift-s",
                    CommandName = "SaveAll",
                    Command = model.Commands.SaveAllCommand
                },
                new mmKeyBinding
                {
                    Key = "ctrl-p",
                    CommandName = "Print",
                    Command = model.Commands.PrintPreviewCommand
                },
                new mmKeyBinding
                {
                    Key = "ctrl-q",
                    CommandName = "InsertQuote",
                    Command = model.Commands.ToolbarInsertMarkdownCommand,
                    CommandParameter = "quote"
                },
                new mmKeyBinding
                {
                    Key = "ctrl-w",
                    CommandName = "CloseActiveDocument",
                    Command = model.Commands.CloseActiveDocumentCommand
                },
                new mmKeyBinding
                {
                    Key = "ctrl-f4",
                    CommandName = "CloseActiveDocument",
                    Command = model.Commands.CloseActiveDocumentCommand
                },
                new mmKeyBinding
                {
                    Key = "alt-z",
                    CommandName = "ToggleWordWrap",
                    Command = model.Commands.WordWrapCommand
                },
                new mmKeyBinding
                {
                    Key = "f1",
                    CommandName = "ShowHelp",
                    Command = model.Commands.HelpCommand,
                    HasJavaScriptHandler = true
                },
                new mmKeyBinding
                {
                    Key = "f5",
                    CommandName = "ReloadEditor",
                    HasJavaScriptHandler = true
                },
                new mmKeyBinding
                {
                    Key = "ctrl-f5",
                    CommandName = "ReloadEditor2",
                    HasJavaScriptHandler = true
                },


                //JavaScript Only Commands
                new mmKeyBinding
                {
                    Key = "ctrl-k",
                    CommandName = "InsertHyperlink",
                    HasJavaScriptHandler= true
                },
                new mmKeyBinding
                {
                    Key = "ctrl-l",
                    CommandName = "InsertList",
                    HasJavaScriptHandler= true
                },
                new mmKeyBinding
                {
                    Key = "ctrl-j",
                    CommandName = "InsertEmoji",
                    HasJavaScriptHandler= true
                },


            };

        }

        public void SetKeyBindings()
        {
            foreach (var kb in KeyBindings)
            {
                KeyBinding keyBinding = CreateKeyboardShortcutBinding(
                    kb.Key,
                    kb.Command, kb.CommandParameter);
                if (kb != null)
                    mmApp.Model.Window.InputBindings.Add(keyBinding);
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

            var keyBindings =
                JsonSerializationUtils.DeserializeFromFile(filename, typeof(List<mmKeyBinding>), false)
                    as List<mmKeyBinding>;

            if (keyBindings != null)
            {
                foreach (var kb in keyBindings)
                {
                    var keyBinding = KeyBindings.FirstOrDefault(binding => binding.CommandName == kb.CommandName);

                    keyBinding.Key = kb.Key;
                    if (keyBinding.Command != null)
                        keyBinding.Command.KeyboardShortcut = kb.Key;
                }

                return true;
            }

            return false;
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

    /// <summary>
    /// Holds a specific key binding combination
    /// </summary>
    [DebuggerDisplay("{Key}")]
    public class mmKeyBinding
    {
        public string Key { get; set; }

        public string CommandName { get; set; }



        [JsonIgnore]
        public CommandBase Command
        {
            get => _command;
            set
            {
                _command = value;
                _command.KeyboardShortcut = Key;
            }
        }

        private CommandBase _command;

        [JsonIgnore] public object CommandParameter { get; set; }

        public string JavaScriptHandlerScript
        {
            get
            {
                if (string.IsNullOrEmpty(CommandName))
                    return CommandName;

                return CommandName[0].ToString().ToLower() + CommandName.Substring(1);
            }
        }

        public bool HasJavaScriptHandler { get; set; }
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
