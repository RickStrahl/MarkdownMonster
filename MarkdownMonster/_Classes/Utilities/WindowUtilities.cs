using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MarkdownMonster.Windows
{
    public class WindowUtilities
    {
        public static void DoEvents()
        {
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new EmptyDelegate(delegate { }));
        }
        private delegate void EmptyDelegate();
    }
}
