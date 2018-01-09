using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IdealWebSocket.ServerWebSocket
{
    public class WebSocketSessionContainer : IDictionary<String, WebSocketSession>
    {
        Dictionary<String, WebSocketSession> sessionContainer = new Dictionary<String, WebSocketSession>();

        public WebSocketSessionContainer()
        {

        }

        public WebSocketSession this[string key]
        {
            get
            {
                if (ContainsKey(key))
                    return sessionContainer[key];
                return null;
            }
            set
            {
                if (ContainsKey(key))
                {
                    sessionContainer.Remove(key);
                    sessionContainer.Add(key, value);
                }
                else
                {
                    sessionContainer.Add(key, value);
                }
            }
        }
     
        public ICollection<string> Keys => sessionContainer.Keys;

        public ICollection<WebSocketSession> Values => sessionContainer.Values;

        public int Count => sessionContainer.Count;

        public bool IsReadOnly => true;

        public void Add(string key, WebSocketSession value)
        {
            lock (sessionContainer)
            {
                if (!ContainsKey(key))
                {
                    sessionContainer.Add(key, value);
                }
            }          
        }

        public void Add(KeyValuePair<string, WebSocketSession> item)
        {
            lock (sessionContainer)
            {
                if (!ContainsKey(item.Key))
                {
                    sessionContainer.Add(item.Key, item.Value);
                }
            }           
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, WebSocketSession> item)
        {
            return ContainsKey(item.Key);    
        }

        public bool ContainsKey(string key)
        {
            return sessionContainer.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, WebSocketSession>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, WebSocketSession>> GetEnumerator()
        {
            return sessionContainer.GetEnumerator();
        }

        public bool Remove(string key)
        {
            lock (sessionContainer)
            {
                return sessionContainer.Remove(key);
            }            
        }

        public bool Remove(KeyValuePair<string, WebSocketSession> item)
        {
            lock (sessionContainer)
            {
                return sessionContainer.Remove(item.Key);
            }            
        }

        public bool TryGetValue(string key, out WebSocketSession value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return sessionContainer.GetEnumerator();
        }
    }
}
