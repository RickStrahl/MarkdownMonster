using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MarkdownMonster.Controls;
using Newtonsoft.Json;
using Westwind.Utilities;

namespace MarkdownMonster.Services
{
    public class WebServer
    {
        /// <summary>
        /// Connection Timeout in milliseconds. Resets connection if waiting
        /// longer than this timeout.
        /// </summary>
        public int ConnectionTimeout { get; set; } = 10_000;
        bool Cancelled { get; set; } = false;


        Thread SocketThread { get; set; }
        TcpListener TcpServer { get; set; }

        WebRequestContext RequestContext = null;

        string IpAddress { get; set; } = "127.0.0.1";
        int ServerPort { get; set; } = 5009;

        /// <summary>
        /// If true uses SSL
        /// </summary>
        bool Secure { get; set; }

        public Func<WebServerOperation,WebServerResult> OnMarkdownMonsterOperation { get; set; }

        /// <summary>
        /// Last exception that occurred when starting the server or intercepting
        /// request data.
        /// </summary>
        public Exception LastException { get; set; }


        #region Start and Stop Server 

        public bool StartServer(string ipAddress = "127.0.0.1", int serverPort = 5009)
        {
            try
            {
                SocketThread = new Thread(RunServer);
                SocketThread.Start();
                mmApp.Configuration.WebServer.IsRunning = true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                SocketThread = null;
                return false;
            }

            return true;
        }

        public void StopServer()
        {
            Cancelled = true;
            Thread.Sleep(80);

            CloseConnection();

            try
            {
                TcpServer?.Stop();
                TcpServer = null;
            }
            catch
            {
                TcpServer = null;
            }

            try
            {
                SocketThread?.Abort();
            }
            finally
            {
                SocketThread = null;
            }

        }


