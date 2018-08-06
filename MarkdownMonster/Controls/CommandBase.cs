using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MarkdownMonster.Annotations;

namespace MarkdownMonster
{
    /// <summary>
    /// Base Command class to allow handling of commands generically
    /// </summary>
    public class CommandBase : ICommand, INotifyPropertyChanged
    {
        private readonly Action<object, ICommand> _execute;
        private readonly Func<object, ICommand, bool> _canExecute;
        private readonly Func<object, ICommand, bool> _previewExecute;

        public string Caption { get; set; }

        private string _keyboardShortcut;

        public string KeyboardShortcut
        {
            get => _keyboardShortcut;
            set
            {
                if (value == _keyboardShortcut) return;
                _keyboardShortcut = value;        
                OnPropertyChanged(nameof(KeyboardShortcut));
            }
        }

        public string ToolTip { get; set; }

        /// <summary>
        /// Constructor that allows you to hook up each of the command events
        /// </summary>
        /// <param name="execute">Function that takes action</param>
        /// <param name="canExecute">Function that determines whether the action is enabled</param>
        /// <param name="previewExecute">Function fire just prior to Execute to determine whether execute should fire. Return true or fals to indicate whether Execute should run or not. Allows for actions like cancelling or conditional execution.</param>
        public CommandBase(Action<object, ICommand> execute, Func<object, ICommand, bool> canExecute = null, Func<object, ICommand, bool> previewExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
            _previewExecute = previewExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute.Invoke(parameter, this);
        }

        public bool PreviewExecute(object parameter)
        {
            return _previewExecute == null || _previewExecute.Invoke(parameter, this);
        }

        public void Execute(object parameter)
        {
            if (PreviewExecute(parameter) && CanExecute(parameter))
                _execute?.Invoke(parameter, this);
        }


        /// <summary>
        /// Event hook that ensures that CanExecute gets called as needed
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecute != null)
                CommandManager.RequerySuggested -= value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
