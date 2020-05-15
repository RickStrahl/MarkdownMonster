using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Forms;
using ReverseMarkdown;

namespace MarkdownMonster.WebSockets
{
public class WebSocketServer
{
    
    string IpAddress { get; set; } = "127.0.0.1";
    int ServerPort { get; set; } = 5009;

    Thread SocketThread { get; set; }
    TcpListener TcpServer { get; set; }

    /// <summary>
    /// Last exception that occurred when starting the server or intercepting
    /// request data.
    /// </summary>
    public Exception LastException { get; set; }

    public bool StartServer(string ipAddress = "127.0.0.1", int serverPort = 5009)
    {
        try
        {
            SocketThread = new Thread(RunServer);
            SocketThread.Start();
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

        TcpServer?.Stop();
        TcpServer = null;

        try
        {
            SocketThread?.Abort();
        }
        finally
        {
            SocketThread = null;
        }

    }

    /// <summary>
    /// Called when a Web Socket message has been captured.
    /// This method returns the raw binary data.
    /// 
    /// Important: This occurs on a non-synchronized thread
    /// so if you're using this in UI make sure to marshal
    /// to the UI thread.
    /// </summary>
    public Action<byte[]> OnBinaryMessage;

    /// <summary>
    /// Called when a Web Socket message has been captured.
    /// This method returns the data as a string.
    /// 
    /// Important: This occurs on a non-synchronized thread
    /// so if you're using this in UI make sure to marshal
    /// to the UI thread.
    /// </summary>
    public Action<string> OnStringMessage;

    //public Action OnDisconnected;
    //public Action<string> OnConnected;


    public void RunServer()
    {
        TcpServer?.Stop();

        TcpServer = new TcpListener(IPAddress.Parse(IpAddress),ServerPort);
        TcpServer.Start();

        TcpClient client = null;
        NetworkStream stream = null;

        // enter to an infinite cycle to be able to handle every change in stream
        while (true)
        {
            try
            {
                if (client == null || !client.Connected)
                    client = TcpServer.AcceptTcpClient();
                if (stream == null)
                    stream = client.GetStream();

                var capturedData = new List<byte>();

                while (!stream.DataAvailable)
                {
                    Thread.Sleep(1); // don't hog CPU
                    while (client.Available < 3)
                    {
                        Thread.Sleep(10);
                    }
                }

                // Read initial buffer
                byte[] bytes = new byte[client.Available];
                stream.Read(bytes, 0, client.Available);

                // check for handshake only if the block isn't very big
                if (bytes.Length < 1028)
                {
                    string s = Encoding.UTF8.GetString(bytes);

                    if (Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase))
                    {
                        // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
                        // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
                        // 3. Compute SHA-1 and Base64 hash of the new value
                        // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
                        string swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                        string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                        byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create()
                            .ComputeHash(Encoding.UTF8.GetBytes(swka));
                        string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                        // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
                        byte[] response = Encoding.UTF8.GetBytes(
                            "HTTP/1.1 101 Switching Protocols\r\n" +
                            "Connection: Upgrade\r\n" +
                            "Upgrade: websocket\r\n" +
                            "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

                        stream.Write(response, 0, response.Length);
                        continue; // done here
                    }
                }

                // Capture Payload data
                capturedData.AddRange(bytes);


                // Read multiple buffers for large content
                while (stream.DataAvailable)
                {
                    try
                    {
                        stream.Read(bytes, 0, client.Available);
                        capturedData.AddRange(bytes);
                    }
                    catch
                    {
                        // TODO: needs specific exception handling for disconnects
                    }
                }

                bytes = capturedData.ToArray();

                if (bytes.Length < 0)
                    continue; // nothing to do



                List<byte> output = new List<byte>();
                while (true)
                {
                    bool fin = (bytes[0] & 0b10000000) != 0;
                    bool mask = (bytes[1] & 0b10000000) !=
                                0; // must be true, "All messages from the client to the server have this bit set"
                    int opcode = bytes[0] & 0b00001111; // expecting 1 - text message
                    int msglen = bytes[1] - 128, // & 0111 1111
                        offset = 2;


                    if (msglen == 126)
                    {
                        // was ToUInt16(bytes, offset) but the result is incorrect
                        msglen = BitConverter.ToUInt16(new byte[] {bytes[3], bytes[2]}, 0);
                        offset = 4;
                    }
                    else if (msglen == 127)
                    {
                        // TODO: This is broken in Chromium!!! returns 0 0 0 0 0 2 0 0 for any content with ml 127
                        msglen = (int) BitConverter.ToUInt64(
                            new byte[] {bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2]},
                            0);
                        offset = 10;
                    }

                    if (msglen == 0)
                    {
                        // nothing to do - empty message
                    }
                    else if (mask)
                    {
                        byte[] decoded = new byte[msglen];
                        byte[] masks = new byte[4]
                        {
                            bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3]
                        };
                        offset += 4;

                        for (int i = 0; i < msglen; ++i)
                            decoded[i] = (byte) (bytes[offset + i] ^ masks[i % 4]);

                        output.AddRange(decoded);

                        if (!fin)
                        {
                            // keep going with the next frame
                            var off = msglen + offset;
                            var spanBytes = bytes.AsSpan<byte>(off);
                            bytes = spanBytes.ToArray(); //Array.Copy(bytes, off, b2, 0, bytes.Length - off);
                        }
                        else
                            break;
                    }
                }

                // Notifications if connected
                OnBinaryMessage?.Invoke(output.ToArray());
                OnStringMessage?.Invoke(Convert.ToBase64String(output.ToArray()));
            }
            catch
            {
                // keep it going - otherwise the thread crashes and the server goes away
            }
        }
    }
}
}
