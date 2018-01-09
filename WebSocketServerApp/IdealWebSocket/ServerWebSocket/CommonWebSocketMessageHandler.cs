using System;
using System.Collections.Generic;
using System.Text;

namespace IdealWebSocket.ServerWebSocket
{
    public class CommonWebSocketMessageHandler : IWebSocketMessageHandler
    {
        public WebSocketMessage CreateCloseMessage(string errorMessage,WebSocketCloseStatusCode closeStatusCode)
        {
            byte[] error = Encoding.UTF8.GetBytes(errorMessage.Trim());
            if (error.Length > 123)
                throw new Exception("close frame payload data length must be 0-125");
            List<WebSocketFragment> fragments = new List<WebSocketFragment>();
            List<byte> list = new List<byte>();
            Byte firstByte = 0x88;
            Byte secondByte = (Byte)error.Length;
            list.Add(firstByte);
            list.Add(secondByte);
            for(int j = 0; j < 2; j++)
            {
                Byte b = (Byte)((int)closeStatusCode >> 8 * j);
                list.Add(b);
            }
            for(int i = 0; i < error.Length; i++)
            {
                list.Add(error[i]);
            }
            WebSocketFragment fragment = new WebSocketFragment(list.ToArray());
            fragments.Add(fragment);
            return new WebSocketMessage(fragments);
        }

        public WebSocketMessage CreateMessage(byte[] sendBytes, int buffersize,string messagetype)
        {
            if (messagetype != "text" && messagetype != "binary")
                throw new Exception("messagetype is not surport,must be text or bianry");

            List<WebSocketFragment> fragments = new List<WebSocketFragment>();
            int payloadLength = (buffersize - 10);
            int count = sendBytes.Length / payloadLength;
            int rest = sendBytes.Length % payloadLength;
            if (rest != 0)
                count++;
            for(int i = 0; i < count; i++)
            {
                List<byte> list = new List<byte>();
                Byte firstbyte = 0;
                if (i == count - 1)
                {
                    firstbyte = (Byte)(firstbyte | 0x80);
                }
                if(i==0 && messagetype == "text")
                {
                    firstbyte = (Byte)(firstbyte | 0x1);
                }
                if(i==0 && messagetype == "binary")
                {
                    firstbyte = (Byte)(firstbyte | 0x2);
                }
                list.Add(firstbyte);
                Byte sencondByte = 0;
                int size = rest == 0 ? payloadLength : (i == count - 1) ? rest : payloadLength;
                if(size>=0 && size <= 125)
                {
                    sencondByte = (Byte)(sencondByte | size);
                    list.Add(sencondByte);
                }
                else if(size>125 && size <= 65535)
                {
                    sencondByte = (Byte)(sencondByte | 0x7e);
                    list.Add(sencondByte);
                    for(int j = 1; j >= 0; j--)
                    {
                        Byte b = (Byte)(size >> 8 * j);
                        list.Add(b);
                    }
                }
                else
                {
                    sencondByte = (Byte)(sencondByte | 0x7f);
                    list.Add(sencondByte);
                    for(int j = 7; j >= 0; j--)
                    {
                        Byte b = (Byte)(size >> 8 * j);
                        list.Add(b);
                    }
                }
                for(int n = 0; n < size; n++)
                {
                    list.Add(sendBytes[payloadLength * i + n]);
                }
                WebSocketFragment fragment = new WebSocketFragment(list.ToArray());
                fragments.Add(fragment);
            }
            return new WebSocketMessage(fragments);
        }

        public WebSocketMessage CreatePing()
        {
            List<WebSocketFragment> fragments = new List<WebSocketFragment>();
            List<byte> list = new List<byte>();
            Byte firstByte = 0x89;
            Byte sencondByte = 0x0;
            list.Add(firstByte);
            list.Add(sencondByte);
            WebSocketFragment fragment = new WebSocketFragment(list.ToArray());
            fragments.Add(fragment);
            return new WebSocketMessage(fragments);
        }

        public WebSocketMessage CreatePong(byte[] pingdata)
        {
            if (pingdata.Length > 125)
                throw new Exception("pong frame payload data length must be 0-125");
            List<WebSocketFragment> fragments = new List<WebSocketFragment>();
            List<byte> list = new List<byte>();
            Byte firstByte = 0x8a;
            Byte sencondByte = (Byte)pingdata.Length;
            list.Add(firstByte);
            list.Add(sencondByte);
            for(int i = 0; i < pingdata.Length; i++)
            {
                list.Add(pingdata[i]);
            }
            WebSocketFragment fragment = new WebSocketFragment(list.ToArray());
            fragments.Add(fragment);
            return new WebSocketMessage(fragments);
        }

        public WebSocketFragment Parse(byte[] receiveBytes, out WebSocketReceiveError error)
        {
            error = null;
            WebSocketFragment fragment = new WebSocketFragment(receiveBytes);
            #region validate
            if (fragment.IsExtensionNegotiated)
            {
                error = new WebSocketReceiveError() { CloseStatusCode = WebSocketCloseStatusCode.ProtocolError, CloseReason = "RSV1,RSV2,RSV3 must be 0,because extension negotiation is not supported" };
            }
            if (fragment.IsFinal && fragment.Opcode == WebSocketOpcode.Continuation)
            {
                error = new WebSocketReceiveError() { CloseStatusCode = WebSocketCloseStatusCode.ProtocolError, CloseReason = "FIN is set to 1 but opcode indicate a contituation" };
            }
            if (fragment.Opcode == WebSocketOpcode.ReservedNoncontrolframe)
            {
                error = new WebSocketReceiveError() { CloseStatusCode = WebSocketCloseStatusCode.ProtocolError, CloseReason = "Opcode 3-7 is reserved for non-control frames and is not suported now" };
            }
            if (fragment.Opcode == WebSocketOpcode.ReservedControlframe)
            {
                error = new WebSocketReceiveError() { CloseStatusCode = WebSocketCloseStatusCode.ProtocolError, CloseReason = "Opcode B-F is reserved for controlframes and is not suported now" };
            }
            if (fragment.IsFinal && fragment.Opcode == WebSocketOpcode.Ping)
            {
                error = new WebSocketReceiveError() { CloseStatusCode = WebSocketCloseStatusCode.ProtocolError, CloseReason = "Ping frame can not be fragmented" };
            }
            if (fragment.IsFinal && fragment.Opcode == WebSocketOpcode.Pong)
            {
                error = new WebSocketReceiveError() { CloseStatusCode = WebSocketCloseStatusCode.ProtocolError, CloseReason = "Pong frame can not be fragmented" };
            }
            if (fragment.IsFinal && fragment.Opcode == WebSocketOpcode.Close)
            {
                error = new WebSocketReceiveError() { CloseStatusCode = WebSocketCloseStatusCode.ProtocolError, CloseReason = "Close frame can not be fragmented" };
            }
            if (fragment.Opcode == WebSocketOpcode.Unkown)
            {
                error = new WebSocketReceiveError() { CloseStatusCode = WebSocketCloseStatusCode.ProtocolError, CloseReason = "Unkown Opcode" };
            }
            if (!fragment.IsMasked)
            {
                error = new WebSocketReceiveError() { CloseStatusCode = WebSocketCloseStatusCode.ProtocolError, CloseReason = "Message send from client must be masked" };
            }
            if (fragment.PayloadLength != (ulong)fragment.PayloadData.Length)
            {
                error = new WebSocketReceiveError() { CloseStatusCode = WebSocketCloseStatusCode.ProtocolError, CloseReason = "payloadlength field value does not match the length of received payload data" };
            }
            #endregion
            return fragment;
        }
    }
}
