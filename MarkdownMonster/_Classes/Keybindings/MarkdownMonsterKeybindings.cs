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
                    Key = "Ctrl+Shift+R",
                    CommandName = "RefreshBrowserContentCommand",
                    Command = model.Commands.RefreshBrowserContentCommand
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
                new AppKeyBinding
                {
                    Key="Alt+G",
                    CommandName="CommitToGit",
                    Command = model.Commands.Git.CommitToGitCommand

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
                    Id="ReloadEditor",
                    Key = "F5",
                    CommandName = "ReloadEditor",
                    HasJavaScriptHandler = true
                },
                new AppKeyBinding
                {
                    Id="ReloadEditor2",
                    Key = "Ctrl+F5",
                    CommandName = "ReloadEditor",
                    HasJavaScriptHandler = true
                },


                //JavaScript Editor Only Commands
                new AppKeyBinding
                {
                    Id = "EditorCommand_Softbreak",
                    CommandName = "EditorCommand",
                    CommandParameter = "softbreak",
                    HasJavaScriptHandler= true,
                    Key = "Shift+Enter"
                },
                new AppKeyBinding
                {
                    Id="EditorCommand_Bold",
                    Key = "Ctrl+B",
                    CommandName = "EditorCommand",
                    CommandParameter = "bold",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Id="EditorCommand_Italic",
                    Key = "Ctrl+I",
                    CommandName = "EditorCommand",
                    CommandParameter = "italic",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Id="EditorCommand_Href",
                    Key = "Ctrl+K",
                    CommandName = "EditorCommand",
                    CommandParameter = "href",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Id="EditorCommand_Href2",
                    Key = "Ctrl+Shift+K",
                    CommandName = "EditorCommand",
                    CommandParameter = "href2",
                    HasJavaScriptHandler= true
                },

                new AppKeyBinding
                {
                    Id="EditorCommand_List",
                    Key = "Ctrl+L",
                    CommandName = "EditorCommand",
                    CommandParameter = "list",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Id = "EditorCommand_Emoji",
                    Key = "Ctrl+J",
                    CommandName = "EditorCommand",
                    CommandParameter = "emoji",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Id = "EditorCommand_Image",
                    CommandName = "EditorCommand",
                    CommandParameter = "image",
                    Key = "Alt+I",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Id = "EditorCommand_Image2",
                    CommandName = "EditorCommand",
                    CommandParameter = "image2",
                    Key = "Alt+Shift+I",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Id="EditorCommand_Quote",
                    CommandName = "EditorCommand",
                    CommandParameter = "quote",
                    HasJavaScriptHandler= true,
                    Key = "Ctrl+Q"
                },
                new AppKeyBinding
                {
                    Id = "EditorCommand_Code",
                    CommandName = "EditorCommand",
                    CommandParameter = "code",
                    HasJavaScriptHandler= true,
                    Key = "Alt+C",
                },
                new AppKeyBinding
                {
                    Id = "EditorCommand_InlineCode",
                    CommandName = "EditorCommand",
                    CommandParameter = "inlinecode",
                    HasJavaScriptHandler= true,
                    Key = "Ctrl+`"
                },


                new AppKeyBinding
                {
                    Key = "F3",
                    CommandName = "FindNext",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key="Ctrl+Shift+F",
                    CommandName="FindInFiles",
                    Command= model.Commands.OpenSearchSidebarCommand
                },
                new AppKeyBinding
                {
                    Key = "F12",
                    CommandName = "TogglePreviewBrowser",
                    HasJavaScriptHandler= false
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
                    Id="ZoomEditorUp",
                    Key = "Ctrl+=",
                    CommandName = "ZoomEditorUp",
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Id="ZoomEditorUp2",
                    Key = "Ctrl++",
                    CommandName = "ZoomEditorUp",
                    HasJavaScriptHandler= true
                },

                new AppKeyBinding
                {
                    Key = "Ctrl+Shift+C",
                    CommandName = "CopyAsHtml",
                    Command = model.Commands.CopyAsHtmlCommand,
                    HasJavaScriptHandler= false
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+Shift+V",
                    CommandName="PasteMarkdownFromHtml",
                    Command = model.Commands.PasteMarkdownFromHtmlCommand,
                    HasJavaScriptHandler= false
                },
                new AppKeyBinding
                {
                    Key = "Ctrl+Shift+Z",
                    CommandName = "RemoveMarkdownFormatting",
                    Command = model.Commands.RemoveMarkdownFormattingCommand,
                    HasJavaScriptHandler= false
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
                    Key = "Ctrl+C",
                    CommandName = "Copy",
                    //Command = model.Commands.CopyToClipboardCommand,
                    HasJavaScriptHandler= true
                },
                new AppKeyBinding
                {
                    Key="F7",
                    CommandName = "NextSpellCheckError",
                    HasJavaScriptHandler = true
                }, new AppKeyBinding
                {
                    Key="Shift+F7",
                    CommandName = "PreviousSpellCheckError",
                    HasJavaScriptHandler = true
                },
                // fix odd Alt-E handling in the editor, do nothing
                new AppKeyBinding
                {
                    Id="AltEDefaultBinding",
                    Key="Alt+E",
                    CommandName = "DoNothing",
                    HasJavaScriptHandler = true
                },


                new AppKeyBinding
                {
                    Id= "SidebarTabActivationCommand",
                    Key="Ctrl-1",
                    CommandName = "SidebarTabActivationCommand",
                    CommandParameter = "1"
                },
                new AppKeyBinding
                {
                    Id= "SidebarTabActivationCommand2",
                    Key="Ctrl-2",
                    CommandName = "SidebarTabActivationCommand",
                    CommandParameter = "2"
                },
                new AppKeyBinding
                {
                    Id= "SidebarTabActivationCommand3",
                    Key="Ctrl-3",
                    CommandName = "SidebarTabActivationCommand",
                    CommandParameter = "3"
                },
                new AppKeyBinding
                {
                    Id= "SidebarTabActivationCommand4",
                    Key="Ctrl-4",
                    CommandName = "SidebarTabActivationCommand",
                    CommandParameter = "4"
                }

            };
        }
    }
}
