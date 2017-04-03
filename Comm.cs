﻿using System;
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
    class Setting
    {
        static public string port { get; set; }
        static public string level { get; set; }
        static public bool isSupportCloudApi { get; set; }
        static public string engine { get; set; }
        static public int thinktimeout { get; set; }
    }

    class Log
    {
        private string LogPath;
        private string spath = "log";
        private StreamWriter log;       
        public Log()
        {            
            if (!Directory.Exists(spath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(spath);
                directoryInfo.Create();
            }

            LogPath = DateTime.Now.ToLongDateString();
            log = new StreamWriter(spath + "/" + LogPath + "-" + Setting.port + ".log", true);
        }
        public void WriteInfo(string message)
        {
            if (LogPath != DateTime.Now.ToLongDateString())
            {
                log.Close();
                LogPath = DateTime.Now.ToLongDateString();
                log = new StreamWriter(spath + "/" + LogPath + "-" + Setting.port + ".log", true);
            }
            WriteInfo("{0}", message);
        }
        public void WriteInfo(string format, params object[] obj)
        {
            try
            {
                log.WriteLine(string.Format("[{0}] {1}", System.DateTime.Now, string.Format(format, obj)));
                log.Flush();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    class Role
    {
        public IWebSocketConnection connection { get; set; }
        public List<System.String> message { get; set; }
        public DateTime currentTime { get; set; }
        public Role()
        {
            connection = null;
            message = new List<System.String>();
            currentTime = new System.DateTime(); 
        }
    }

    class Msg
    {
        public IWebSocketConnection connection { get; set; }
        public string message { get; set; }
        public bool isreturn { get; set; }
        public Msg()
        {
            connection = null;
            message = "";
            isreturn = false;
        }
    }

    class Comm
    {
        private static StreamWriter PipeWriter { get; set; }
        private static Msg currentMsg { get; set; }
        Log log { get; set; }
        Queue MsgQueue;
        IRedisClient Redis;
        HashOperator operators;
        Process pProcess;
        Boolean isEngineRun;
        private static int nMsgQueuecount { get; set; }
        private static int timeout { get; set; }
        public void WriteInfo(string message)
        {
            log.WriteInfo(message);
        }
        public int getMsgQueueCount()
        {
            return MsgQueue.Count;
        }
        public int getDealspeed()
        {
            return nMsgQueuecount;
        }
        public void ReceiveMessage()
        {
            isEngineRun = true;
            executeCommand(Setting.engine, "");
            return;
        }

        public string QuerybestFromCloud(string board)
        {
            if (!Setting.isSupportCloudApi)
            {
                return null;
            } 
            string serverResult = "";
            try
            {
                serverResult = getFromRedis("Querybest:" + board);
                if (serverResult == null)
                {
                    string serverUrl = "http://api.chessdb.cn:81/chessdb.php?action=querybest&board=" + board;
                    string postData = "";
                    serverResult = HttpPostConnectToServer(serverUrl, postData);
                    setToRedis("Querybest:" + board, serverResult); 
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return serverResult;
        }
        public string getFromRedis(string key)
        {
            return Redis.GetValue(key);             
        }
        public void setToRedis(string key,string value)
        {
            Redis.SetEntryIfNotExists(key, value);
        }

        public string QueryallFromCloud(string message)
        {
            string board = message.Substring(9, message.Length - 9);
            if (!Setting.isSupportCloudApi)
            {
                return null;
            }
            string serverResult = "";
            try
            {
                serverResult = getFromRedis("Queryall:" + board);
                if (serverResult == null)
                {                    
                    string serverUrl = "http://api.chessdb.cn:81/chessdb.php?action=queryall&board=" + board;
                    string postData = "";                
                    serverResult = HttpPostConnectToServer(serverUrl, postData);
                    setToRedis("Queryall:"+board, serverResult);                    
                }
                serverResult = serverResult.Replace("move:", "");//替换为空
                serverResult = serverResult.Replace("score:", "");//替换为空
                serverResult = serverResult.Replace("rank:", "");//替换为空
                serverResult = serverResult.Replace("note:", "");//替换为空
                serverResult = "Queryall" + serverResult;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return serverResult;
        }

        public void DealMessage()
        {
            while (true)
            {
                try
                {
                    if (PipeWriter != null && MsgQueue.Count > 0)
                    {
                        Msg msg = new Msg();
                        msg = (Msg)MsgQueue.Dequeue();
                        currentMsg = msg;
                        string board = msg.message.Substring(13, msg.message.Length - 9 - 12);
                        string serverResult = QuerybestFromCloud(board);

                        if (PipeWriter != null)  PipeWriter.Write(msg.message + "\r\n");
                        string move = "";

                        if (serverResult != null && serverResult.IndexOf("move:") != -1)
                        {
                            //Console.WriteLine(serverResult);
                            if (board.Length > 61)
                            {
                                move = " bookmove " + serverResult.Substring(5, 4);
                            }
                            else if (board.Length < 51)
                            {
                                move = " egtbmove " + serverResult.Substring(5, 4);
                            }
                            //Console.WriteLine(move);
                            log.WriteInfo(move);
                        }
                        if (PipeWriter != null)
                        {
                            PipeWriter.Write("go depth " + Setting.level + move + "\r\n");
                            timeout = 0;
                        }
                        while (!msg.isreturn)
                        {
                            Thread.Sleep(10);
                        }
                        nMsgQueuecount++;
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Thread.Sleep(100);
            }
        }

        void executeCommand(string strFile, string args)
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
                while (isEngineRun)
                {
                    line = reader.ReadLine();
                    if (currentMsg != null && line != null)
                    {
                        string[] sArray = line.Split(' '); 
                        int intDepth = 0;
                        if (sArray[1] == "depth")
                            intDepth = Int32.Parse(sArray[2]);
                        //消息过滤，大于14层的消息才转发
                        /*info depth 14 seldepth 35 multipv 1 score 19 nodes 243960507 nps 6738309 hashfull 974 tbhits 0 time 36205 
                         * pv h2e2 h9g7 h0g2 i9h9 i0h0 b9c7 h0h4 h7i7 h4h9 g7h9 c3c4 b7a7 b2c2 c9e7 c2c6 a9b9 b0c2 g6g5 a0a1 h9g7 
                         */
                        if (intDepth > 13 && sArray[3] == "seldepth")
                        {
                           // Console.WriteLine(line);
                            currentMsg.connection.Send(line);
                        } 
                        
                        if (line.IndexOf("bestmove") != -1)
                        {
                            currentMsg.connection.Send(line);
                            log.WriteInfo(currentMsg.connection.ConnectionInfo.ClientIpAddress + ":" + currentMsg.connection.ConnectionInfo.ClientPort.ToString() + " " + line);
                            currentMsg.connection = null;
                            currentMsg.isreturn = true;
                        }
                    }  
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //p.WaitForExit();
        }

        public void Start()
        {
            LoadXml();
            log = new Log();
            MsgQueue = new Queue();
            nMsgQueuecount = 0;

            InitRedis();    

            OnReceive();

            OnDeal();

            OnTime();

            OnTimeDeal();
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
            if ((timeout++) > Setting.thinktimeout-1)
            {
                timeout = 0;
                if (currentMsg != null && currentMsg.isreturn == false)
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
        public void InitRedis()
        {
            //获取Redis操作接口
            Redis = RedisManager.GetClient();
            //Hash表操作
            operators = new HashOperator();

            Redis.Password = "jiao19890228";
        }


        public void resetEngine()
        {
            isEngineRun = false;
            pProcess.Kill();
            pProcess.Close();
            PipeWriter = null;
            LoadXml();
            log.WriteInfo("resetEngine:" + Setting.engine);
            OnReceive();
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

        public void OnReceive()
        {
            Thread receiveDataThread = new Thread(new ThreadStart(ReceiveMessage));
            receiveDataThread.IsBackground = true;
            receiveDataThread.Start();
        }

        public void OnDeal() 
        {
            Thread DealMeassgaehread = new Thread(new ThreadStart(DealMessage));
            DealMeassgaehread.IsBackground = true;
            DealMeassgaehread.Start();
        }

        public void OnMessage(Msg msg)
        {
            MsgQueue.Enqueue(msg);            
        }

        public void LoadXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(".\\config.xml");
            XmlNode xn = doc.SelectSingleNode("configuration");
            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode xn1 in xnl)
            {
                XmlElement xe = (XmlElement)xn1;
                if (xe.GetAttribute("key").ToString() == "Port")
                {
                    Setting.port = xe.GetAttribute("value").ToString();
                }
                if (xe.GetAttribute("key").ToString() == "Level")
                {
                    Setting.level = xe.GetAttribute("value").ToString();
                }
                if (xe.GetAttribute("key").ToString() == "Thinktimeout")
                {
                    Setting.thinktimeout = int.Parse(xe.GetAttribute("value").ToString());
                }
                if (xe.GetAttribute("key").ToString() == "CloudAPI")
                {
                    Setting.isSupportCloudApi = Convert.ToBoolean(xe.GetAttribute("value"));
                }
                if (xe.GetAttribute("key").ToString() == "EnginePath")
                {
                    Setting.engine = xe.GetAttribute("value").ToString();
                }
            }
        }

        public string HttpPostConnectToServer(string serverUrl, string postData)
        {
            var dataArray = Encoding.UTF8.GetBytes(postData);
            //创建请求  
            var request = (HttpWebRequest)HttpWebRequest.Create(serverUrl);
            request.Method = "POST";
            request.ContentLength = dataArray.Length;
            //设置上传服务的数据格式  
            request.ContentType = "application/x-www-form-urlencoded";
            //请求的身份验证信息为默认  
            request.Credentials = CredentialCache.DefaultCredentials;
            //请求超时时间  
            request.Timeout = 10000;
            //创建输入流  
            Stream dataStream;
            //using (var dataStream = request.GetRequestStream())  
            //{  
            //    dataStream.Write(dataArray, 0, dataArray.Length);  
            //    dataStream.Close();  
            //}  
            try
            {
                dataStream = request.GetRequestStream();
            }
            catch (Exception)
            {
                return null;//连接服务器失败  
            }
            //发送请求  
            dataStream.Write(dataArray, 0, dataArray.Length);
            dataStream.Close();
            //读取返回消息  
            string res = "";
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                res = reader.ReadToEnd();
                reader.Close();
            }
            catch (Exception ex)
            {
                log.WriteInfo(ex.Message);
                // mylog.WriteInfo("{\"error\":\"connectToServer\",\"error_description\":\"" + ex.Message + "\"}");//连接服务器失败  
            }
            return res;
        }

        
    }
}
