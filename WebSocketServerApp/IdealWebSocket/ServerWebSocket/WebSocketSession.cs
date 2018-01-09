using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace IdealWebSocket.ServerWebSocket
{
    //abstract of one tcp connection of websocket
    //一个websocket连接的抽象
    public class WebSocketSession
    {
        /// <summary>
        /// identification;唯一标识,使用remoteendpoint
        /// </summary>
        public String SessionID { get; private set; }

        public Dictionary<String, String> Headers { get; private set; }
        
        public String Host { get { return Headers["Host"]; } }

        public String Upgrade { get { return Headers["Upgrade"]; } }

        public String Connection { get { return Headers["Connection"]; } }

        public String SecWebSocketKey { get { return Headers["Sec-WebSocket-key"]; } }

        public String SecWebSocketVersion { get { return Headers["Sec-WebSocket-Version"]; } }

        public String Origin { get { return Headers.ContainsKey("Origin") ? Headers["Origin"] : null; } }

        public String RemoteEndpoint { get { return  serverSocket.RemoteEndPoint.ToString();  } }

        public String RequestUrl
        {
            get
            {
                var array = Headers["StartLine"].Split(' ');
                return array[1];
            }
        }

        Socket serverSocket = null;
      

        CommonWebSocketMessageHandler messageHandler = new CommonWebSocketMessageHandler();

        internal class ReceiveObject
        {
            public Socket Socket { get; set; }

            public byte[] Buffer { get; set; }

            public WebSocketMessage Message { get; set; }
         
            public Action<WebSocketSession,WebSocketMessage> Success { get; set; }

            public Action<WebSocketSession,WebSocketReceiveError> Error { get; set; }

            public WebSocketSession Session { get; set; }

            public Object State { get; set; }
        }

        public WebSocketSession(Socket socket,Dictionary<String,String> items,string sessionID)
        {
            serverSocket = socket;
            Headers = items;
            SessionID = sessionID;           
        }
        
        public void ReceiveAsync(Action<WebSocketSession,WebSocketMessage> successCallback,Action<WebSocketSession,WebSocketReceiveError> errorCallback,Object state)
        {
            byte[] buffer = new byte[serverSocket.ReceiveBufferSize];
            WebSocketMessage message = new WebSocketMessage();
            ReceiveObject obj = new ReceiveObject();
            obj.Socket = serverSocket;
            obj.Buffer = buffer;
            obj.Message = message;
            obj.Success = successCallback;
            obj.Error = errorCallback;
            obj.Session = this;
            obj.State = state;
            serverSocket.BeginReceive(buffer, 0, serverSocket.ReceiveBufferSize, SocketFlags.None, ReceiveCallback, obj);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            WebSocketReceiveError error;
            ReceiveObject obj = (ReceiveObject)ar.AsyncState;
            int receiveLength = obj.Socket.EndReceive(ar);
            if (receiveLength > 0)
            {
                ArraySegment<byte> bytes = new ArraySegment<byte>(obj.Buffer, 0, receiveLength);
                WebSocketFragment fragment = messageHandler.Parse(bytes.ToArray(), out error);
                if (error != null)
                {
                    Close(error.CloseReason, error.CloseStatusCode);
                    obj.Error(obj.Session,error);
                    WebSocketSessionContainer sessions = (WebSocketSessionContainer)obj.State;
                    sessions.Remove(SessionID);
                }
                else
                {
                    if (fragment.Opcode == WebSocketOpcode.Ping)
                    {
                        WebSocketMessage m = messageHandler.CreatePong(fragment.PayloadData);
                        Send(m);
                    }
                    else if (fragment.Opcode == WebSocketOpcode.Pong)
                    {

                    }
                    else if (fragment.Opcode == WebSocketOpcode.Close)
                    {
                        Close("Normal Close", WebSocketCloseStatusCode.Normal);
                        obj.Error(obj.Session, new WebSocketReceiveError() { CloseReason = "Normal Close", CloseStatusCode = WebSocketCloseStatusCode.Normal });
                        WebSocketSessionContainer sessions = (WebSocketSessionContainer)obj.State;
                        sessions.Remove(SessionID);
                    }
                    else
                    {
                        obj.Message.Fragments.Add(fragment);
                        if (fragment.IsFinal)
                        {
                            obj.Success(obj.Session, obj.Message);
                            obj.Message = new WebSocketMessage();
                        }
                    }                                                       
                }
            }
            if (serverSocket.Connected)
            {
                serverSocket.BeginReceive(obj.Buffer, 0, serverSocket.ReceiveBufferSize, SocketFlags.None, ReceiveCallback, obj);
            }
        }

        public Boolean Send(WebSocketMessage message)
        {
            try
            {
                for (int i = 0; i < message.Fragments.Count; i++)
                {
                    serverSocket.Send(message.Fragments[i].FragmentMessage);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Boolean Send(byte[] payloadData)
        {
            try
            {
                int size = serverSocket.SendBufferSize;
                WebSocketMessage message = messageHandler.CreateMessage(payloadData, size, "text");
                Send(message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Boolean Send(String payloadMessage)
        {
            try
            {
                byte[] m = Encoding.UTF8.GetBytes(payloadMessage);
                int size = serverSocket.ReceiveBufferSize;
                WebSocketMessage message = messageHandler.CreateMessage(m, size, "text");
                Send(message);
                return true;
            }
            catch
            {
                return false;
            }             
        }
     
        public void Close(string closeMessage,WebSocketCloseStatusCode closeStatusCode)
        {
            //关系close之前需要先发送关闭帧
            WebSocketMessage message = messageHandler.CreateCloseMessage(closeMessage, closeStatusCode);
            Send(message);        
            serverSocket.Shutdown(SocketShutdown.Both);
            serverSocket.Close();
            serverSocket.Dispose();
        }
    }
}