        public void RunServer()
        {
            TcpServer?.Stop();

            try
            {
                TcpServer = new TcpListener(IPAddress.Parse(IpAddress), ServerPort);
                TcpServer.Start();
            }
            catch(Exception ex)
            {
                
                var window = mmApp.Model?.Window;
                if (window != null)
                    window.Dispatcher.InvokeAsync( ()=>
                    {
                        mmApp.Model.Configuration.WebServer.IsRunning = false;
                        window.ShowStatusError(
                                $"Failed to start Web Server on `localhost:{ServerPort}`: {ex.Message}");
                    });
                return;
            }


            // enter to an infinite cycle to be able to handle every change in stream
            while (!Cancelled)
            {
                try
                {
                    RequestContext = OpenConnection();
                    var capturedData = new List<byte>();

                    
                    WaitForConnectionData();

                    
                    if (!RequestContext.Connection.Connected)
                        continue;

                    if ( RequestContext.Connection.Available == 0)
                        continue;

                    if (!ParseRequest())
                        continue;


                    // Send CORS header so this can be accessed
                    if (RequestContext.Verb == "OPTIONS")
                    {
                        WriteResponseInternal(null,
                            "HTTP/1.1 200 OK\r\n" +
                            "Access-Control-Allow-Origin: *\r\n" +
                            "Access-Control-Allow-Methods: GET,POST,PUT,OPTIONS\r\n" +
                            "Access-Control-Allow-Headers: *\r\n");

                        continue; // done here
                    }
                    else if (RequestContext.Verb == "POST" &&
                            (RequestContext.Path == "/markdownmonster" || RequestContext.Path.StartsWith("/markdownmonster/")))
                    {
                        WebServerResult result = new WebServerResult();

                        try
                        {
                            var operation = JsonConvert.DeserializeObject(RequestContext.RequestContent, typeof(WebServerOperation)) as WebServerOperation;
                            if (operation == null)
                                throw new ArgumentException("Invalid json request data: " + RequestContext.RequestContent);

                            try
                            {
                                result = OnMarkdownMonsterOperation?.Invoke(operation);
                            }
                            catch
                            {
                                mmApp.Model?.Window?.ShowStatusError("Web Server Client Request failed. Operation: " + operation.Operation);
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteErrorResponse("Markdown Monster Message Processing failed: " + ex.Message + "\r\n\r\n" +
                                               RequestContext.RequestContent);
                            continue;
                        }

                        WriteResponse(result);
                    }
                    else if (RequestContext.Path.StartsWith("/ping"))
                    {
                        WriteDataResponse("OK");
                    }
                    else
                    {
                        var result = new WebServerResult("Invalid URL access", 404M);
                        WriteResponse(result);
                    }
                }
                catch (Exception ex)
                {
                    WriteErrorResponse("An error occurred: " + ex.Message);
                }
                finally
                {
                    // close connection
                    CloseConnection();
                    RequestContext = null;
                }
            }

        }

        #endregion

        #region Connection Operations

        /// <summary>
        /// Returns a raw stream which can be SSL encoded and the original
        /// Network stream so both are accessible. Use the raw stream
        /// for read/write and the Network Stream for checking data
        /// availability
        /// </summary>
        /// <param name="secure"></param>
        /// <returns></returns>
        WebRequestContext OpenConnection(bool secure = false)
        {
            WebRequestContext requestContext = new WebRequestContext();
            try
            {
                requestContext.Connection = TcpServer.AcceptTcpClient();
                requestContext.Connection.ReceiveTimeout = 3000;
                requestContext.Connection.SendTimeout = 3000;

                requestContext.NetworkStream = requestContext.Connection.GetStream();

                if (secure)
                    requestContext.Stream = new SslStream(requestContext.NetworkStream);
                else
                    requestContext.Stream = requestContext.NetworkStream;
            }
            catch
            {
                return null;
            }

            return requestContext;
        }

        void CloseConnection()
        {
            // close connection			
            RequestContext?.Close();
            RequestContext = null;
        }

        #endregion

        #region Receive Processing
        private void WaitForConnectionData()
        {

            var dt = DateTime.UtcNow.AddMilliseconds(ConnectionTimeout);

            while (RequestContext.NetworkStream != null &&
                   !RequestContext.NetworkStream.DataAvailable)
            {
                
                if (TcpServer == null)
                    return;

                Thread.Sleep(1); // don't hog CPU

                if(RequestContext?.Connection == null)
                     return;

                
                while ( RequestContext?.Connection != null &&
                        RequestContext.Connection.Connected &&
                        RequestContext.Connection.Available < 3)
                {
                    if (dt < DateTime.UtcNow)
                        return;  // break and restart connection in case it's hung

                    Thread.Sleep(10);
                }
            }
        }

        private bool ParseRequest()
        {
            if (RequestContext.Connection == null ||
                !RequestContext.Connection.Connected)
                return false;

            // Read initial buffer to get the headers
            int available = RequestContext.Connection.Available;
            byte[] bytes = new byte[available];
            int bytesRead = RequestContext.Stream.Read(bytes, 0, available);
            if (bytesRead < available)
                Array.Resize(ref bytes, bytesRead);

            var sb = new StringBuilder();
            sb.Append(Encoding.UTF8.GetString(bytes));

            var firstLine = GetFirstHeaderLine(sb.ToString());
            if (firstLine == null)
                return false;  // invalid

            //Read multiple buffers for large content
            while (RequestContext.NetworkStream.DataAvailable)
            {
                try
                {
                    RequestContext.Stream.Read(bytes, 0, RequestContext.Connection.Available);
                    sb.Append(Encoding.UTF8.GetString(bytes));
                }
                catch
                {
                    // TODO: needs specific exception handling for disconnects
                    return false;
                }
            }


            var tokens = firstLine.Split(' ');
            if (tokens.Length < 3)
                return false; // invalid GET /path HTTP/1.1

            RequestContext.Verb = tokens[0].ToUpper();
            RequestContext.Path = tokens[1];

            string fullRequest = sb.ToString();
            int at = fullRequest.IndexOf("\r\n\r\n");
            if (at < 1)
                return false;

            RequestContext.RequestHeaders = fullRequest.Substring(0, at);
            if (fullRequest.Length > at + 4)
                RequestContext.RequestContent = fullRequest.Substring(at + 4);


            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="data"></param>
        /// <param name="type"></param>
        public void HandleOperation(string action, string data = null, string type = "text")
        {
            var operation = new WebServerOperation() {Operation = action, Data = data, Type="text" };
            OnMarkdownMonsterOperation?.Invoke(operation);
        }


        public void HandleOperation(string action, object data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            var operation = new WebServerOperation() {Operation = action, Data = jsonData, Type = "json" };
            OnMarkdownMonsterOperation?.Invoke(operation);
        }

        #endregion

        #region Response Output Wrappers
        internal void WriteResponseInternal(string data, string headers)
        {
            byte[] content = null;

            if (!string.IsNullOrEmpty(data))
                content = Encoding.UTF8.GetBytes(data);

            if (string.IsNullOrEmpty(headers))
                headers = "HTTP/1.1 200 OK\r\n" +
                          "Content-Type: application/json\r\n";

            if (content != null)
                headers += "Content-Length: " + content.Length;

            byte[] response = Encoding.UTF8.GetBytes(headers.TrimEnd('\r', '\n') + "\r\n\r\n");
            try
            {
                if (RequestContext?.NetworkStream == null)
                    return;

                RequestContext.NetworkStream.Write(response, 0, response.Length);

                if (content != null)
                    RequestContext.NetworkStream.Write(content, 0, content.Length);
            }
            catch(Exception ex)
            {
                // socket write failure
                mmApp.Model?.Window?.ShowStatusError("Web Server failed to send response: " + ex.Message);
            }
        }



        /// <summary>
        /// Creates a response with a data result
        /// </summary>
        /// <param name="data"></param>
        public void WriteDataResponse(object data)
        {
            var result = new WebServerResult(data);
            WriteResponse(result);
        }

        /// <summary>
        /// Writes a 204 response with no data
        /// </summary>
        public void WriteNoDataResponse()
        {
            var result = new WebServerResult();
            WriteResponse(result);
        }

        /// <summary>
        /// Writes a response with a full WebServerResult structure
        /// that allows maximum control over the response output.
        /// </summary>
        /// <param name="result"></param>

        public void WriteResponse(WebServerResult result)
        {
            string headers =
                $"HTTP/1.1 {result.HttpStatusCode} Server Error\r\n" +
                "Access-Control-Allow-Origin: *\r\n" +
                "Access-Control-Allow-Methods: GET,POST,PUT,OPTIONS\r\n" +
                "Access-Control-Allow-Headers: *\r\n";

            if (result.hasNoData)
            {
                WriteResponseInternal(null, headers);
            }
            else
            {
                var json = JsonSerializationUtils.Serialize(result, formatJsonOutput: true);
                headers =
                    $"HTTP/1.1 {result.HttpStatusCode} Server Error\r\n" +
                    "Access-Control-Allow-Origin: *\r\n" +
                    "Access-Control-Allow-Methods: GET,POST,PUT,OPTIONS\r\n" +
                    "Access-Control-Allow-Headers: *\r\n";

                WriteResponseInternal(json, headers + "Content-Type: application/json\r\n");
            }
        }


        public void WriteErrorResponse(string message = null)
        {
            var result = new WebServerResult(message);
            WriteResponse(result);
        }


        #endregion


        string GetFirstHeaderLine(string s)
        {
            var at = s.IndexOf("\r\n");
            if (at < 0)
                at = s.IndexOf("\n");

            if (at == 0)
                return null;

            return s.Substring(0, at - 1);
        }


        

    }

    internal class WebRequestContext
    {
        internal TcpClient Connection { get; set; }
        internal NetworkStream NetworkStream { get; set; }
        internal Stream Stream { get; set; }

        internal string Verb { get; set; }
        internal string Path { get; set; }

        internal string RequestHeaders { get; set; }

        internal string RequestContent {get; set; }

        internal void Close()
        {
            NetworkStream?.Close();
            Connection?.Close();
            Stream = null;
            NetworkStream = null;
            Connection = null;
        }
    }

    /// <summary>
    /// Operations that are to be performed on the server
    /// </summary>
    public class WebServerOperation
    {
        public string Operation { get; set; } = "open";

        public string Data { get; set; }

        public string Type { get; set; } = "text";   // text, json
    }
}
