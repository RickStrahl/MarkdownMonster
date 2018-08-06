using System.Diagnostics;
using Newtonsoft.Json;

namespace MarkdownMonster.Utilities
{
    /// <summary>
    /// Holds a specific key binding combination
    /// </summary>
    [DebuggerDisplay("{Key}")]
    public class AppKeyBinding
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