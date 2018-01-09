using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketServerApp.ChatProtocolCore
{
    //a very simple application protol
    public class ChatProtocol
    {
        public string CommandType { get; set; }

        public string UserName { get; set; }

        public string Message { get; set; }

        public int TimeStamp { get; set; }
    }
}
