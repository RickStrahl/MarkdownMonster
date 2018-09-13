using System.Collections.Generic;
using System.Windows.Controls;

namespace MarkdownMonster.Utilities
{

    /// <summary>
    /// Class to manage application key bindings.
    /// Either set a Command/Command parameter for a binding
    /// and/or add HasJavaScript = true to force JavaScript
    /// processing
    /// 
    /// To use:
    /// * Add keybindings in ctor()
    /// * Instantiate
    /// * call SetKeyBindings()
    /// * (optional) call SaveKeyBindings() to save to disk
    /// * (optional) call LoadKeyBindings() to load from disk
    /// </summary>
    public class MarkdownMonsterKeybindings : KeyBindingsManager
    {
        public MarkdownMonsterKeybindings(Control control) : base(control)
        {
            BindingsControl = control;

            var model = mmApp.Model;


            // Add any bindings to Commands here
            // Any JavaScript handlers should be in editor-keybindings.js and
            // have a handler that matches the command name:
            //
            // keyBindings.distractionFreeMode: function() {},
            // keybindings.newDocument: function() {}

            KeyBindings = new List<AppKeyBinding>
            {
                // View Commands
                new AppKeyBinding
                {
                    Key = "alt+shift+return",
                    CommandName = "DistractionFreeMode",
                    Command = model.Commands.DistractionFreeModeCommand,
                    CommandParameter = "Toggle"
                },
                new AppKeyBinding
                {
                    Key = "f11",
                    CommandName = "PresentationMode",
                    Command = model.Commands.PresentationModeCommand,
                    CommandParameter = "Toggle"
                },
                new AppKeyBinding
                {
                    Key = "f12",
                    CommandName = "TogglePreviewBrowser",
                    Command = model.Commands.TogglePreviewBrowserCommand,
                    CommandParameter="Toggle"
                },


                // Document Commands
                new AppKeyBinding
                {
                    Key = "ctrl-n",
                    CommandName = "NewDocument",
                    Command = model.Commands.NewDocumentCommand,
                    HasJavaScriptHandler = true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-o",
                    CommandName = "OpenDocument",
                    Command = model.Commands.OpenDocumentCommand,
                    HasJavaScriptHandler = true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-s",
                    CommandName = "SaveDocument",
                    Command = model.Commands.SaveCommand,
                    HasJavaScriptHandler = true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-shift-s",
                    CommandName = "SaveAs",
                    Command = model.Commands.SaveAsCommand
                },
                new AppKeyBinding
                {
                    Key = "alt-shift-s",
                    CommandName = "SaveAll",
                    Command = model.Commands.SaveAllCommand
                },
                new AppKeyBinding
                {
                    Key = "ctrl-p",
                    CommandName = "PrintPreview",
                    Command = model.Commands.PrintPreviewCommand
                },
                new AppKeyBinding
                {
                    Key = "ctrl-q",
                    CommandName = "InsertQuote",
                    Command = model.Commands.ToolbarInsertMarkdownCommand,
                    CommandParameter = "quote"
                },
                new AppKeyBinding
                {                    
                    Key = "ctrl-f4",
                    CommandName = "CloseActiveDocument",
                    Command = model.Commands.CloseActiveDocumentCommand
                },
                new AppKeyBinding
                {
                    Id = "CloseActiveDocument2",
                    Key = "ctrl-w",
                    CommandName = "CloseActiveDocument",
                    Command = model.Commands.CloseActiveDocumentCommand
                },

                // Editor Commands
                new AppKeyBinding
                {
                    Key = "alt-z",
                    CommandName = "ToggleWordWrap",
                    Command = model.Commands.WordWrapCommand
                },
                new AppKeyBinding
                {
                    Key = "f1",
                    CommandName = "ShowHelp",
                    Command = model.Commands.HelpCommand,
                    HasJavaScriptHandler = true
                },
                new AppKeyBinding
                {
                    Key = "f5",
                    CommandName = "ReloadEditor",
                    HasJavaScriptHandler = true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-f5",
                    CommandName = "ReloadEditor",
                    HasJavaScriptHandler = true
                },


                //JavaScript Editor Only Commands
                new AppKeyBinding
                {
                    Key = "ctrl-b",
                    CommandName = "InsertBold",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-i",
                    CommandName = "InsertItalic",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-k",
                    CommandName = "InsertHyperlink",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-l",
                    CommandName = "InsertList",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-j",
                    CommandName = "InsertEmoji",
                    HasJavaScriptHandler= true
                },

                new AppKeyBinding
                {
                    Key = "alt-i",
                    CommandName = "InsertImage",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "F3",
                    CommandName = "FindNext",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "F12",
                    CommandName = "TogglePreviewBrowser",
                    HasJavaScriptHandler= false
                },
                new AppKeyBinding
                {
                    Key = "alt-c",
                    CommandName = "InsertCodeblock",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-`",
                    CommandName = "InsertInlineCode",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "shift-del",
                    CommandName = "DeleteCurrentLine",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-tab",
                    CommandName = "NextTab",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-shift-tab",
                    CommandName = "PreviousTab",
                    HasJavaScriptHandler= true
                },

                new AppKeyBinding
                {
                    Key = "ctrl--",
                    CommandName = "ZoomEditorDown",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-=",
                    CommandName = "ZoomEditorUp",
                    HasJavaScriptHandler= true
                },


                new AppKeyBinding
                {
                    Key = "ctrl-shift-c",
                    CommandName = "CopyMarkdownAsHtml",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-shift-v",
                    CommandName = "PasteHtmlAsMarkdown",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-shift-z",                     
                    CommandName = "RemoveMarkdownFormatting",
                    HasJavaScriptHandler= true
                },

                new AppKeyBinding
                {
                    Id = "Paste2",
                    Key = "shift-insert",
                    CommandName = "Paste2",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "ctrl-v",
                    CommandName = "Paste",
                    HasJavaScriptHandler= true
                },


            };
        }
    }
}
