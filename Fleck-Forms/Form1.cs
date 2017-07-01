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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Management;
using System.Collections.Concurrent;
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
        
        private void Form1_Load(object sender, EventArgs e)
        {       
            InitListView();

            comm = new Engine();            
            comm.Start();
            countQueue = new Queue();
            MsgCount = 0;
            RunTime = System.DateTime.Now;
  
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
                    comm.OnMessage(socket, message);
                    Role role = comm.GetRoleAt(socket);
                    string[] names = { DateTime.Now.ToLongTimeString(), role.GetAddr(), role.GetMsgCount().ToString(),message };
                    AddMsg(names); 
                };
            });
        }

        private void AddMsg(string [] role)
        {
            try
            {
                MsgCount++;
                this.Invoke(this.addMsgDelegate, new Object[] { role });
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (comm != null)
            {
                string m_online = comm.getUserCount().ToString();

                string m_msg = MsgCount.ToString() + " 个";

                countQueue.Enqueue(MsgCount);
                if (countQueue.Count > 60)
                {
                    countQueue.Dequeue();
                }
                string m_speed = (MsgCount - (int)countQueue.Peek()).ToString() + " 个/分钟";

                string m_undo = comm.getMsgQueueCount().ToString() + " 个";

                string m_time = RunTime.ToString();

                DateTime currentTime = System.DateTime.Now;
                TimeSpan span = currentTime.Subtract(RunTime);
                string m_span = span.Days + "天" + span.Hours + "时" + span.Minutes + "分" + span.Seconds + "秒";

                string m_CPU = comm.getCurrentCpuUsage();
                string m_Memory = comm.getAvailableRAM();

                string[] names = { DateTime.Now.ToLongTimeString(),m_online, m_msg, m_speed, m_undo, m_CPU, m_Memory, m_time, m_span };
                AddListViewItem(listView4, names, 0);

                //显示引擎信息
                lock (comm.OutputEngineQueue)
                {
                    int num = comm.OutputEngineQueue.Count;
                    for (int i = 0; i < num; i++ )
                    {
                        string q = comm.OutputEngineQueueDequeue();
                        string[] str = { DateTime.Now.ToLongTimeString(), q };
                        AddListViewItem(listView3, str);
                    }
                }               
            }
        }

        private void InitListView()
        {
            listView1.GridLines = true;
            //单选时,选择整行
            listView1.FullRowSelect = true;
            //显示方式
            listView1.View = View.Details;
            //没有足够的空间显示时,是否添加滚动条
            listView1.Scrollable = true;
            //是否可以选择多行
            listView1.MultiSelect = false;

            listView1.View = View.Details;
            listView1.Columns.Add("时间", 60);
            listView1.Columns.Add("用户", 132);
            listView1.Columns.Add("状态", 73);

            listView2.GridLines = true;
            //单选时,选择整行
            listView2.FullRowSelect = true;
            //显示方式
            listView2.View = View.Details;
            //没有足够的空间显示时,是否添加滚动条
            listView2.Scrollable = true;
            //是否可以选择多行
            listView2.MultiSelect = false;

            listView2.View = View.Details;
            listView2.Columns.Add("时间", 60);
            listView2.Columns.Add("用户", 132);
            listView2.Columns.Add("序号", 40);
            listView2.Columns.Add("命令", 150);

            listView3.GridLines = true;
            //单选时,选择整行
            listView3.FullRowSelect = true;
            //显示方式
            listView3.View = View.Details;
            //没有足够的空间显示时,是否添加滚动条
            listView3.Scrollable = true;
            //是否可以选择多行
            listView3.MultiSelect = false;

            listView3.View = View.Details;
            listView3.Columns.Add("时间", 60);
            listView3.Columns.Add("命令", 400);

            listView4.GridLines = true;
            //单选时,选择整行
            listView4.FullRowSelect = true;
            //显示方式
            listView4.View = View.Details;
            //没有足够的空间显示时,是否添加滚动条
            listView4.Scrollable = true;
            //是否可以选择多行
            listView4.MultiSelect = false;

            listView4.View = View.Details;
            listView4.Columns.Add("  时间", 60, HorizontalAlignment.Center);
            listView4.Columns.Add("在线用户", 100, HorizontalAlignment.Center);
            listView4.Columns.Add("接受请求", 100, HorizontalAlignment.Center);
            listView4.Columns.Add("处理速度", 100, HorizontalAlignment.Center);
            listView4.Columns.Add("等待处理", 100, HorizontalAlignment.Center);
            listView4.Columns.Add("CPU使用", 100, HorizontalAlignment.Center);
            listView4.Columns.Add("剩余内存", 100, HorizontalAlignment.Center);
            listView4.Columns.Add("启动时间", 200, HorizontalAlignment.Center);
            listView4.Columns.Add("运行时间", 100, HorizontalAlignment.Center);
        }

        private void AddListViewItem(ListView listView, string[] array,int showLines = 28)
        {
            if (listView.Items.Count > showLines)
            {
                listView.Items.Clear();
            }

            listView.BeginUpdate();
            ListViewItem lvItem;
            ListViewItem.ListViewSubItem lvSubItem;
            lvItem = new ListViewItem();
            lvItem.Text = array[0];
            listView.Items.Add(lvItem);

            for (int x = 1; x < array.Length; x++)
            {
                lvSubItem = new ListViewItem.ListViewSubItem();
                lvSubItem.Text = array[x];
                lvItem.SubItems.Add(lvSubItem);
            }
            listView.EndUpdate();
        }

        Engine comm;
        DateTime RunTime;
        int MsgCount;
        Queue countQueue;

        //安全调用控件
        public delegate void AddConnectionItem(IWebSocketConnection socket);
        public AddConnectionItem addListDelegate;

        public delegate void AddMsgItem(string [] message);
        public AddMsgItem addMsgDelegate;

        public delegate void RemoveConnectionListItem(IWebSocketConnection socket);
        public RemoveConnectionListItem removeListDelegate;

        public void AddMsgItemMethod(string [] message)
        {
            AddListViewItem(listView2, message);
            System.Threading.Thread.Sleep(1);
        }

        public void AddListItemMethod(IWebSocketConnection socket)
        {
            string address = socket.ConnectionInfo.ClientIpAddress;
            string port = socket.ConnectionInfo.ClientPort.ToString();
            string str = address + ":" + port;

            string[] names = { DateTime.Now.ToLongTimeString(), str, "connected!" };
            AddListViewItem(listView1,names);

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
            
            string[] names = { DateTime.Now.ToLongTimeString(), str, "closed!" };
            AddListViewItem(listView1,names);

            remove(address, port);            
        }

        public void remove(string address, string port)
        {
            TreeNode tn;
            string str;
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

        private void btn_reset_Click(object sender, EventArgs e)
        {
            comm.resetEngine();
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            listView2.Items.Clear();
            listView3.Items.Clear();
            listView4.Items.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comm.stopEngine();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            comm.bJson = checkBox1.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //comm.SQLite_Test();
        }
        
    }
}
