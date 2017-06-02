using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.Xml;
using System.Net;
using ServiceStack.Redis;
using RedisStudy;
using System.Timers;

namespace Fleck.aiplay
{    
    class Comm
    {
        private static StreamWriter PipeWriter { get; set; }
        private static bool isLock { get; set; }
        Log log;
        Log logPosition;
        Setting setting;
        Queue EngineerQueue;
        Queue DealSpeedQueue;
        RedisHelper redis;
        Process pProcess;
        Boolean isEngineRun;
        User user;
        private static int nMsgQueuecount { get; set; }
        private static int timeout { get; set; }        

        public void WriteInfo(string message, bool isOutConsole = true)
        {
            log.WriteInfo(message);
            if (isOutConsole)
            {
                Console.WriteLine(message);
            }
        }

        public void WritePosition(string message)
        {
            logPosition.WritePosition(message);            
        }
     
        public int getMsgQueueCount()
        {
            return EngineerQueue.Count;
        }
      
        public int getDealspeed()
        {
            return nMsgQueuecount;
        }

        public void resetEngine()
        {            
            KillPipeThread();
            LoadXml();
            WriteInfo("resetEngine:" + Setting.engine);
            StartPipeThread();
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
                WriteInfo("[error]  resetEngine" + ex.Message);
            }
            Thread.Sleep(100);
        }

        public string getDepth()
        {
            return Setting.level;
        }

        public string getThinktimeout()
        {
            return Setting.thinktimeout.ToString();
        }

        public bool getSupportCloudApi()
        {
            return Setting.isSupportCloudApi;
        }  
       
        private void LoadXml()
        {
            setting.LoadXml();
        }

