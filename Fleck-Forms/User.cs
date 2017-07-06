using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Fleck.aiplay
{
    class Role
    {
        public IWebSocketConnection connection { get; set; }
        public Queue<Msg> MsgQueue { get; set; }
        public Queue QueryallQueue { get; set; }
        public Queue<Msg> FinishQueue { get; set; }
        public Msg currentMsg { get; set; }
        public DateTime createTime { get; set; }
        public DateTime lastdealTime { get; set; }
        public Role()
        {
            connection = null;
            MsgQueue = new Queue<Msg>();
            QueryallQueue = new Queue();
            FinishQueue = new Queue<Msg>();
            createTime = System.DateTime.Now;
            lastdealTime = System.DateTime.Now;
        }
        public Role(IWebSocketConnection connection)
        {
            this.connection = connection;
            MsgQueue = new Queue<Msg>();
            FinishQueue = new Queue<Msg>();
            QueryallQueue = new Queue();
            createTime = System.DateTime.Now;
            lastdealTime = System.DateTime.Now;
        }
        public void EnqueuePositionMessage(Msg msg)
        {
            lock (MsgQueue)
            {
                MsgQueue.Enqueue(msg);
            }
        }

        public void EnqueueQueryMessage(string msg)
        {
            QueryallQueue.Enqueue(msg);
        }

        public override string ToString()
        {
            return connection.ConnectionInfo.ClientIpAddress + ":" + connection.ConnectionInfo.ClientPort.ToString() + " createTime:" + createTime.ToString() + " lastdealTime:" + lastdealTime.ToString(); ;
        }
        public void Done(string line)
        {
            Send(line);
            currentMsg.dealTime = System.DateTime.Now;
            currentMsg.retval = line;
            currentMsg.isreturn = true;
            lastdealTime = currentMsg.dealTime;
            lock (MsgQueue)
            {
                MsgQueue.Dequeue();
            }
            FinishQueue.Enqueue(currentMsg);
        }

        public string GetAddr()
        {
            return connection.ConnectionInfo.ClientIpAddress + ":" + connection.ConnectionInfo.ClientPort.ToString();
        }

        public Msg GetCurrentMsg()
        {
            currentMsg = MsgQueue.Peek();
            return currentMsg;
        }

        public int GetMsgCount()
        {
            return MsgQueue.Count + FinishQueue.Count + QueryallQueue.Count;
        }

        public void Send(string line)
        {
            connection.Send(line);
        }
        //检查用户活跃度，10分钟不操作认为离线
        public bool isActive()
        {
            DateTime currentTime = System.DateTime.Now;
            TimeSpan span = currentTime.Subtract(lastdealTime);
            if (span.Minutes > 0)
            {
                return false;
            }
            return true;
        }
        //检查消息处理，20秒不操作认为离线
        public bool Check()
        {
            DateTime currentTime = System.DateTime.Now;
            Msg firstMsg = GetCurrentMsg();
            TimeSpan span = currentTime.Subtract(firstMsg.createTime);
            if (span.Seconds > 20)
            {
                return false;
            }
            return true;
        }
    }

    class Msg
    {
        public string id { get; set; }
        public string message { get; set; }
        public List<string> mList { get; set; }
        public bool isreturn { get; set; }
        public string retval { get; set; }
        public DateTime createTime { get; set; }
        public DateTime dealTime { get; set; }
        public int dealCount { get; set; }
        public Msg()
        {
            id = "";
            message = "";
            retval = "";
            mList = new List<string>();
            isreturn = false;
            createTime = System.DateTime.Now;
            dealTime = System.DateTime.Now;
            dealCount = 0;
        }
        public Msg(string message)
        {
            id = "";
            this.message = message;
            retval = "";
            mList = new List<string>();
            isreturn = false;
            createTime = System.DateTime.Now;
            dealTime = System.DateTime.Now;
            dealCount = 0;
        }
        public Msg(string id, string message)
        {
            this.id = id;
            this.message = message;
            retval = "";
            mList = new List<string>();
            isreturn = false;
            createTime = System.DateTime.Now;
            dealTime = System.DateTime.Now;
            dealCount = 0;
        }
        public bool CheckDate()
        {
            if (dealCount > 5)
            {
                return false;
            }
            return true;
        }
    }

    class User
    {
        public List<IWebSocketConnection> allSockets;
        public List<Role> allRoles;
        public Role currentRole { get; set; }
        public Role currentMsg { get; set; }
        public User()
        {
            allSockets = new List<IWebSocketConnection>();
            allRoles = new List<Role>();
        }

        public void Add(Role role)
        {
            allRoles.Add(role);
            allSockets.Add(role.connection);
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


        public int getSize()
        {
            return allRoles.Count;
        }
    }
}
