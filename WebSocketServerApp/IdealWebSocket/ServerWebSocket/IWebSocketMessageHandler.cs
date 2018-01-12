using System;
using System.Collections.Generic;
using System.Text;

namespace IdealWebSocket.ServerWebSocket
{
    //define the parse and create method of one entire websocket message
    //定义用于解析 和 创建 一条websocket消息或者帧的方法,解析和创建是否应该分离?创建借口是考虑之后用于扩展不同的解码方式(配合客户端可以支持的压缩方式)
    interface IWebSocketMessageHandler
    {
        
        //将收到的字节数组解析为一个websocket帧
        //parse the bytes received to one websocket fragment.
        WebSocketFragment Parse(byte[] receiveBytes, out WebSocketReceiveError error);

        /// <summary>
        /// create one websockeet message;创建一条websocket消息
        /// </summary>
        /// <param name="sendBytes">message bytes to be send;需要发送的消息的二进制数组</param>
        /// <param name="buffersize">server send buffer size;服务端的发送缓冲区大小</param>
        /// <param name="messagetype">websocket message type,"text" or "bianry",only text suported now;仅支持text类型,binary需要另外实现</param>
        /// <returns></returns>
        WebSocketMessage CreateMessage(byte[] sendBytes,int buffersize,string messagetype);

        /// <summary>
        /// create the ping frame;创建ping
        /// </summary>
        /// <returns></returns>
        WebSocketMessage CreatePing();

        /// <summary>
        /// create the pong frame;创建pong
        /// </summary>
        /// <param name="pingdata"></param>
        /// <returns></returns>
        WebSocketMessage CreatePong(byte[] pingdata);
        
        /// <summary>
        /// create the close frame;创建关闭帧
        /// </summary>
        /// <param name="error">the error message send to client;要发送给客户端的错误消息</param>
        /// <param name="closeStatusCode">websockt close status code</param>
        /// <returns></returns>
        WebSocketMessage CreateCloseMessage(string error,WebSocketCloseStatusCode closeStatusCode);
    }
}
