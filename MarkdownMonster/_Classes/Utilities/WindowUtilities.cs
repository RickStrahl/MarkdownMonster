using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// WPF Helpers for MM
    /// </summary>
    public class WindowUtilities
    {
        /// <summary>
        /// Idle loop to let events fire in the UI
        /// 
        /// Use SPARINGLY or not at all if there is a better way
        /// but there are a few places where this is required.
        /// </summary>
        public static void DoEvents()
        {
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new EmptyDelegate(delegate { }));
        }
        private delegate void EmptyDelegate();
    }
}
