using System.Diagnostics;
using Newtonsoft.Json;

namespace MarkdownMonster.Utilities
{
    /// <summary>
    /// Holds a specific key binding combination
    /// </summary>
    [DebuggerDisplay("{Key} - {CommandName}")]
    public class AppKeyBinding
    {
        
        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                    return CommandName;

                return _id;
            }
            set => _id = value;
        }
        private string _id;

        public string Key { get; set; }


        /// <summary>
        /// Name of the Command to execute. Used in JavaScript
        /// (with lower case 1st letter) to find the appropriate
        /// command handler
        /// </summary>
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

        [JsonIgnore] public bool HasJavaScriptHandler { get; set; }
    }
}
