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

namespace Fleck.aiplay
{
    class Setting
    {
        static public string port { get; set; }
        static public string level { get; set; }
        static public bool isSupportCloudApi { get; set; }
        static public string engine { get; set; }
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
        StreamWriter PipeWriter { get; set; }
        Msg currentmsg { get; set; }
        Log log;
        Queue MsgQueue;
        IRedisClient Redis;
        HashOperator operators;

        public void WriteInfo(string message)
        {
            log.WriteInfo(message);
        }

        public void ReceiveMessage()
        {
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
                    if (MsgQueue.Count > 0)
                    {
                        Msg msg = new Msg();
                        msg = (Msg)MsgQueue.Dequeue();
                        currentmsg = msg;
                        string board = msg.message.Substring(13, msg.message.Length - 9 - 12);
                        string serverResult = QuerybestFromCloud(board);

                        PipeWriter.Write(msg.message + "\r\n");
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
                        PipeWriter.Write("go depth " + Setting.level + move + "\r\n");
                        while (!msg.isreturn)
                        {
                            Thread.Sleep(10);
                        }
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
                Process p = new System.Diagnostics.Process();
                p.StartInfo = new System.Diagnostics.ProcessStartInfo();
                p.StartInfo.FileName = strFile;
                p.StartInfo.Arguments = args;
                p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                StreamReader reader = p.StandardOutput;//截取输出流
                PipeWriter = p.StandardInput;//截取输入流

                string line = reader.ReadLine();//每次读取一行
                Console.WriteLine(line);
                while (true)
                {
                    line = reader.ReadLine();
                    if (currentmsg != null && line != null)
                    {
                        currentmsg.connection.Send(line);
                        if (line.IndexOf("bestmove") != -1)
                        {
                            log.WriteInfo(currentmsg.connection.ConnectionInfo.ClientIpAddress + ":" + currentmsg.connection.ConnectionInfo.ClientPort.ToString() + " " + line);
                            currentmsg.connection = null;
                            currentmsg.isreturn = true;
                        }

                    }
                  //  Console.WriteLine(line);
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

            InitRedis();    

            OnReceive();
            OnDeal();

        }

        public void InitRedis()
        {
            //获取Redis操作接口
            Redis = RedisManager.GetClient();
            //Hash表操作
            operators = new HashOperator();

            Redis.Password = "jiao19890228";
        }

        public void ReloadXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(".\\config.xml");
            XmlNode xn = doc.SelectSingleNode("configuration");
            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode xn1 in xnl)
            {
                XmlElement xe = (XmlElement)xn1;
                if (xe.GetAttribute("key").ToString() == "Level")
                {
                    Setting.level = xe.GetAttribute("value").ToString();
                }
                if (xe.GetAttribute("key").ToString() == "CloudAPI")
                {
                    Setting.isSupportCloudApi = Convert.ToBoolean(xe.GetAttribute("value"));
                }
            }

        }

        public string getDepth()
        {
            return Setting.level;
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
