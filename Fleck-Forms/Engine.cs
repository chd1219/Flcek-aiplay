using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text.RegularExpressions;

namespace Fleck.aiplay
{
    class Engine : Comm
    {
        private static StreamWriter PipeWriter { get; set; }
        private static bool bLock { get; set; }
        Process pProcess;
        Boolean bEngineRun;
        DateTime EngineRunTime;
        List<string> result;
        public Queue<Role> InputEngineQueue; 
        public Role currentRole;
        public string tmpmessage;
        public Queue OutputEngineQueue;
        public bool bJson { get; set; }
        public int getMsgQueueCount()
        {
             return InputEngineQueue.Count;
        }

        public bool CheckInputEngineQueue()
        {
            if (InputEngineQueue.Count > 0)
            {
                return true;
            }
            return false;
        }

        public void AddOutput(string line, bool save = false)
        {
            if (save)
            {
                WriteInfo(line);
            }
            lock (OutputEngineQueue)
            {
                OutputEngineQueue.Enqueue(line);
            }
        }

        public void Start()
        {
            //启动引擎线程
            Init();    
            //启动管道线程
            StartPipeThread();
            //启动消费者线程
            StartCustomerThread();
            result = new List<string>();
            InputEngineQueue = new Queue<Role>();
            OutputEngineQueue = new Queue();
            bLock = false;
            currentRole = null;
            bJson = false;
        }

        public void StartPipeThread()
        {
            Thread pipeThread = new Thread(new ThreadStart(PipeThread));
            pipeThread.IsBackground = true;
            pipeThread.Start();
        }

        private void PipeInit(string strFile, string arg)
        {
            pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo = new System.Diagnostics.ProcessStartInfo();
            pProcess.StartInfo.FileName = strFile;
            pProcess.StartInfo.Arguments = arg;
            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.RedirectStandardInput = true;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
        }

        private void KillPipeThread()
        {
            bEngineRun = false;
            try
            {
                if (pProcess != null)
                {
                    pProcess.Kill();
                    pProcess.Close();
                }                
                PipeWriter = null;
            }
            catch (System.Exception ex)
            {                
                AddOutput("[error] KillPipeThread " + ex.Message, true);
            }
            Thread.Sleep(100);
        }

        public void PipeThread()
        {            
            EngineRunTime = System.DateTime.Now;
            int intDepth = 0;
            string line = "";
            try
            {
                //管道参数初始化
                PipeInit(Setting.engine, "");
                //截取输出流
                StreamReader reader = pProcess.StandardOutput;
                //截取输入流
                PipeWriter = pProcess.StandardInput;
                //每次读取一行
                line = reader.ReadLine();
                AddOutput(line); 
                line = reader.ReadLine();
                AddOutput(line);
                bEngineRun = true;

                while (bEngineRun)
                {
                    line = reader.ReadLine();

                    if (line != null)
                    {
                        string[] sArray = line.Split(' ');
                        /* 消息过滤
                         * info depth 14 seldepth 35 multipv 1 score 19 nodes 243960507 nps 6738309 hashfull 974 tbhits 0 time 36205 
                         * pv h2e2 h9g7 h0g2 i9h9 i0h0 b9c7 h0h4 h7i7 h4h9 g7h9 c3c4 b7a7 b2c2 c9e7 c2c6 a9b9 b0c2 g6g5 a0a1 h9g7 
                         */
                        if (sArray.Length > 3 && sArray[1] == "depth" && sArray[3] == "seldepth")
                        {
                            intDepth = Int32.Parse(sArray[2]);
                            currentRole.Send(line);
                            currentRole.GetCurrentMsg().mList.Add(line);
                            redis.PushItemToList(currentRole.GetCurrentMsg().message, line);
                        }

                        if (line.IndexOf("bestmove") != -1)
                        {
                            AddOutput("depth " + intDepth.ToString() + " " + line);
                            currentRole.Done(line);
                            InputEngineQueue.Dequeue();
                            bLock = false;
                        }
                        Thread.Sleep(10);
                    }
                }
            }
            catch (System.Exception ex)
            {
                AddOutput("[error] PipeThread " + ex.Message, true);
                resetEngine();                
            }
        }
        
