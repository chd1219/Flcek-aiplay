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
        Log log { get; set; }
        Setting setting;
        Queue RoleQueue;
        RedisHelper redis;
        Process pProcess;
        Boolean isEngineRun;
        User user;
        private static int nMsgQueuecount { get; set; }
        private static int timeout { get; set; }        

        public void WriteInfo(string message)
        {
            log.WriteInfo(message);
        }
     
        public int getMsgQueueCount()
        {
            return RoleQueue.Count;
        }
      
        public int getDealspeed()
        {
            return nMsgQueuecount;
        }

        public void resetEngine()
        {
            isEngineRun = false;
            try
            {
                pProcess.Kill();
                pProcess.Close();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                WriteInfo("[error]  resetEngine" + ex.Message);
            }

            PipeWriter = null;
            setting.LoadXml();
            WriteInfo("resetEngine:" + Setting.engine);
            OnPipe();
            isLock = false;
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
       
        public void PipeThread()
        {
            isEngineRun = true;
            Pipe(Setting.engine, "");
            return;
        }

        public void DealMessage()
        {
            while (true)
            {
                try
                {
                    if (PipeWriter != null && RoleQueue.Count > 0 && isLock == false)
                    {
                        isLock = true;

                        Msg msg = new Msg();
                        user.currentRole = (Role)RoleQueue.Peek();
                        msg = user.GetCurrentMsg();
    
                        if (PipeWriter != null)
                        {
                            WriteInfo(msg.message);
                            WriteInfo("getFromEngineer");
                            Console.WriteLine("getFormEngineer");
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
                    Console.WriteLine(ex.Message);
                    WriteInfo("[error] DealMessage" + ex.Message);
                    resetEngine();
                }

                Thread.Sleep(100);
            }
        }

        void Pipe(string strFile, string args)
        {
            try
            {
                pProcess = new System.Diagnostics.Process();
                pProcess.StartInfo = new System.Diagnostics.ProcessStartInfo();
                pProcess.StartInfo.FileName = strFile;
                pProcess.StartInfo.Arguments = args;
                pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.RedirectStandardInput = true;
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.CreateNoWindow = true;
                pProcess.Start();
                StreamReader reader = pProcess.StandardOutput;//截取输出流
                PipeWriter = pProcess.StandardInput;//截取输入流

                string line = reader.ReadLine();//每次读取一行
                Console.WriteLine(line);
                int intDepth = 0;
                while (isEngineRun)
                {
                    line = reader.ReadLine();
                    if (user.currentRole != null && line != null)
                    {
                        string[] sArray = line.Split(' ');                        
                        if (sArray[1] == "depth")
                            intDepth = Int32.Parse(sArray[2]);
                        //消息过滤，大于1层的消息才转发
                        /*info depth 14 seldepth 35 multipv 1 score 19 nodes 243960507 nps 6738309 hashfull 974 tbhits 0 time 36205 
                         * pv h2e2 h9g7 h0g2 i9h9 i0h0 b9c7 h0h4 h7i7 h4h9 g7h9 c3c4 b7a7 b2c2 c9e7 c2c6 a9b9 b0c2 g6g5 a0a1 h9g7 
                         */
                        if (intDepth > 0 && sArray[3] == "seldepth")
                        {
                            //Console.WriteLine(line);                           
                            user.Send(line);
                           // List<string> list = GetAllItemsFromList(currentMsg.message);
                           // if (list.Count < intDepth)
                            {
                                redis.SetItemInList(user.GetCurrentMsg().message, intDepth - 1, line);
                                WriteInfo(line);
                            }
                        }

                        if (line.IndexOf("bestmove") != -1)
                        { 
                            Console.WriteLine("depth " + intDepth);
                            Console.WriteLine(line);
                            WriteInfo(line);
                            user.Deal(line);
                            
                            //返回结果后删除消息
                            user.currentRole = null;
                            RoleQueue.Dequeue();
                            nMsgQueuecount++;
                            isLock = false;
                        }
                        Thread.Sleep(10);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                WriteInfo("[error] executeCommand " + ex.Message);
            }
            //p.WaitForExit();
        }
        //消息队列
        public void EnqueueMessage(Role role)
        {
            RoleQueue.Enqueue(role);
            //30秒未处理则重启引擎
            DateTime tt = System.DateTime.Now;
            Msg firstMsg = role.dealList.Peek();
            TimeSpan span = tt.Subtract(firstMsg.createTime);
            if (span.Seconds > 20)
            {
                RoleQueue.Dequeue();
                resetEngine();
            }
        }   
        
        public void Start()
        {
            setting = new Setting();
            log = new Log();
            RoleQueue = new Queue();
            nMsgQueuecount = 0;
            isLock = false;
            user = new User();
            redis = new RedisHelper();

            OnPipe();

            OnDeal();

            OnTime();

            OnTimeDeal();
        }

        public void OnOpen(IWebSocketConnection socket)
        {
            user.Add(socket);
            Console.WriteLine(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " Connected!");
            WriteInfo(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " Connected!");                        
        }
       
        public void OnClose(IWebSocketConnection socket)
        {
            Console.WriteLine(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " Close!");
            WriteInfo(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " Close!");
            user.Remove(socket);           
        }

        public void OnMessage(IWebSocketConnection socket, string message)
        {
            string[] sArray = message.Split(' ');
            if (sArray[0] == "listmessage" && sArray[1] == "at" && sArray.Length == 3)
            {
                int seek = Int32.Parse(sArray[2]);
//                 if (seek < allRoles.Count)
//                 {
//                     var r = allRoles[seek];
//                     var connection = r.connection;
//                     socket.Send(connection.ConnectionInfo.ClientIpAddress + ":" + connection.ConnectionInfo.ClientPort.ToString() + " " + r.currentTime.ToString() + " open");
//                     foreach (var m in r.message.ToList())
//                     {
//                         socket.Send(connection.ConnectionInfo.ClientIpAddress + ":" + connection.ConnectionInfo.ClientPort.ToString() + " " + m);
//                     }
//                 }
                return;
            }
            if (message == "listallmessage")
            {
//                 foreach (var r in allRoles.ToList())
//                 {
//                     var connection = r.connection;
//                     socket.Send(connection.ConnectionInfo.ClientIpAddress + ":" + connection.ConnectionInfo.ClientPort.ToString() + " " + r.currentTime.ToString() + " open");
//                     foreach (var m in r.message.ToList())
//                     {
//                         socket.Send(connection.ConnectionInfo.ClientIpAddress + ":" + connection.ConnectionInfo.ClientPort.ToString() + " " + m);
//                     }
//                 }
               return;
            }          

            //comm.WriteInfo(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " " + message);        

            if (message.IndexOf("queryall") != -1)
            {
                string strquery = redis.QueryallFromCloud(message);
                if (strquery != null)
                {
                    socket.Send(strquery);
                }
                return;
            }
            if (message == "HeartBeat")
            {
                return;
            }
            if (message == "count")
            {
               // socket.Send("There are " + user.allSockets.Count + " clients online.");
            }
            if (message == "list")
            {
//                 int no = 1;
//                 allSockets.ToList().ForEach(
//                     s => socket.Send((no++) + ": " + s.ConnectionInfo.ClientIpAddress + ":" + s.ConnectionInfo.ClientPort.ToString())
//                     );
//                 socket.Send("There are " + allSockets.Count + " clients online.");
            }
            if (message == "msgcount")
            {
                socket.Send("There are " + getMsgQueueCount() + " messages haven't deal.");
            }
            if (message == "dealspeed")
            {
                socket.Send("The deal speed is " + getDealspeed() + " peer minute.");
            }
            if (message == "timeout")
            {
                socket.Send("The thinktimeout is " + getThinktimeout() + " second.");
            }
            if (message == "depth")
            {
                socket.Send(getDepth());
            }
            if (message == "cloudapi")
            {
                socket.Send(getSupportCloudApi().ToString());
            }
            if (message == "reload")
            {
                setting.LoadXml();
            }
            if (message == "reset")
            {
                resetEngine();
            }
            //过滤命令
            if (message.IndexOf("position") == -1)
            {
                // socket.Send(message);
                Console.WriteLine(message);
                return;
            }

            List<string> list = redis.GetAllItemsFromList(message);
            string strmsg;
            int nlevel = Int32.Parse(Setting.level);
            if (list.Count >= nlevel)
            {
                WriteInfo(message);
                WriteInfo("getItemFromList");
                Console.WriteLine("getFromList");

                for (int i = 0; i < nlevel; i++)
                {
                    strmsg = list[i];
                    if (strmsg.Length > 0)
                    {
                        socket.Send(strmsg);
                        WriteInfo(strmsg);
                    }

                    if (strmsg.Length == 0)
                    {
                        string info = list[i - 1];
                        if (info.Length > 0)
                        {
                            string[] infoArray = info.Split(' ');
                            for (int j = 0; j < info.Length; j++)
                            {
                                if (infoArray[j] == "pv")
                                {
                                    socket.Send("bestmove " + info[j + 1]);
                                    return;
                                }
                            }
                        }
                    }

                    if (i == nlevel - 1)
                    {
                        string[] infoArray = strmsg.Split(' ');
                        for (int j = 0; j < strmsg.Length; j++)
                        {
                            if (infoArray[j] == "pv")
                            {
                                socket.Send("bestmove " + strmsg[j + 1]);
                                return;
                            }
                        }

                    }
                }
            }
 
            var role = user.GetAt(socket);
            role.EnqueueMessage(new Msg(message));


            Console.WriteLine("There are " + getMsgQueueCount() + " messages haven't deal.");
            return;
        }
      
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            // Console.WriteLine(nMsgQueuecount);
            nMsgQueuecount = 0;
        }

        public void OnTime()
        {
            System.Timers.Timer t = new System.Timers.Timer();
            t.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            t.Interval = 60000;
            t.Enabled = true;
        }

        private static void OnTimedEventDeal(object source, ElapsedEventArgs e)
        {
            if ((timeout++) > Setting.thinktimeout - 1)
            {
                timeout = 0;
              //  if (user.currentRole != null && currentMsg.isreturn == false)
                {
                    PipeWriter.Write("stop\r\n");
                }
            }
        }
        
        public void OnTimeDeal()
        {
            System.Timers.Timer t = new System.Timers.Timer();
            t.Elapsed += new ElapsedEventHandler(OnTimedEventDeal);
            t.Interval = 1000;
            t.Enabled = true;
            timeout = 0;
        }
        
        public void OnPipe()
        {
            Thread pipeThread = new Thread(new ThreadStart(PipeThread));
            pipeThread.IsBackground = true;
            pipeThread.Start();
        }

        public void OnDeal()
        {
            Thread DealMeassgaehread = new Thread(new ThreadStart(DealMessage));
            DealMeassgaehread.IsBackground = true;
            DealMeassgaehread.Start();
        }       
    }
}
