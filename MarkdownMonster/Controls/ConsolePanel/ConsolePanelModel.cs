using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MarkdownMonster.Controls
{
    public class ConsolePanel
    {
        private AppModel Model { get; set; }


        public ConsolePanel()
        {
            Model = mmApp.Model;
        }

        /// <summary>
        /// Writes a line with linefeed to the console
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public void WriteLine(string text, Brush color=null)
        {

            Write(text + "\n",color);
        }


        /// <summary>
        /// Writes text tot he console panel without a line feed.
        /// Next write continues on the active line
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public void Write(string text, Brush color = null)
        {
            if (!Model.WindowLayout.IsConsolePanelVisible)
                Model.WindowLayout.IsConsolePanelVisible = true;


            Model.WindowLayout.ConsoleText += text;
            Model.Window.ConsolePanelScroll.ScrollToVerticalOffset(99999999);
        }

        /// <summary>
        /// Clears the content of the console window.
        /// </summary>
        public void Clear()
        {
            Model.WindowLayout.ConsoleText = null;
        }

        /// <summary>
        /// Makes the panel visible
        /// </summary>
        public void Show()
        {
            Model.WindowLayout.IsConsolePanelVisible = true;
        }


        /// <summary>
        /// Hides the panel
        /// </summary>
        public void Hide()
        {
            Model.WindowLayout.IsConsolePanelVisible = false;
        }

    }
}
