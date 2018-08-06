using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MarkdownMonster.Windows;

namespace MarkdownMonster.Utilities
{
    public class KeyBindingsManager
    {
        public List<mmKeyBinding> KeyBindings { get; set; }

        public KeyBindingsManager()
        {
            var model = mmApp.Model;

            var KeyBindings = new List<mmKeyBinding>
            {
                new mmKeyBinding
                {
                    Key = "alt+shift+return",
                    Command = model.Commands.DistractionFreeModeCommand,
                    CommandParameter = "Toggle"
                },
                new mmKeyBinding
                {
                    Key = "f11",
                    Command = model.Commands.PresentationModeCommand,
                    CommandParameter = "Toggle"
                },
                new mmKeyBinding
                {
                    Key = "ctrl-n",
                    Command = model.Commands.NewDocumentCommand,
                    JavaScriptHandlerScript = "te.specialkey(\"ctrl-n\");"
                },
                new mmKeyBinding
                {
                    Key = "ctrl-shift-s",
                    Command = model.Commands.SaveAsCommand
                },
                new mmKeyBinding
                {
                    Key = "alt-shift-s",
                    Command = model.Commands.SaveAsCommand
                },
                new mmKeyBinding
                {
                    Key = "ctrl-p",
                    Command = model.Commands.PrintPreviewCommand
                },
                new mmKeyBinding
                {
                    Key = "ctrl-q",
                    Command = model.Commands.ToolbarInsertMarkdownCommand,
                    CommandParameter = "quote"
                },
                new mmKeyBinding
                {
                    Key = "ctrl-w",
                    Command = model.Commands.CloseActiveDocumentCommand
                },
                new mmKeyBinding
                {
                    Key = "ctrl-f4",
                    Command = model.Commands.CloseActiveDocumentCommand
                },
                new mmKeyBinding
                {
                    Key = "alt-z",
                    Command = model.Commands.WordWrapCommand
                },
                new mmKeyBinding
                {
                    Key = "f1",
                    Command = model.Commands.HelpCommand
                },



                new mmKeyBinding
                {
                    Key = "ctrl-p",
                    Command = model.Commands.PrintPreviewCommand
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
        /// Creates a keyboard shortcut from a 
        /// </summary>
        /// <param name="ksc"></param>
        /// <param name="command"></param>
        /// <returns>KeyBinding - Window.InputBindings.Add(keyBinding)</returns>
        public static KeyBinding CreateKeyboardShortcutBinding(string ksc, ICommand command, object commandParameter = null)
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
                    kb.Key = (Key)k.ConvertFromString(key);
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

        public CommandBase Command { get; set; }

        public object CommandParameter { get; set; }
        
        public string JavaScriptHandlerScript { get; set; }

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
