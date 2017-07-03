using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{

    /// <summary>
    /// Extension methods for the System.Windows.Threading.Dispatcher object
    /// that provides an easy way for delayed execution of code.
    /// </summary>
    public static class DispatcherExtensions
    {
        
        /// <summary>
        /// Dispatcher.Delay Extension method that delay executes 
        /// an action. 
        /// </summary>        
        /// <param name="disp">The Dispatcher instance</param>
        /// <param name="delayMs">milliseconds to delay before executing</param>
        /// <param name="action">Single parm action to perform ; (arg) => {}</param>
        ///<param name="parm">The parameter to pass</param>
        public static void Delay(this Dispatcher disp, int delayMs,
                                 Action<object> action, object parm = null)
        {
            var ignore = Task.Delay(delayMs).ContinueWith((t) =>
            {
                disp.Invoke(action, parm);
            });
        }
        
        /// <summary>
        /// Dispatcher.Delay Extension method that delay executes 
        /// an action. 
        /// </summary>        
        /// <param name="disp">The Dispatcher instance</param>
        /// <param name="delayMs">milliseconds to delay before executing</param>
        /// <param name="action">Single parm action to perform ; (arg) => {}</param>
        /// <param name="parm">The parameter to pass</param>
        /// <param name="priority">optional Dispatcher priority</param>
        public static void DelayWithPriority(this Dispatcher disp, int delayMs,
                          Action<object> action, object parm = null, 
                          DispatcherPriority priority = DispatcherPriority.ApplicationIdle)
        {
            var ignore = Task.Delay(delayMs).ContinueWith((t) =>
            {
                disp.BeginInvoke(action, priority, parm);
            });
            
        }

        /// <summary>
        /// Dispatcher.Delay Extension method that delay executes 
        /// an action. This version awaits both the delay and the
        /// synchonized action
        /// </summary>        
        /// <param name="disp">The Dispatcher instance</param>
        /// <param name="delayMs">milliseconds to delay before executing</param>
        /// <param name="action">Single parm action to perform ; (arg) => {}</param>
        /// <param name="parm">The parameter to pass</param>
        /// <param name="priority">optional Dispatcher priority</param>
        public static async Task DelayAsync(this Dispatcher disp, int delayMs,
                          Action<object> action, object parm = null,
                          DispatcherPriority priority = DispatcherPriority.ApplicationIdle)
        {
            await Task.Delay(delayMs).ConfigureAwait(false);
            await disp.BeginInvoke(action, priority, parm);
        }
    }


    /// <summary>
    /// Debounces events by a given timeout. Effectively delays execution of
    /// the provided action until after the timeout has passed and no further
    /// events have fired within the timeout period. Any events fired before
    /// the last are discarded.
    /// 
    /// Use this to ensure that events aren't handled too frequently and are
    /// delayed until an operation is completed (like keyboard input or
    /// sizing operations for example).
    /// </summary>
    public class DebounceDispatcher
    {
        private DispatcherTimer timer;

        /// <summary>
        /// Debounce an actual event. Essentially wrap the logic you 
        /// would normally use in your event code in an Action of object
        /// and pass to this method to debounce the event.
        /// Example: https://gist.github.com/RickStrahl/0519b678f3294e27891f4d4f0608519a
        /// </summary>
        /// <param name="timeout">Timeout in Milliseconds</param>
        /// <param name="action">Action<object> to fire when debounced event fires</object></param>
        /// <param name="param">optional parameter</param>
        /// <param name="priority">optional priorty for the dispatcher</param>
        /// <param name="disp">optional dispatcher. If not passed or null CurrentDispatcher is used.</param>
        public void Debounce(int timeout, Action<object> action,
            object param = null,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
            Dispatcher disp = null)
        {
            if (disp == null)
                disp = Dispatcher.CurrentDispatcher;

            if (timer == null)
            {
                timer = new DispatcherTimer(TimeSpan.FromMilliseconds(timeout), priority, (s, e) =>
                {
                    timer.IsEnabled = false;
                    action.Invoke(param);
                }, disp)
                { IsEnabled = true };
            }
            else
                timer.IsEnabled = false;

            timer.IsEnabled = true;
        }
    }
}