        private int getUserCount()
        {
            return user.allSockets.Count;
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
        
        public void PipeThread()
        {
            isEngineRun = true;
            try
            {
                //管道参数初始化
                PipeInit(Setting.engine, "");
                //截取输出流
                StreamReader reader = pProcess.StandardOutput;
                //截取输入流
                PipeWriter = pProcess.StandardInput;
                //每次读取一行
                string line = reader.ReadLine();
                Console.WriteLine(line);
                int intDepth = 0;
                while (isEngineRun)
                {
                    line = reader.ReadLine();
                    Role role = user.currentRole;
                    if (role != null && line != null)
                    {
                        string[] sArray = line.Split(' ');
                        /* 消息过滤
                         * info depth 14 seldepth 35 multipv 1 score 19 nodes 243960507 nps 6738309 hashfull 974 tbhits 0 time 36205 
                         * pv h2e2 h9g7 h0g2 i9h9 i0h0 b9c7 h0h4 h7i7 h4h9 g7h9 c3c4 b7a7 b2c2 c9e7 c2c6 a9b9 b0c2 g6g5 a0a1 h9g7 
                         */
                        if (sArray.Length > 3 && sArray[1] == "depth" && sArray[3] == "seldepth")
                        {
                            intDepth = Int32.Parse(sArray[2]);
                            role.Send(line);
                            //插入redis表
                            redis.SetItemInList(role.GetCurrentMsg().message, intDepth - 1, line);
                            WriteInfo(line, false);
                        }

                        if (line.IndexOf("bestmove") != -1)
                        {
                            Console.WriteLine("depth " + intDepth);
                            WriteInfo(line);
                            role.Deal(line);
                            //返回结果后删除消息
                            user.currentRole = null;
                            EngineerQueue.Dequeue();
                            nMsgQueuecount++;
                            isLock = false;
                        }
                        Thread.Sleep(10);
                    }
                }
            }
            catch (System.Exception ex)
            {
                WriteInfo("[error] executeCommand " + ex.Message);
            }
        }        

        public void DealThread()
        {
            while (true)
            {
                try
                {
                    if (PipeWriter != null && EngineerQueue.Count > 0 && isLock == false)
                    {
                        //同步锁
                        isLock = true;

                        Msg msg = new Msg();
                        user.currentRole = (Role)EngineerQueue.Peek();
                        msg = user.currentRole.GetCurrentMsg();
    
                        if (PipeWriter != null)
                        {
                            WriteInfo("getFromEngineer");
                            PipeWriter.Write(msg.message + "\r\n");
                            PipeWriter.Write("go depth " + Setting.level + "\r\n");

                            redis.CheckItemInList(msg.message, Int32.Parse(Setting.level));

                            timeout = 0;
                        }                       
                        Thread.Sleep(10);                        
                    }
                }
                catch (System.Exception ex)
                {
                    WriteInfo("[error] DealMessage " + ex.Message);
                    resetEngine();
                }

                Thread.Sleep(100);
            }
        }

        public void StatThread()
        {
            while (true)
            {
                if (DealSpeedQueue.Count >= 100)
                {
                    DealSpeedQueue.Dequeue();
                }
                DealSpeedQueue.Enqueue(nMsgQueuecount);
                nMsgQueuecount = 0;
                //休眠60s
                Thread.Sleep(60000);
            }
        }    

        public void EnqueueEngineerMessage(Role role)
        {
            EngineerQueue.Enqueue(role);
            //检查处理队列，异常重启引擎
            if (!role.Check())
            {
                EngineerQueue.Dequeue();
                resetEngine();
            }
        }   
        
        public void Start()
        {
            //初始化对象
            Init();           
            //启动管道线程
            StartPipeThread();
            //启动处理线程
            StartDealThread();
            //统计在线用户的提交量
            StartStatThread(); 
        }

        public void Init()
        {
            setting = new Setting();
            log = new Log();
            logPosition = new Log("Position");
            EngineerQueue = new Queue();
            DealSpeedQueue = new Queue();                
            user = new User();
            redis = new RedisHelper();
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
                            DealQueryallMessage(socket, message);                            
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

        private void DealActiveMessage(IWebSocketConnection socket)
        {
            int no = 0;
            foreach (Role r in user.allRoles)
            {
                if (r.isActive())
                {
                    socket.Send((no++)+1 + ": " + r.ToString());
                }
            }           
            socket.Send("There are " + no + " clients active.");
        }
        
        private void DealListMessage(IWebSocketConnection socket)
        {
            int no = 0;
            user.allRoles.ToList().ForEach(
                r => socket.Send((no++)+1 + ": " + r.ToString())
                );
            socket.Send("There are " + no + " clients online.");
        }

        private void DealQueryallMessage(IWebSocketConnection socket, string message)
        {
            string str = redis.QueryallFromCloud(message);
            if (str != null)
            {
                socket.Send(str);
            }
        }

        private void DealPositionMessage(IWebSocketConnection socket, string message)
        {
            //记录每个用户的消息队列
            var role = user.GetAt(socket);
            role.EnqueueMessage(new Msg(message));
            //查redis表
            List<string> list = redis.GetAllItemsFromList(message);
            string strmsg = "";
            int nlevel = Int32.Parse(Setting.level);
            if (list.Count >= nlevel)
            {
                WriteInfo("getFromList");
                //过滤空消息
                for (int i = 0; i < nlevel; i++)
                {
                    if (list[i].Length > 0)
                    {
                        strmsg = list[i];
                        socket.Send(strmsg);                        
                    }
                }            
                if (strmsg.Length > 0)
                {    
                    string[] infoArray = strmsg.Split(' ');
                    for (int j = 0; j < infoArray.Length; j++)
                    {
                        if (infoArray[j] == "pv")
                        {
                            role.Deal("bestmove " + infoArray[j + 1]);
                            WriteInfo("depth " + infoArray[2]+" bestmove " + infoArray[j + 1]);
                            return;
                        }
                    }
                }
            }
            //加入引擎处理队列          
            EnqueueEngineerMessage(role);

            Console.WriteLine("There are " + getMsgQueueCount() + " messages haven't deal.");
        }
        
        public void StartPipeThread()
        {
            Thread pipeThread = new Thread(new ThreadStart(PipeThread));
            pipeThread.IsBackground = true;
            pipeThread.Start();
            isLock = false;
        }

        public void StartDealThread()
        {
            Thread dealThread = new Thread(new ThreadStart(DealThread));
            dealThread.IsBackground = true;
            dealThread.Start();
        }

        public void StartStatThread()
        {
            Thread statThread = new Thread(new ThreadStart(StatThread));
            statThread.IsBackground = true;
            statThread.Start();
            nMsgQueuecount = 0;        
        }       
    }
}
