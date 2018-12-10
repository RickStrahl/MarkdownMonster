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
                    Key = "Alt+Shift+Enter",
                    CommandName = "DistractionFreeMode",
                    Command = model.Commands.DistractionFreeModeCommand,
                    CommandParameter = "Toggle"
                },
                new AppKeyBinding
                {
                    Key = "F11",
                    CommandName = "PresentationMode",
                    Command = model.Commands.PresentationModeCommand,
                    CommandParameter = "Toggle"
                },
                new AppKeyBinding
                {
                    Key = "F12",
                    CommandName = "TogglePreviewBrowser",
                    Command = model.Commands.TogglePreviewBrowserCommand,
                    CommandParameter="Toggle"
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+Shift+B",
                    CommandName = "ToggleLeftSidebarPanel",
                    Command = model.Commands.ToggleLeftSidebarPanelCommand
                },
                new AppKeyBinding
                {
                    Key="Shift+F12",
                    CommandName="ShowExternalBrowser",
                    Command = model.Commands.ViewInExternalBrowserCommand,
                    CommandParameter = "Toggle"
                },

                // Document Commands
                new AppKeyBinding
                {
                    Key = "Ctrl+N",
                    CommandName = "NewDocument",
                    Command = model.Commands.NewDocumentCommand,
                    HasJavaScriptHandler = true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+O",
                    CommandName = "OpenDocument",
                    Command = model.Commands.OpenDocumentCommand,
                    HasJavaScriptHandler = true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+S",
                    CommandName = "SaveDocument",
                    Command = model.Commands.SaveCommand,
                    HasJavaScriptHandler = true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+Shift+S",
                    CommandName = "SaveAs",
                    Command = model.Commands.SaveAsCommand
                },
                new AppKeyBinding
                {
                    Key = "Alt+Shift+S",
                    CommandName = "SaveAll",
                    Command = model.Commands.SaveAllCommand
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+P",
                    CommandName = "PrintPreview",
                    Command = model.Commands.PrintPreviewCommand
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+Q",
                    CommandName = "InsertQuote",
                    Command = model.Commands.ToolbarInsertMarkdownCommand,
                    CommandParameter = "quote"
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+F4",
                    CommandName = "CloseActiveDocument",
                    Command = model.Commands.CloseActiveDocumentCommand
                },
                new AppKeyBinding
                {
                    Id = "CloseActiveDocument2",
                    Key = "Ctrl+W",
                    CommandName = "CloseActiveDocument",
                    Command = model.Commands.CloseActiveDocumentCommand
                },

                // Editor Commands
                new AppKeyBinding
                {
                    Key = "Alt+Z",
                    CommandName = "ToggleWordWrap",
                    Command = model.Commands.WordWrapCommand
                },
                new AppKeyBinding
                {
                    Key = "F1",
                    CommandName = "ShowHelp",
                    Command = model.Commands.HelpCommand,
                    HasJavaScriptHandler = true
                },
                new AppKeyBinding
                {
                    Key = "F5",
                    CommandName = "ReloadEditor",
                    HasJavaScriptHandler = true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+F5",
                    CommandName = "ReloadEditor",
                    HasJavaScriptHandler = true
                },


                //JavaScript Editor Only Commands
                new AppKeyBinding
                {
                    Key = "Ctrl+B",
                    CommandName = "InsertBold",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+I",
                    CommandName = "InsertItalic",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+K",
                    CommandName = "InsertHyperlink",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+L",
                    CommandName = "InsertList",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+J",
                    CommandName = "InsertEmoji",
                    HasJavaScriptHandler= true
                },

                new AppKeyBinding
                {
                    Key = "Alt+I",
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
                    Key = "Alt+C",
                    CommandName = "InsertCodeblock",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+`",
                    CommandName = "InsertInlineCode",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "Shift+Del",
                    CommandName = "DeleteCurrentLine",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+Tab",
                    CommandName = "NextTab",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+Shift+Tab",
                    CommandName = "PreviousTab",
                    HasJavaScriptHandler= true
                },

                new AppKeyBinding
                {
                    Key = "Ctrl+-",
                    CommandName = "ZoomEditorDown",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+=",
                    CommandName = "ZoomEditorUp",
                    HasJavaScriptHandler= true
                },


                new AppKeyBinding
                {
                    Key = "Ctrl+Shift+C",
                    CommandName = "CopyMarkdownAsHtml",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+Shift+V",
                    CommandName = "PasteHtmlAsMarkdown",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+Shift+Z",
                    CommandName = "RemoveMarkdownFormatting",
                    HasJavaScriptHandler= true
                },

                new AppKeyBinding
                {
                    Id = "Paste2",
                    Key = "Shift+Insert",
                    CommandName = "Paste2",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+V",
                    CommandName = "Paste",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key="F7",
                    CommandName = "NextSpellCheckError",                    
                    HasJavaScriptHandler = true
                }

            };
        }
    }
}