        public void resetEngine()
        {
            bLock = false;
            KillPipeThread();
            //启动管道线程
            StartPipeThread();
        }

        internal void Flush(Role role)
        {
            if (role == currentRole)
            {
                PipeWriter.Write("stop\r\n");
            }            
        }

        public void StartCustomerThread()
        {
            Thread customerThread = new Thread(new ThreadStart(CustomerThread));
            customerThread.IsBackground = true;
            customerThread.Start();
        }

        public Role GetRoleAt(IWebSocketConnection socket)
        {
            return user.GetAt(socket);
        }

        public void OnOpen(IWebSocketConnection socket)
        {
            var role = new Role(socket);
            user.Add(role);
        }

        public void OnClose(IWebSocketConnection socket)
        {
            var role = user.GetAt(socket);
            user.Remove(socket);
        }

        public void OnMessage(IWebSocketConnection socket, string message)
        {
            switch (message)
            {
                case "HeartBeat":
                    break;
                case "count":
                    {
                        socket.Send("There are " + getUserCount() + " clients online.");
                        break;
                    }
                case "msgcount":
                    {
                        socket.Send("There are " + getMsgQueueCount() + " messages haven't deal.");
                        break;
                    }
                case "activecount":
                    {
                        socket.Send(getActiveCount().ToString());
                        break;
                    }
                case "dealspeed":
                    {
                        socket.Send("The deal speed is " + getDealspeed() + " peer minute.");
                        break;
                    }
                case "timeout":
                    {
                        socket.Send("The thinktimeout is " + getThinktimeout() + " second.");
                        break;
                    }
                case "depth":
                    {
                        socket.Send(getDepth());
                        break;
                    }
                case "cloudapi":
                    {
                        socket.Send(getSupportCloudApi().ToString());
                        break;
                    }
                case "reload":
                    {
                        LoadXml();
                        break;
                    }
                case "reset":
                    {
                        resetEngine();
                        break;
                    }
                case "list":
                    {
                        DealListMessage(socket);
                        break;
                    }//统计在线用户的活跃度
                case "active":
                    {
                        DealActiveMessage(socket);
                        break;
                    }
                default:
                    {
                        //过滤命令
                        if (message.IndexOf("queryall") != -1)
                        {
                            string str = DealQueryallMessage(message);
                            AddOutput(str);
                            socket.Send(str);
                            var role = user.GetAt(socket);

                            role.EnqueueQueryMessage(message);                               

                        }
                        else if (message.IndexOf("position") != -1)
                        {
                            DealPositionMessage(socket, message);
                            WritePosition(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " " + message);
                        }

                        break;
                    }
            }

        }

        private void DealPositionMessage(IWebSocketConnection socket, string message)
        {
            //记录每个用户的消息队列
            var role = user.GetAt(socket);
            Msg msg;
            if (bJson)
            {
                msg = Json2Msg(message);
            }
            else
            {
                msg = new Msg(message);
            }
            if (msg != null)
            {
                role.EnqueuePositionMessage(msg);
                InputEngineQueue.Enqueue(role);
            }          
        }
        
        public void CustomerThread()
        {
            Msg msg;
            while (true)
            {                
                try
                {
                    if (CheckInputEngineQueue() && bEngineRun && !bLock)
                    { 
                        //同步锁
                        bLock = true;
                        currentRole = InputEngineQueue.Peek();
                        msg = currentRole.GetCurrentMsg();
                        
                        EngineDeal(msg.message);
                       
                    }
                    Thread.Sleep(10);
                }
                catch (System.Exception ex)
                {                    
                    AddOutput("[error] GetFromEngine " + ex.Message, true);
                    resetEngine();
                }
            }
        }

        public void EngineDeal(string message)
        {
            if (redis.ContainsKey(message))
            {
                OutputEngineQueue.Enqueue("getFromList");
                getFromList(currentRole, message);
                InputEngineQueue.Dequeue();
                bLock = false;
            }
            else
            {
                OutputEngineQueue.Enqueue("getFromEngine");
                PipeWriter.Write(message + "\r\n");
                PipeWriter.Write("go depth " + Setting.level + "\r\n");
                Thread.Sleep(50);
            }
        }

        public void stopEngine()
        {
            bLock = false;
            KillPipeThread();
            AddOutput("引擎停止！");
        }
    }
}
