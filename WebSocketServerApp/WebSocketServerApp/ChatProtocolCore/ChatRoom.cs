using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketServerApp.ChatProtocolCore
{
    //a very simple chat room;聊天室
    public class ChatRoom
    {
        public List<String> UserList
        {
            get
            {
                lock (Users)
                {
                    List<String> list = new List<string>();
                    var items = GetEnumerator();
                    while (items.MoveNext())
                    {
                        var item = items.Current;
                        list.Add(item.Key);
                    }
                    return list;
                }
                
            }
        }

        public List<String> RemoteEndpointList
        {
            get
            {
                lock (Users)
                {
                    List<String> list = new List<string>();
                    var items = GetEnumerator();
                    while (items.MoveNext())
                    {
                        var item = items.Current;
                        list.Add(item.Value);
                    }
                    return list;
                }
            }
        }

        public Dictionary<String,String> Users { get; private set; }

        public ChatRoom()
        {
            Users = new Dictionary<string, string>();    
        }

        public Boolean ContainsUser(string user)
        {
            return Users.ContainsKey(user);
        }

        public Boolean AddUser(String user,string remotePoint)
        {
            lock (Users)
            {
                if (ContainsUser(user))
                {
                    return false;
                }
                else
                {
                    Users.Add(user, remotePoint);
                    return true;
                }
            }          
        }

        public Boolean RemoveUser(string user)
        {
            return Users.Remove(user);
        }

        public String GetUser(string remote)
        {
            var items = Users.GetEnumerator();
            while (items.MoveNext())
            {
                var item = items.Current;
                if (item.Value == remote)
                {
                    return item.Key;
                }
            }
            return null;
        }



        public IEnumerator<KeyValuePair<String,String>> GetEnumerator()
        {
            return Users.GetEnumerator();
        }
    }
}
