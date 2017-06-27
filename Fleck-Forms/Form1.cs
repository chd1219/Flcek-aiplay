using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Fleck.aiplay;
using Fleck;
using System.Threading;
using System.Collections;

namespace Fleck_Forms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            addListDelegate = new AddConnectionItem(AddListItemMethod);
            removeListDelegate = new RemoveConnectionListItem(RemoveListItemMethod);
            addMsgDelegate = new AddMsgItem(AddMsgItemMethod);

            InitializeComponent();            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (comm != null)
            {
                m_online.Text = comm.getUserCount().ToString();

                m_time.Text = RunTime.ToString();

                DateTime currentTime = System.DateTime.Now;
                TimeSpan span = currentTime.Subtract(RunTime);
                m_span.Text = span.Days + "天" + span.Hours + "时" + span.Minutes + "分" + span.Seconds + "秒";

                m_msg.Text = MsgCount.ToString() + " 个";

                countQueue.Enqueue(MsgCount);
                if (countQueue.Count > 60)
                {
                    countQueue.Dequeue();
                }
                m_speed.Text = (MsgCount-(int)countQueue.Peek()).ToString() + " 个/分钟";
            }
        }

        private void InitListView()
        {
            #region 增加Item的標題，共有三個列
            //1、創建標題，共三列
            listView1.View = View.Details;
            listView1.Columns.Add("文件名");
            listView1.Columns.Add("大小");
            listView1.Columns.Add("創建日期");
            listView1.BeginUpdate();
            #region 增加第一個Item
            //2、定義一個ListViewItem，在View.Details模式下，有點像第一列中一個值
            ListViewItem lvItem;
            //3、定義ListViewSubItem，在View.Details模式下，有點像第二列中一個值
            ListViewItem.ListViewSubItem lvSubItem;
            //實列化一個Item，在View.Details模式下，有點像加第一行的第一個值
            lvItem = new ListViewItem();
            //Item的顯示的文字
            lvItem.Text = "文件夾1";
            //4、Item增加到ListView控件中，即增加第一行。在View.Details模式下，有點像增加了第一個項目的第一列的第一個值
            listView1.Items.Add(lvItem);
            //實例化SubItem
            lvSubItem = new ListViewItem.ListViewSubItem();
            lvSubItem.Text = "10";
            //5、將SubItem增加到第一個Item中，在View.Details模式下，有點像增加了第一個項目的第二列的第一個值
            lvItem.SubItems.Add(lvSubItem);
            lvSubItem = new ListViewItem.ListViewSubItem();
            lvSubItem.Text = "20080114";
            //將SubItem增加到第一個Item中，在View.Details模式下，有點像增加了第一個項目的第三列的第一個值
            lvItem.SubItems.Add(lvSubItem);
            #endregion
            lvItem = new ListViewItem();
            lvItem.Text = "文件夾2";
            lvSubItem = new ListViewItem.ListViewSubItem();
            lvSubItem.Text = "20";
            lvItem.SubItems.Add(lvSubItem);
            lvSubItem = new ListViewItem.ListViewSubItem();
            lvSubItem.Text = "20080115";
            lvItem.SubItems.Add(lvSubItem);
            listView1.Items.Add(lvItem);
            #endregion
            listView1.EndUpdate();
        }
        private void Form1_Load(object sender, EventArgs e)
        {       
            InitListView();

            comm = new Engine();
            comm.Start();

            countQueue = new Queue();
            MsgCount = 0;
            lastMsgCount = 0;
            nSpeed = 0;
            nSpan = 0;
            RunTime = System.DateTime.Now;
            m_time.Text = RunTime.ToString();

            DateTime currentTime = System.DateTime.Now;
            TimeSpan span = currentTime.Subtract(RunTime);
            m_span.Text = span.Days + "天" + span.Hours + "时" + span.Minutes + "分" + span.Seconds + "秒";

            FleckLog.Level = LogLevel.Info;
            var server = new WebSocketServer("ws://0.0.0.0:" + Setting.port);

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    AddConnection(socket);
                    comm.OnOpen(socket);                    
                };
                socket.OnClose = () =>
                {
                    DelConnection(socket);
                    comm.OnClose(socket);
                };
                socket.OnMessage = message =>
                {
                    AddMsg(message);
                    comm.OnMessage(socket, message);
                };
            });
        }

        Engine comm;
        DateTime RunTime;
        int MsgCount;
        int lastMsgCount;
        int nSpeed;
        int nSpan;
        Queue countQueue;
        //安全调用控件
        public delegate void AddConnectionItem(IWebSocketConnection socket);
        public AddConnectionItem addListDelegate;

        public delegate void AddMsgItem(string message);
        public AddMsgItem addMsgDelegate;

        public delegate void RemoveConnectionListItem(IWebSocketConnection socket);
        public RemoveConnectionListItem removeListDelegate;

        public void AddMsgItemMethod(string message)
        {
            if (m_msgList.Items.Count > 30)
            {
                m_msgList.Items.Clear();
            }
            m_msgList.Items.Add(DateTime.Now.ToLongTimeString() + " " + message);
            System.Threading.Thread.Sleep(1);
        }

        public void AddListItemMethod(IWebSocketConnection socket)
        {
            string address = socket.ConnectionInfo.ClientIpAddress;
            string port = socket.ConnectionInfo.ClientPort.ToString();
            string str = address + ":" + port;
            if (m_connectionList.Items.Count > 30)
            {
                m_connectionList.Items.Clear();
            }
            m_connectionList.Items.Add(DateTime.Now.ToLongTimeString() + " " + str + " connected!");

            add(address, port);

        }
        public void add(string address, string port)
        {
            TreeNode tn;
            string str;
            bool isfind = false;
            int index = 0;
            for (index = 0; index < treeView1.Nodes.Count; index++)
            {
                str = treeView1.Nodes[index].Text;
                if (str.IndexOf(address) != -1)
                {
                    isfind = true;
                    break;
                }
            }

            if (isfind)
            {
                tn = treeView1.Nodes[index];
                tn.Name = tn.Text = address+"("+(tn.Nodes.Count+1)+")";
            }
            else
            {
                tn = new TreeNode();
                tn.Name = tn.Text = address;
                treeView1.Nodes.Add(tn);
            }

            tn.Nodes.Add(port);
            System.Threading.Thread.Sleep(1);
        }

        public void RemoveListItemMethod(IWebSocketConnection socket)
        {
            string address = socket.ConnectionInfo.ClientIpAddress;
            string port = socket.ConnectionInfo.ClientPort.ToString();
            string str = address + ":" + port;
            if (m_connectionList.Items.Count > 100)
            {
                m_connectionList.Items.Clear();
            }
            m_connectionList.Items.Add(DateTime.Now.ToLongTimeString() + " "+ str + " closed!");

            remove(address, port);            
        }

        public void remove(string address, string port)
        {
            TreeNode tn;
            string str;
            int index = 0;
            try
            {
                for (int i = 0; i < treeView1.Nodes.Count; i++)
                {
                    str = treeView1.Nodes[i].Text;
                    if (str.IndexOf(address) != -1)
                    {
                        tn = treeView1.Nodes[i];

                        if (tn.Nodes.Count > 1)
                        {
                            for (int j = 0; j < tn.Nodes.Count; j++)
                            {
                                str = tn.Nodes[j].Text;
                                if (str.IndexOf(port) != -1)
                                {
                                    tn.Nodes.Remove(tn.Nodes[j]);
                                    break;
                                }
                            }

                            if (tn.Nodes.Count > 1)
                            {
                                tn.Name = tn.Text = address + "(" + tn.Nodes.Count + ")";
                            }
                            else
                            {
                                tn.Name = tn.Text = address;
                            }

                        }
                        else
                        {
                            treeView1.Nodes.Remove(tn);
                        }

                        break;
                    }
                }

                System.Threading.Thread.Sleep(1);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
 
            
        }

       

        public void AddMsg(string str)
        {           
            try
            { 
                MsgCount++;
                this.Invoke(this.addMsgDelegate, new Object[] { str });
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void DelConnection(IWebSocketConnection socket)
        {
            try
            {
                this.Invoke(this.removeListDelegate, new Object[] { socket });
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void AddConnection(IWebSocketConnection socket)
        {
            try
            {
                this.Invoke(this.addListDelegate, new Object[] { socket });
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void btn_expend_Click(object sender, EventArgs e)
        {
            treeView1.ExpandAll();
        }

        private void btn_closeall_Click(object sender, EventArgs e)
        {
            treeView1.CollapseAll();
        }

    }
}
