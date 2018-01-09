
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

//http://www.rfc-editor.org/rfc/rfc6455.txt
namespace IdealWebSocket.ServerWebSocket
{
    public abstract class WebSocketServer
    {
        static readonly string WebSocketAcceptGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        Socket socket = null;

        public WebSocketSessionContainer Sessions = new WebSocketSessionContainer();
      
        static object lockobj = new object();

        protected abstract void OnSessionNewMessage(WebSocketSession session, WebSocketMessage message);

        protected abstract void OnSessionClose(WebSocketSession session, WebSocketReceiveError error);

        protected abstract void OnSessionConnected(WebSocketSession session);

        public WebSocketServer()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);                    
        }

        /// <summary>
        /// set server listen port;设置服务端的侦听端口
        /// </summary>
        /// <param name="port"></param>
        public void SetPort(int port)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);
            socket.Bind(endpoint);
            socket.Listen(10);
        }

        /// <summary>
        /// open the server;打开服务端
        /// </summary>
        public void Open()
        {
            socket.BeginAccept(AcceptCallback, socket);
        }
         
        private void AcceptCallback(IAsyncResult ar)
        {
            Socket serverSocket = (Socket)ar.AsyncState;          
            Socket singleSocket = null;
            try
            {
                singleSocket= serverSocket.EndAccept(ar);
                byte[] receiveBytes = new byte[1024];
                int length = singleSocket.Receive(receiveBytes);
                Dictionary<string, string> headers = GetHeadersFromRequest(receiveBytes,length);
                string error;
                byte[] response;
                if(Validate(headers,out error))
                {
                    string clientKey = headers["Sec-WebSocket-Key"].Trim();
                    string acceptKey = GenerateAcceptKey(clientKey);
                    response = HandshakeAcceptedResponse(acceptKey);
                    singleSocket.Send(response);                   
                    string remoteEndpoint = singleSocket.RemoteEndPoint.ToString();
                    WebSocketSession session = new WebSocketSession(singleSocket, headers,acceptKey);
                    OnSessionConnected(session);
                    Sessions.Add(remoteEndpoint, session);
                    session.ReceiveAsync(OnSessionNewMessage, OnSessionClose, Sessions);
                }
                else
                {
                    response = HandeshakeRejectedResponse(error);
                    singleSocket.Send(response);
                    singleSocket.Shutdown(SocketShutdown.Both);
                    singleSocket.Close();
                    singleSocket.Dispose();
                }
            }
            catch(Exception e)
            {
                singleSocket.Shutdown(SocketShutdown.Both);
                singleSocket.Close();
                singleSocket.Dispose();               
            }
            finally
            {
                serverSocket.BeginAccept(AcceptCallback, serverSocket);
            }
        }

        private Dictionary<String, String> GetHeadersFromRequest(byte[] receiveBytes,int length)
        {
            Dictionary<String, String> dic = new Dictionary<string, string>();
            string s = Encoding.UTF8.GetString(receiveBytes,0,length).Trim();
            Console.WriteLine(s);
            string[] headers = s.Split("\r\n");
            for (int i = 0; i < headers.Length; i++)
            {
                if (i == 0)
                {
                    dic.Add("StartLine", headers[i]);
                }
                else
                {
                    string[] items = headers[i].Split(':');
                    dic.Add(items[0], items[1]);
                }
            }
            return dic;
        }

        private Boolean Validate(Dictionary<String,String> headers,out string error)
        {           
            try
            {
                error = string.Empty;
                string startline = headers["StartLine"];
                string[] items = startline.Split(' ');
                if (items[0].ToUpper() != "GET")
                {
                    error = "http method must be get";
                    return false;
                }                   
                double httpversion = double.Parse(items[2].Split('/')[1]);
                if (httpversion < 1.1)
                {
                    error = "http version must be higher than 1.1";
                    return false;
                }
                if (!headers.ContainsKey("Host"))
                {
                    error = "Host header is required";
                    return false;
                }
                else
                {
                    if (string.IsNullOrEmpty(headers["Host"]))
                    {
                        error = "Host is empty";
                        return false;
                    }
                }
                if (!headers.ContainsKey("Upgrade"))
                {
                    error = "Upgrade Header is required";
                    return false;
                }
                else
                {
                    if (headers["Upgrade"].ToLower().IndexOf("websocket") ==-1)
                    {
                        error = "Upgrade header value must include string 'websocket'";
                        return false;
                    }
                }
                if (!headers.ContainsKey("Connection"))
                {
                    error = "Connection Header is required";
                    return false;
                }
                else
                {
                    if (headers["Connection"].ToLower().IndexOf("upgrade") == -1)
                    {
                        error = "Connection header value must include string 'upgrade'";
                        return false;
                    }
                }
                if (!headers.ContainsKey("Sec-WebSocket-Key"))
                {
                    error = "Sec-WebSocket-Key header is required";
                    return false;
                }
                else
                {
                    string value = headers["Sec-WebSocket-Key"];
                    byte[] a = Convert.FromBase64String(value);
                    if (a.Length != 16)
                    {
                        error = "Sec-WebSocket-Key value must be 16 byte if decoded";
                        return false;
                    }
                }
                if (!headers.ContainsKey("Sec-WebSocket-Version"))
                {
                    error = "Sec-WebSocket-Version header is required";
                    return false;
                }
                else
                {
                    if (headers["Sec-WebSocket-Version"].Trim() != "13")
                    {
                        error = "Sec-WebSocket-Version value must be 13";
                        return false;
                    }
                }
                if(headers.ContainsKey("User-Agent") && !headers.ContainsKey("Origin"))
                {
                    error = "Origin header is required if client is a browser";
                    return false;
                }               
                return true;
            }
            catch
            {
                error = "unkown error";
                return false;
            }
        }

        private byte[] HandshakeAcceptedResponse(string acceptKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("HTTP/1.1 101 Switching Protocols\r\n");
            sb.Append("Upgrade:websocket\r\n");
            sb.Append("Connection:Upgrade\r\n");
            sb.Append(string.Format("Sec-WebSocket-Accept:{0}\r\n", acceptKey));
            //sb.Append("Sec-WebSocket-Extensions:permessage-deflate\r\n");
            sb.Append("\r\n");
            Console.WriteLine(sb.ToString());
            byte[] response = Encoding.ASCII.GetBytes(sb.ToString());
            return response;
        }

        private byte[] HandeshakeRejectedResponse(string error)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("HTTP/1.1 403 {0}\r\n",error));
            byte[] response = Encoding.ASCII.GetBytes(sb.ToString());
            return response;
        }

        private string GenerateAcceptKey(string clientKey)
        {
            SHA1 sha1=new SHA1CryptoServiceProvider();
            clientKey += WebSocketAcceptGuid;
            byte[] bytes_in = Encoding.ASCII.GetBytes(clientKey);
            byte[] bytes_out = sha1.ComputeHash(bytes_in);
            string acceptKey = Convert.ToBase64String(bytes_out);
            return acceptKey.Trim();
        }

    }
}
