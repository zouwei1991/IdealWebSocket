using System;
using System.Collections.Generic;
using System.Text;

namespace IdealWebSocket.ServerWebSocket
{
    //abstract of one entire WebSocketMessage,grouped by list fragment
    //一条完整的WebSocket消息,由一条或数条fragment对象组成
    public class WebSocketMessage
    {
        /// <summary>
        /// list of fragment of one websocket message
        /// 一条websocket消息的帧的集合
        /// </summary>
        List<WebSocketFragment> m_Fragments;
        public List<WebSocketFragment> Fragments { get { return m_Fragments; } }

        /// <summary>
        /// payload data of one message
        /// 一条websocket消息的二进制数组
        /// </summary>
        public byte[] Message
        {
            get
            {
                List<byte> list = new List<byte>();
                for (int i = 0; i < m_Fragments.Count; i++) {
                    for(int j = 0; j < m_Fragments[i].PayloadData.Length; j++)
                    {
                        list.Add(m_Fragments[i].PayloadData[j]);
                    }
                }
                return list.ToArray();
            }
        }

        /// <summary>
        /// OpCode of websocket message ,"text" or "binary"
        /// 消息内容的类型,文本或者二进制,如果是控制帧,服务端会自动处理
        /// </summary>
        public WebSocketOpcode Opcode { get { return m_Fragments[0].Opcode; } }

        /// <summary>
        /// message type
        /// </summary>
        public string MessageType { get
            {
                if (m_Fragments[0].Opcode == WebSocketOpcode.Text)
                {
                    return "text";
                }
                if (m_Fragments[0].Opcode == WebSocketOpcode.Binary)
                {
                    return "bianry";
                }
                throw new Exception("Unkown OpCode");
            }
        }

        public WebSocketMessage(List<WebSocketFragment> fragments)
        {
            m_Fragments = new List<WebSocketFragment>();
            m_Fragments.AddRange(fragments);
        }

        public WebSocketMessage()
        {
            m_Fragments = new List<WebSocketFragment>();
        }
    }
}
