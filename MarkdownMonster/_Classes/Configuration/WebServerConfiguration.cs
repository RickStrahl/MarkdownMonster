using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarkdownMonster.Annotations;
using MarkdownMonster.Services;
using Newtonsoft.Json;

namespace MarkdownMonster.Configuration
{
    /// <summary>
    /// Settings that control the local built-in Web Server that can
    /// be used to open Markdown Monster from Web pages or any other
    /// client that can make HTTP requests. Ooperations supported are:
    ///
    /// Open - same as command line operations
    /// 
    /// By default this server is off and can be started via
    /// Application Protocol:
    ///
    /// markdownmonster:webserver
    ///
    /// or by autostarting on startup
    /// </summary>
    public class WebServerConfiguration : INotifyPropertyChanged
    {
        /// <summary>
        /// Port used for the Socket Server. Note if you change this value
        /// any script code used to access the server requires that you
        /// also change the client port!
        /// </summary>
        public int Port
        {
            get => _port;
            set
            {
                if (value == _port) return;
                _port = value;
                OnPropertyChanged();
            }
        }

        private int _port = 5009;


        /// <summary>
        /// Determines whether the internal Web Socket Server is automatically
        /// started when Markdown Monster starts.
        ///
        /// The WebSocket server allows Web Browsers and external applications
        /// using WebSockets to open new documents in Markdown Monster for editing.
        /// </summary>
        public bool AutoStart
        {
            get { return _AutoStart; }
            set
            {
                if (value == _AutoStart) return;
                _AutoStart = value;
                OnPropertyChanged(nameof(AutoStart));
            }
        }
        private bool _AutoStart;
        

        /// <summary>
        /// Determines whether the socket is automatically started when MArkdown Monster Starts
        /// </summary>
        [JsonIgnore]
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (value == _isRunning) return;

                _isRunning = value;

                if (!_isRunning)
                    WebServerLauncher.StopMarkdownMonsterWebServer();
                else
                    WebServerLauncher.StartMarkdownMonsterWebServer();

                OnPropertyChanged(nameof(IsRunning));
            }
        }

        private bool _isRunning = false;
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
