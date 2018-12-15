using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using FontAwesome.WPF;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Displays statusbar text formats and timeouts in a reusable way.
    /// Assumes a specific structure for the status bar as it updates
    /// 
    /// </summary>
    public class StatusBarHelper
    {
        DebounceDispatcher debounce = new DebounceDispatcher();
        
        public TextBlock StatusText { get; set; }

        public FontAwesome.WPF.FontAwesome StatusIcon { get; set; }

        public StatusBarHelper(TextBlock statusText, FontAwesome.WPF.FontAwesome statusIcon)
        {            
            StatusText = statusText;
            StatusIcon = statusIcon;
        }

        
        public void ShowStatus(string message = null, int milliSeconds = 0,
            FontAwesomeIcon icon = FontAwesomeIcon.None,
            Color color = default(Color),
            bool spin = false)
        {
            // run in a dispatcher here to force the UI to be updated
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                if (color == default(Color))
                    color = Colors.Green;

                if (icon != FontAwesomeIcon.None)
                    SetStatusIcon(icon, color, spin);

                if (message == null)
                {
                    message = "Ready";
                    SetStatusIcon();
                }

                StatusText.Text = message;

                if (milliSeconds > 0)
                {
                    // debounce rather than delay so if something else displays
                    // a message the delay timer is 'reset'
                    debounce.Debounce(milliSeconds, (p) => ShowStatus(null, 0), null);
                }
            },DispatcherPriority.Background);

            //WindowUtilities.DoEvents();
        }

        /// <summary>
        /// Displays an error message using common defaults for a timeout milliseconds
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="timeout">optional timeout</param>
        /// <param name="icon">optional icon (warning)</param>
        /// <param name="color">optional color (firebrick)</param>
        public void ShowStatusError(string message, int timeout = -1, FontAwesomeIcon icon = FontAwesomeIcon.Warning, Color color = default(Color))
        {
            if (timeout == -1)
                timeout = mmApp.Configuration.StatusMessageTimeout;

            if (color == default(Color))
                color = Colors.OrangeRed;

            ShowStatus(message, timeout, icon, color);
        }

        /// <summary>
        /// Shows a success message with a green check icon for the timeout
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="timeout">optional timeout</param>
        /// <param name="icon">optional icon (warning)</param>
        /// <param name="color">optional color (firebrick)</param>
        public void ShowStatusSuccess(string message, int timeout = -1, FontAwesomeIcon icon = FontAwesomeIcon.CheckCircle, Color color = default(Color))
        {
            if (timeout == -1)
                timeout = mmApp.Configuration.StatusMessageTimeout;

            if (color == default(Color))
                color = Colors.LimeGreen;

            ShowStatus(message, timeout, icon, color);
        }


        /// <summary>
        /// Displays an Progress message using common defaults including a spinning icon
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="timeout">optional timeout</param>
        /// <param name="icon">optional icon (warning)</param>
        /// <param name="color">optional color (firebrick)</param>
        /// <param name="spin"></param>
        public void ShowStatusProgress(string message, int timeout = -1, FontAwesomeIcon icon = FontAwesomeIcon.CircleOutlineNotch, Color color = default(Color), bool spin = true)
        {
            if (timeout == -1)
                timeout = mmApp.Configuration.StatusMessageTimeout;

            if (color == default(Color))
                color = Colors.Goldenrod;

            ShowStatus(message, timeout, icon, color, spin);
        }

        /// <summary>
        /// Status the statusbar icon on the left bottom to some indicator
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        /// <param name="spin"></param>
        public void SetStatusIcon(FontAwesomeIcon icon, Color color, bool spin = false)
        {
            StatusIcon.Icon = icon;
            StatusIcon.Foreground = new SolidColorBrush(color);
            if (spin)
                StatusIcon.SpinDuration = 2;

            StatusIcon.Spin = spin;
        }

        /// <summary>
        /// Resets the Status bar icon on the left to its default green circle
        /// </summary>
        public void SetStatusIcon()
        {
            StatusIcon.Icon = FontAwesomeIcon.Circle;
            StatusIcon.Foreground = new SolidColorBrush(Colors.Green);
            StatusIcon.Spin = false;
            StatusIcon.SpinDuration = 0;
            StatusIcon.StopSpin();
        }
    }
}
