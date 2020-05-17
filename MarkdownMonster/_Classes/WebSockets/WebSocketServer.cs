//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Net;
//using System.Net.Security;
//using System.Net.Sockets;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Windows.Documents;
//using System.Windows.Forms;
//using LibGit2Sharp;
//using ReverseMarkdown;

//namespace MarkdownMonster.WebSockets
//{
//    public class WebSocketServer
//    {

//        string IpAddress { get; set; } = "127.0.0.1";
//        int ServerPort { get; set; } = 5009;

//        /// <summary>
//        /// If true uses SSL
//        /// </summary>
//        bool Secure {get; set; }

//        Thread SocketThread { get; set; }
//        TcpListener TcpServer { get; set; }

//        TcpClient client = null;

//        /// <summary>
//        /// Last exception that occurred when starting the server or intercepting
//        /// request data.
//        /// </summary>
//        public Exception LastException { get; set; }



//        public bool StartServer(string ipAddress = "127.0.0.1", int serverPort = 5009)
//        {
//            try
//            {
//                SocketThread = new Thread(RunServer);
//                SocketThread.Start();
//            }
//            catch (Exception ex)
//            {
//                LastException = ex;
//                SocketThread = null;
//                return false;
//            }

//            return true;
//        }

//        public void StopServer()
//        {
//            if (client != null)
//            {
//                CloseSocketConnection();
//            }

//            TcpServer?.Stop();
//            TcpServer = null;

//            try
//            {
//                SocketThread?.Abort();
//            }
//            finally
//            {
//                SocketThread = null;
//            }

//        }

//        /// <summary>
//        /// Called when a Web Socket message has been captured.
//        /// This method returns the raw binary data.
//        /// 
//        /// Important: This occurs on a non-synchronized thread
//        /// so if you're using this in UI make sure to marshal
//        /// to the UI thread.
//        /// </summary>
//        public event Action<byte[]> OnBinaryMessage;

//        /// <summary>
//        /// Called when a Web Socket message has been captured.
//        /// This method returns the data as a string.
//        /// 
//        /// Important: This occurs on a non-synchronized thread
//        /// so if you're using this in UI make sure to marshal
//        /// to the UI thread.
//        /// </summary>
//        public event Action<string> OnStringMessage;

//        //public Action OnDisconnected;
//        //public Action<string> OnConnected;


//        public void RunServer()
//        {
//            TcpServer?.Stop();

//            TcpServer = new TcpListener(IPAddress.Parse(IpAddress), ServerPort);
//            TcpServer.Start();


//            Request wrapper = null;
            
//            // enter to an infinite cycle to be able to handle every change in stream
//            while (true)
//            {
//                try
//                {
//                    if (client == null || !client.Connected)
//                        client = TcpServer.AcceptTcpClient();
//                    if (wrapper == null)
//                        wrapper = OpenStream( Secure);

//                    var capturedData = new List<byte>();

//                    while (!wrapper.NetworkStream.DataAvailable)
//                    {
//                        if (TcpServer == null)
//                            return;

//                        Thread.Sleep(1); // don't hog CPU
//                        while (client.Available < 3)
//                        {
//                            Thread.Sleep(10);
//                        }
//                    }

//                    // Read initial buffer
//                    byte[] bytes = new byte[client.Available];
//                    wrapper.Stream.Read(bytes, 0, client.Available);

//                    // check for handshake only if the block isn't very big
//                    if (bytes.Length < 1028)
//                    {
//                        string s = Encoding.UTF8.GetString(bytes);

//                        if (Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase))
//                        {
//                            // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
//                            // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
//                            // 3. Compute SHA-1 and Base64 hash of the new value
//                            // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
//                            string swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
//                            string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
//                            byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create()
//                                .ComputeHash(Encoding.UTF8.GetBytes(swka));
//                            string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

//                            // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
//                            byte[] response = Encoding.UTF8.GetBytes(
//                                "HTTP/1.1 101 Switching Protocols\r\n" +
//                                "Connection: Upgrade\r\n" +
//                                "Upgrade: websocket\r\n" +
//                                "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

//                            wrapper.Stream.Write(response, 0, response.Length);
//                            continue; // done here
//                        }
//                    }

//                    // Capture Payload data
//                    capturedData.AddRange(bytes);


