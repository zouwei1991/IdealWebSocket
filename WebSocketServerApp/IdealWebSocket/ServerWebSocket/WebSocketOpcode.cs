using System;
using System.Collections.Generic;
using System.Text;

namespace IdealWebSocket.ServerWebSocket
{
    public enum WebSocketOpcode
    {
        //indicate a continu frame
        Continuation = 0x0,
        //indicate a text frame
        Text = 0x1,
        //indicate a binary frame
        Binary = 0x2,
        //reserved for future non-control frame
        ReservedNoncontrolframe,
        //indicate a close frame
        Close = 0x8,
        //indicate a ping frame
        Ping = 0x9,
        //indicate a pong frame
        Pong = 0xA,
        //reserver for future control frame
        ReservedControlframe,
        //mean an eror
        Unkown
    }
}
