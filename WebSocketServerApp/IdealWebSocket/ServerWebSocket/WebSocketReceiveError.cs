using System;
using System.Collections.Generic;
using System.Text;

namespace IdealWebSocket.ServerWebSocket
{
    public class WebSocketReceiveError
    {
        public WebSocketCloseStatusCode CloseStatusCode { get; set; }

        public String CloseReason { get; set; }
    }
}