//                    // Read multiple buffers for large content
//                    while (wrapper.NetworkStream.DataAvailable)
//                    {
//                        try
//                        {
//                            wrapper.Stream.Read(bytes, 0, client.Available);
//                            capturedData.AddRange(bytes);
//                        }
//                        catch
//                        {
//                            // TODO: needs specific exception handling for disconnects
//                        }
//                    }

//                    bytes = capturedData.ToArray();

//                    if (bytes.Length < 0)
//                        continue; // nothing to do



//                    List<byte> output = new List<byte>();
//                    while (true)
//                    {
//                        bool fin = (bytes[0] & 0b10000000) != 0;
//                        bool mask = (bytes[1] & 0b10000000) !=
//                                    0; // must be true, "All messages from the client to the server have this bit set"
//                        int opcode = bytes[0] & 0b00001111; // expecting 1 - text message
//                        int msglen = bytes[1] - 128, // & 0111 1111
//                            offset = 2;


//                        if (msglen == 126)
//                        {
//                            // was ToUInt16(bytes, offset) but the result is incorrect
//                            msglen = BitConverter.ToUInt16(new byte[] {bytes[3], bytes[2]}, 0);
//                            offset = 4;
//                        }
//                        else if (msglen == 127)
//                        {
//                            // TODO: This is broken in Chromium!!! returns 0 0 0 0 0 2 0 0 for any content with ml 127
//                            msglen = (int) BitConverter.ToUInt64(
//                                new byte[]
//                                {
//                                    bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2]
//                                },
//                                0);
//                            offset = 10;
//                        }

//                        if (msglen == 0)
//                        {
//                            // nothing to do - empty message
//                        }
//                        else if (mask)
//                        {
//                            byte[] decoded = new byte[msglen];
//                            byte[] masks = new byte[4]
//                            {
//                                bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3]
//                            };
//                            offset += 4;

//                            for (int i = 0; i < msglen; ++i)
//                                decoded[i] = (byte) (bytes[offset + i] ^ masks[i % 4]);

//                            output.AddRange(decoded);

//                            if (!fin)
//                            {
//                                // keep going with the next frame
//                                var off = msglen + offset;
//                                var spanBytes = bytes.AsSpan<byte>(off);
//                                bytes = spanBytes.ToArray(); //Array.Copy(bytes, off, b2, 0, bytes.Length - off);
//                            }
//                            else
//                                break;
//                        }
//                    }

//                    // Notifications if connected
//                    OnBinaryMessage?.Invoke(output.ToArray());
//                    OnStringMessage?.Invoke(Convert.ToBase64String(output.ToArray()));
//                }
//                catch
//                {
//                    // keep it going - otherwise the thread crashes and the server goes away
//                }
//            }
//        }

//        /// <summary>
//        /// Returns a raw stream which can be SSL encoded and the original
//        /// Network stream so both are accessible. Use the raw stream
//        /// for read/write and the Network Stream for checking data
//        /// availability
//        /// </summary>
//        /// <param name="secure"></param>
//        /// <returns></returns>
//        Request OpenStream(bool secure = false)
//        {
//            var wrapper = new Request() {NetworkStream = client.GetStream()  };

//            if (secure)
//                wrapper.Stream = new SslStream(wrapper.NetworkStream);
//            else
//                wrapper.Stream = wrapper.NetworkStream;

//            return wrapper;
//        }


//        #region Send and Close Operations
//        //Random Random = new Random();

//        public void CloseSocketConnection()
//        {
//            if (client == null || !client.Connected) return;

//            try
//            {
//                var mask = new byte[4];  // empty mask
//                SendMessage(client, new byte[] { }, 0x8, false, mask);
//                client.Close();
//            }
//            catch  { }
//        }


//        static void SendMessage(TcpClient client, byte[] payload, int opcode, bool masking, byte[] mask)
//        {
//            if (client == null || !client.Connected)
//                return;

//            if (masking && mask == null) throw new ArgumentException(nameof(mask));

//            using (var packet = new MemoryStream())
//            {
//                byte firstbyte = 0b0_0_0_0_0000; // fin | rsv1 | rsv2 | rsv3 | [ OPCODE | OPCODE | OPCODE | OPCODE ]

//                firstbyte |= 0b1_0_0_0_0000; // fin
//                //firstbyte |= 0b0_1_0_0_0000; // rsv1
//                //firstbyte |= 0b0_0_1_0_0000; // rsv2
//                //firstbyte |= 0b0_0_0_1_0000; // rsv3

