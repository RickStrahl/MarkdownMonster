using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.Annotations;
using MarkdownMonster.WebSockets;
using Newtonsoft.Json;

namespace MarkdownMonster._Classes.Configuration
{
    /// <summary>
    /// Settings that control the Web Socket server that can be used
    /// for browser to Markdown Monster communication.
    ///
    /// By default this server is off and can be started via
    /// Application Protocol:
    ///
    /// markdownmonster:websocketserver
    ///
    /// or by autostarting on startup
    /// </summary>
    public class WebSocketConfiguration : INotifyPropertyChanged
    {
        /// <summary>
        /// Port used for the Socket Server. Note if you change this value
        /// any script code used to access the server requires that you
        /// also change the client port!
        /// </summary>
        public int SocketPort
        {
            get => _socketPort;
            set
            {
                if (value == _socketPort) return;
                _socketPort = value;
                OnPropertyChanged();
            }
        }

        private int _socketPort = 5009;


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
                    WebSocketServer.StopMarkdownMonsterWebSocketServer();
                else
                    WebSocketServer.StartMarkdownMonsterWebSocketServer();

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
