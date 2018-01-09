using System;
using System.Collections.Generic;
using System.Text;
using IdealWebSocket.ServerWebSocket;
using Newtonsoft.Json;
using WebSocketServerApp.ChatProtocolCore;

namespace WebSocketServerApp
{
    //custom websockerServer;一个自定义的websocket服务端
    public class MyWebSocketServer : IdealWebSocket.ServerWebSocket.WebSocketServer
    {
        public ChatRoom Room = new ChatRoom();

        public MyWebSocketServer()
        {

        }

        protected override void OnSessionClose(WebSocketSession session, WebSocketReceiveError error)
        {
            
        }

        protected override void OnSessionConnected(WebSocketSession session)
        {
            
        }

        protected override void OnSessionNewMessage(WebSocketSession session, WebSocketMessage message)
        {
            if (message.Opcode == WebSocketOpcode.Text)
            {
                string m = Encoding.UTF8.GetString(message.Message);
                ChatProtocol chatMessage = JsonConvert.DeserializeObject<ChatProtocol>(m);
                if (string.IsNullOrEmpty(chatMessage.UserName))
                {
                    session.Close("UserName missed", WebSocketCloseStatusCode.InternalServerError);
                    Sessions.Remove(session.SessionID);
                    return;
                }
                if (chatMessage.CommandType == "Login")
                {
                    if (Room.ContainsUser(chatMessage.UserName))
                    {
                        chatMessage.CommandType = "Error";
                        chatMessage.Message = "User have been already in chatroom";
                        session.Close("User have already logined", WebSocketCloseStatusCode.InternalServerError);
                        Sessions.Remove(session.SessionID);
                        return;
                    }
                    else
                    {
                        Room.AddUser(chatMessage.UserName, session.RemoteEndpoint);
                        var remotelist = Room.RemoteEndpointList;
                        foreach (var remote in remotelist)
                        {
                            WebSocketSession s = Sessions[remote];
                            if (s != null)
                            {
                                if (!s.Send(m))
                                {
                                    Sessions.Remove(session.SessionID);
                                    string user = Room.GetUser(remote);
                                    if (!string.IsNullOrEmpty(user))
                                    {
                                        Room.RemoveUser(user);
                                    }
                                }
                            }
                        }
                        return;
                    }
                }
                else if (chatMessage.CommandType == "Message")
                {
                    var remotelist = Room.RemoteEndpointList;
                    foreach(var remote in remotelist)
                    {
                        WebSocketSession s = Sessions[remote];
                        if (s != null)
                        {
                            if (!s.Send(m))
                            {
                                Sessions.Remove(session.SessionID);
                                string user = Room.GetUser(remote);
                                if (!string.IsNullOrEmpty(user))
                                {
                                    Room.RemoveUser(user);
                                }
                            }
                        }
                    }
                }
                else
                {
                    return;
                }
            }

        }
    }
}
