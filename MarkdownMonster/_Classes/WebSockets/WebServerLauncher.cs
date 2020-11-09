using MarkdownMonster._Classes;

namespace MarkdownMonster.Services
{

    /// <summary>
    /// The Markdown Monster Web Server Launcher that manages starting
    /// and stopping of the Web server and routing requests.
    /// </summary>
    public static class WebServerLauncher
    {
        #region Markdown Monster Start/Stop Helpers

        /// <summary>
        /// Starts the Web Socket server and attaches an instance to
        /// the Markdown Monster main window.
        /// </summary>
        public static WebServer StartMarkdownMonsterWebServer(bool force = false)
        {

            var window = mmApp.Model.Window;

            if (!force && window.WebServer != null)
            {
                window.ShowStatusSuccess("The Web server is already running.");
                return window.WebServer; // already running
            }

            // if already running stop first then restart
            window.WebServer?.StopServer();

            var server = new WebServer();
            window.WebServer = server;
            server.OnMarkdownMonsterOperation = WebServerRequestHandler.OnMarkdownMonsterOperationHandler;

            server.StartServer();

            window.ShowStatusSuccess("The Web server  has been started.");
            return window.WebServer;
        }

        /// <summary>
        /// Shuts down the Web Socket server on the Markdown Monster instance
        /// </summary>
        public static void StopMarkdownMonsterWebServer()
        {
            var window = mmApp.Model.Window;

            var ws = window.WebServer;
            if (ws != null)
            {
                ws.OnMarkdownMonsterOperation = null;
                ws.StopServer();
                window.WebServer = null;
            }
            mmApp.Model.Configuration.WebServer.IsRunning = false;
            window.ShowStatusSuccess("The Web server has been stopped.");
        }

       

        #endregion
    }
}
