using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fleck.aiplay
{
    class Role
    {
        public IWebSocketConnection connection { get; set; }
        public Queue<Msg> dealList { get; set; }
        public Queue<Msg> finishList { get; set; }
        public DateTime createTime { get; set; }
        public Role()
        {
            connection = null;
            dealList = new Queue<Msg>();
            finishList = new Queue<Msg>();
            createTime = System.DateTime.Now;
        }
        public Role(IWebSocketConnection connection)
        {
            this.connection = connection;
            dealList = new Queue<Msg>();
            finishList = new Queue<Msg>();
            createTime = System.DateTime.Now;
        }
        public void EnqueueMessage(Msg msg)
        {
            dealList.Enqueue(msg);
        }
    }

    class Msg
    {
        public string message { get; set; }
        public bool isreturn { get; set; }
        public string retval { get; set; }
        public DateTime createTime { get; set; }
        public DateTime dealTime { get; set; }
        public Msg()
        {
            message = "";
            retval = "";
            isreturn = false;
            createTime = System.DateTime.Now;
            dealTime = System.DateTime.Now;
        }
        public Msg(string message)
        {
            this.message = message;
            retval = "";
            isreturn = false;
            createTime = System.DateTime.Now;
            dealTime = System.DateTime.Now;
        }
    }

    class User
    {
        List<IWebSocketConnection> allSockets;
        List<Role> allRoles;
        public Role currentRole { get; set; }
        public Role currentMsg { get; set; }
        public User()
        {
            allSockets = new List<IWebSocketConnection>();
            allRoles = new List<Role>();
        }

        public void Add(IWebSocketConnection socket)
        {
            var role = new Role(socket);
            allRoles.Add(role);
            allSockets.Add(socket);
        }

        public void Remove(IWebSocketConnection socket)
        {
            foreach (var r in allRoles.ToList())
            {
                if (r.connection == socket)
                {
                    allRoles.Remove(r);
                }
            }
            allSockets.Remove(socket);
        }

        public Role GetAt(IWebSocketConnection socket)
        {
            int index = allSockets.IndexOf(socket);
            return allRoles[index];
        }

        public Msg GetCurrentMsg()
        {
            return currentRole.dealList.Peek();
        }

        public void Deal(string line)
        {
            currentRole.connection.Send(line);
            Msg msg = GetCurrentMsg();
            msg.dealTime = System.DateTime.Now;
            msg.retval = line;
            msg.isreturn = true;
            currentRole.finishList.Enqueue(msg);
            currentRole.dealList.Dequeue();      
        }

        public void Send(string line)
        {
            currentRole.connection.Send(line);
        }
    }
}
