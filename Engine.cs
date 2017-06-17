using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections;

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
        public Queue<Role> EngineQueue; 
        public Role currentRole;

        public void Start()
        {
            //启动引擎线程
            Init();    
            //启动管道线程
            StartPipeThread();
            //启动消费者线程
            StartCustomerThread();
            result = new List<string>();
            EngineQueue = new Queue<Role>();
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
                        }

                        if (line.IndexOf("bestmove") != -1)
                        {
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
            WriteInfo(role.GetAddr() + " Connected! They are " + user.getSize() + " Clients online");
        }

        public void OnClose(IWebSocketConnection socket)
        {
            var role = user.GetAt(socket);
            WriteInfo(role.GetAddr() + " Close!");
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
                          //  DealQueryallMessage(socket, message);
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
            role.EnqueueMessage(new Msg(message));

            //查redis表，有的话返回结果，没有加入引擎队列
            if (redis.ContainsKey(message))
            {
                getFromList(role, message);
            }
            else
            {
                //将role加入引擎处理队列，如果队列中已经存在则刷新队列
                if (!EngineQueue.Contains(role))
                {
                   EngineQueue.Enqueue(role);
                }
                else
                {
                    Flush(role);
                }               
            }
        }

        public void CustomerThread()
        {
            while (true)
            {
                try
                {        
                    if (EngineQueue.Count > 0 && PipeWriter != null && isLock == false)
                    {
                        Console.WriteLine("There are " + EngineQueue.Count + " messages to deal");
                        //同步锁
                        isLock = true;
                        currentRole = EngineQueue.Dequeue();

                        Msg msg = currentRole.GetCurrentMsg();

                        Console.WriteLine("getFromEngine");
                        PipeWriter.Write(msg.message + "\r\n");
                        PipeWriter.Write("go depth " + Setting.level + "\r\n");
                        
                        //等待计算结果
//                         bool timeout = true;
//                         for (int i = 0; i < 10 * Setting.thinktimeout; i++)
//                         {
//                             if (msg.isreturn)
//                             {
//                                 timeout = false;
//                                 break;
//                             }
//                             Thread.Sleep(100);
//                         }
//                         //超时处理
//                         if (timeout)
//                         {
//                             WriteInfo("timeout");
//                             PipeWriter.Write("stop\r\n");
//                         }
                    }
                    Thread.Sleep(100);
                }
                catch (System.Exception ex)
                {
                    WriteInfo("[error] GetFromEngine " + ex.Message);
                    resetEngine();
                    isLock = false;
                }
            }
        }    
    }
}