//                firstbyte += (byte) opcode; // Text
//                packet.WriteByte(firstbyte);

//                // Set bit: bytes[byteIndex] |= mask;

//                byte secondbyte = 0b0_0000000; // mask | [SIZE | SIZE  | SIZE  | SIZE  | SIZE  | SIZE | SIZE]

//                if (masking)
//                    secondbyte |= 0b1_0000000; // mask

//                if (payload.LongLength <= 0b0_1111101) // 125
//                {
//                    secondbyte |= (byte) payload.Length;
//                    packet.WriteByte(secondbyte);
//                }
//                else if (payload.LongLength <= UInt16.MaxValue) // If length takes 2 bytes
//                {
//                    secondbyte |= 0b0_1111110; // 126
//                    packet.WriteByte(secondbyte);

//                    var len = BitConverter.GetBytes(payload.LongLength);
//                    Array.Reverse(len, 0, 2);
//                    packet.Write(len, 0, 2);
//                }
//                else // if (payload.LongLength <= Int64.MaxValue) // If length takes 8 bytes
//                {
//                    secondbyte |= 0b0_1111111; // 127
//                    packet.WriteByte(secondbyte);

//                    var len = BitConverter.GetBytes(payload.LongLength);
//                    Array.Reverse(len, 0, 8);
//                    packet.Write(len, 0, 8);
//                }

//                if (masking)
//                {
//                    packet.Write(mask, 0, 4);
//                    payload = ApplyMask(payload, mask);
//                }

//                // Write all data to the packet
//                packet.Write(payload, 0, payload.Length);

//                // Get client's stream
//                var stream = client.GetStream();

//                var finalPacket = packet.ToArray();
//                Console.WriteLine($@"SENT: {BitConverter.ToString(finalPacket)}");

//                // Send the packet
//                foreach (var b in finalPacket)
//                    stream.WriteByte(b);
//            }
//        }

//        static byte[] ApplyMask(IReadOnlyList<byte> msg, IReadOnlyList<byte> mask)
//        {
//            var decoded = new byte[msg.Count];
//            for (var i = 0; i < msg.Count; i++)
//                decoded[i] = (byte) (msg[i] ^ mask[i % 4]);
//            return decoded;
//        }
//        #endregion

//        #region Markdown Monster Start/Stop Helpers

//        /// <summary>
//        /// Starts the Web Socket server and attaches an instance to
//        /// the Markdown Monster main window.
//        /// </summary>
//        public static WebSocketServer StartMarkdownMonsterWebSocketServer(bool force = false)
//        {
//            var window = mmApp.Model.Window;
            
//            if (!force && window.WebSocketServer != null)
//                return window.WebSocketServer; // already running

//            // if already running stop first then restart
//            window.WebSocketServer?.StopServer();

//            var server = new WebSocketServer();
//            server.OnBinaryMessage += OnBinaryMessageInternal;
//            server.StartServer();
//            window.WebSocketServer = server;

            
//            mmApp.Model.Configuration.WebSocket.IsRunning = true;

//            window.ShowStatusSuccess("The WebSocket server has been started.");
//            return window.WebSocketServer;
//        }

//        /// <summary>
//        /// Shuts down the Web Socket server on the Markdown Monster instance
//        /// </summary>
//        public static void StopMarkdownMonsterWebSocketServer()
//        {
//            var window = mmApp.Model.Window;

//            var ws = window.WebSocketServer;
//            if (ws != null)
//            {
//                ws.OnBinaryMessage -= OnBinaryMessageInternal;
//                ws.StopServer();
//                window.WebSocketServer = null;
//            }
//            mmApp.Model.Configuration.WebSocket.IsRunning = false;
//            window.ShowStatusSuccess("WebSocket server has been stopped.");
//        }

//        private static void OnBinaryMessageInternal(byte[] bytes)
//        {
//            App.CommandArgs = new[] {"untitled.base64," + Convert.ToBase64String(bytes)};
//            mmApp.Model.Window.Dispatcher.InvokeAsync(() => mmApp.Model.Window.OpenFilesFromCommandLine());
//        }

//        #endregion

//    }

//    public class Request
//    {
//        public NetworkStream NetworkStream;
//        public Stream Stream;
//    }
//}
