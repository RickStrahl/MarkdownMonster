using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MarkdownMonster
{
    /// <summary>
    /// Base Command class to allow handling of commands generically
    /// </summary>
    public class CommandBase : ICommand
    {

        Action<object, ICommand> _execute;
        private Func<object, ICommand,bool> _canExecute;
        private Func<object, ICommand, bool> _previewExecute;

        public string Caption { get; set; }

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
            if (_canExecute != null)
                return  _canExecute.Invoke(parameter, this);

            return true;
        }


        public bool PreviewExecute(object parameter)
        {
            if (_previewExecute != null)
                return _previewExecute.Invoke(parameter,this);

            return true;
        }

        public void Execute(object parameter)
        {
            if (PreviewExecute(parameter))
                _execute?.Invoke(parameter, this);
        }    
        

        public void InvalidateCanExecute()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        public event EventHandler CanExecuteChanged;
    }
}
