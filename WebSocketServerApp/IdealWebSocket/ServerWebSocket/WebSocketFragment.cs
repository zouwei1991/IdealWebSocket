using System;
using System.Collections.Generic;
using System.Text;

namespace IdealWebSocket.ServerWebSocket
{
    //abstract of one websocket fragment
    //一个websocket帧的抽象
    public class WebSocketFragment
    {
        /// <summary>
        /// Total bytes of one fragment;一个帧的全部字节
        /// </summary>
        private byte[] m_fragmentMessage = null;
        public byte[] FragmentMessage { get { return m_fragmentMessage; } }

        /// <summary>
        /// indicate the fragment is the last fragment of one meesage;指示该帧是否为最后一帧
        /// </summary>
        public bool IsFinal { get { return (m_fragmentMessage[0] & 0x80)>>7 == 1; } }

        /// <summary>
        /// true if any extension have been negitiated;是否有扩展
        /// </summary>
        public bool IsExtensionNegotiated { get { return (m_fragmentMessage[0] & 0x40) != 0 || (m_fragmentMessage[0] & 0x20) != 0 || (m_fragmentMessage[0]& 0x10)!=0; } }

        /// <summary>
        /// fragment opcode
        /// </summary>
        public WebSocketOpcode Opcode
        {
            get
            {
                WebSocketOpcode opcode;
                Byte opCode = (Byte)(m_fragmentMessage[0] & 0xf);
                switch (opCode)
                {
                    case 0x0:
                        opcode = WebSocketOpcode.Continuation;
                        break;
                    case 0x1:
                        opcode = WebSocketOpcode.Text;
                        break;
                    case 0x2:
                        opcode = WebSocketOpcode.Binary;
                        break;
                    case 0x3:
                    case 0x4:
                    case 0x5:
                    case 0x6:
                    case 0x7:
                        opcode = WebSocketOpcode.ReservedNoncontrolframe;
                        break;
                    case 0x8:
                        opcode = WebSocketOpcode.Close;
                        break;
                    case 0x9:
                        opcode = WebSocketOpcode.Ping;
                        break;
                    case 0xA:
                        opcode = WebSocketOpcode.Pong;
                        break;
                    case 0xB:
                    case 0xC:
                    case 0xD:
                    case 0xE:
                    case 0xF:
                        opcode = WebSocketOpcode.ReservedControlframe;
                        break;
                    default:
                        opcode = WebSocketOpcode.Unkown;
                        break;
                }
                return opcode;
            }
        }

        /// <summary>
        /// true if masked.message send from client must be true;message send to client must be false.该帧是否有掩码
        /// </summary>
        public bool IsMasked { get { return (m_fragmentMessage[1] & 0x80)>>7 == 1; } }

        /// <summary>
        /// the bytes oucupied by payloadlength.if 0-125 then 0,if 126,then 2,if 127 then 8.
        /// 用来解释有效数据长度所占用的字节数,如果0-125,占用0字节;如果126,占用2字节,如果127,占用8字节
        /// </summary>
        public uint PayloadBytes
        {
            get
            {
                Byte payload = (Byte)(m_fragmentMessage[1] & 0x7f);
                if (payload == 126)
                    return 2;
                if (payload == 127)
                    return 8;
                else
                    return 0;
            }
        }

        /// <summary>
        /// length of payload bytes;有效载荷的长度
        /// </summary>
        public ulong PayloadLength
        {
            get
            {
                if (PayloadBytes == 0)
                {
                    Byte payload = (Byte)(m_fragmentMessage[1] & 0x7f);
                    return payload;
                }
                else
                {
                    ulong length = 0;
                    for(int i = 0; i < PayloadBytes; i++)
                    {
                        length = (length << 8) | m_fragmentMessage[2 + i];
                    }
                    return length;
                }
            }
        }

        /// <summary>
        /// masking bytes;return null if message is not masked.四位字节的掩码,如果没有掩码,返回null
        /// </summary>
        public byte[] MaskingKey
        {
            get
            {
                if (IsMasked)
                {
                    byte[] key = new byte[4];
                    for(int i = 0; i < 4; i++)
                    {
                        Byte b = m_fragmentMessage[2 + PayloadBytes + i];
                        key[i] = b;
                    }
                    return key;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// payload bytes;有效载荷的字节
        /// </summary>
        public byte[] PayloadData
        {
            get
            {
                List<byte> list = new List<byte>();
                uint offset = IsMasked ? (2 + PayloadBytes + 4) : (2 + PayloadBytes);
                for(uint i = offset; i < m_fragmentMessage.Length; i++)
                {
                    list.Add(m_fragmentMessage[i]);
                }
                if (IsMasked)
                {
                    byte[] unmaskedBytes = UnmaskPayloadData(MaskingKey, list.ToArray());
                    return unmaskedBytes;
                }
                else
                {
                    return list.ToArray();
                }              
            }
        }

        public WebSocketFragment(byte[] message)
        {
            m_fragmentMessage = message;
        }

        //method to unmasked the client message;用于解码的方法
        private byte[] UnmaskPayloadData(byte[] maskBytes, byte[] maskedData)
        {
            byte[] unmaskedData = new byte[maskedData.Length];
            for (int i = 0; i < maskedData.Length; i++)
            {
                byte b = (Byte)(maskedData[i] ^ maskBytes[i % 4]);
                unmaskedData[i] = b;
            }
            return unmaskedData;
        }
    }
}
