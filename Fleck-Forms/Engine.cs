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
        private static bool isLock { get; set; }
        Process pProcess;
        Boolean isEngineRun;
        DateTime EngineRunTime;
        List<string> result;
        public Queue<Role> InputEngineQueue; 
        public Role currentRole;
        public string tmpmessage;
        public Queue OutputEngineQueue;

        public int getMsgQueueCount()
        {
             return InputEngineQueue.Count;
        }

        public void AddOutput(string line)
        {
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
            isLock = false;
            currentRole = null;
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
            isEngineRun = false;
            try
            {
                pProcess.Kill();
                pProcess.Close();
                PipeWriter = null;
            }
            catch (System.Exception ex)
            {
                WriteInfo("[error] KillPipeThread " + ex.Message);
                AddOutput("[error] KillPipeThread " + ex.Message);
            }
            Thread.Sleep(100);
        }

        public void PipeThread()
        {
            isEngineRun = true;
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
                Console.WriteLine(line);

                AddOutput(line);

                while (isEngineRun)
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
                            //redis.PushItemToList(tmpmessage, line);
                            AddOutput(line);
                            
                        }

                        if (line.IndexOf("bestmove") != -1)
                        {
                            AddOutput(line);
                            Console.WriteLine("depth " + intDepth);
                            currentRole.Done(line);                            
                            isLock = false;
                        }
                        Thread.Sleep(10);
                    }
                }
            }
            catch (System.Exception ex)
            {
                WriteInfo("[error] PipeThread " + ex.Message);
                AddOutput("[error] PipeThread " + ex.Message);
                resetEngine();
                isLock = false;
            }
        }
        
        public void resetEngine()
        {
            isLock = false;
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
                        }
                        else if (message.IndexOf("position") != -1)
                        {
                            DealPositionMessage(socket, message);
                            WritePosition(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " " + message);
                        }
                        else
                        {
                            Console.WriteLine(message);
                        }
                        break;
                    }
            }

        }

        private void DealPositionMessage(IWebSocketConnection socket, string message)
        {
            //记录每个用户的消息队列
            var role = user.GetAt(socket);
//            Msg msg = ParseJson(message);
            Msg msg = new Msg(message);

            role.EnqueueMessage(msg);
            InputEngineQueue.Enqueue(role);
        }

        public void CustomerThread1()
        {
            Thread.Sleep(3000);
            StreamReader sr = new StreamReader("C:\\MyProject\\Fleck-aiplay\\bin\\Release\\log\\Position.log", Encoding.Default);
            String line;
            while (true)
            {
                try
                {
                    line = sr.ReadLine();
                    if (PipeWriter != null && isLock == false && line != null)
                    {
                        string[] position = Regex.Split(line, "position");
                        line = "position" + position[1];
                        tmpmessage = line;
                        if (redis.ContainsKey(tmpmessage))
                        {
                            getFromList(tmpmessage);
                        }
                        else
                        {
                            Console.Write("1");
                            PipeWriter.Write(line.ToString() + "\r\n");
                            PipeWriter.Write("go depth " + Setting.level + "\r\n");
                            //同步锁
                            isLock = true;
                            Thread.Sleep(100);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    WriteInfo("[error] GetFromEngine " + ex.Message);
                    resetEngine();
                    isLock = false;
                }
            }
        }

        public void CustomerThread()
        {
            while (true)
            {
                try
                {               
                    if (InputEngineQueue.Count > 0 && PipeWriter != null && isLock == false)
                    { 
                        Console.WriteLine("There are " + InputEngineQueue.Count + " messages to deal");
                        //同步锁
                        isLock = true;
                        currentRole = InputEngineQueue.Dequeue();

                        Msg msg = currentRole.GetCurrentMsg();

                        if (redis.ContainsKey(msg.message))
                        {
                            Console.WriteLine("getFromList");
                            OutputEngineQueue.Enqueue("getFromList");
                            getFromList(currentRole, msg.message);
                            isLock = false;
                        }
                        else
                        {
                            Console.WriteLine("getFromEngine");
                            OutputEngineQueue.Enqueue("getFromEngine");
                            PipeWriter.Write(msg.message + "\r\n");
                            PipeWriter.Write("go depth " + Setting.level + "\r\n");
                            Thread.Sleep(50);
                        }
                    }
                    Thread.Sleep(10);
                }
                catch (System.Exception ex)
                {
                    WriteInfo("[error] GetFromEngine " + ex.Message);
                    AddOutput("[error] GetFromEngine " + ex.Message);
                    resetEngine();
                    isLock = false;
                }
            }
        }    
    }
}
