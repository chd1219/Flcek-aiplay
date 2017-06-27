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
using Newtonsoft.Json;

namespace Fleck.aiplay
{    
    class Comm
    {
        public Log log;
        public Log logPosition;
        public Setting setting;
        public Queue DealSpeedQueue;
        public RedisHelper redis;
        public User user;
        private static int nMsgQueuecount { get; set; }

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
     
        public int getActiveCount()
        {
            int no = 0;
            foreach (Role r in user.allRoles)
            {
                if (r.isActive())
                {
                    no++;
                }
            }
            return no;
        }
       
        public int getDealspeed()
        {
            return nMsgQueuecount;
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

        public void LoadXml()
        {
            setting.LoadXml();
        }

        public int getUserCount()
        {
            return user.allSockets.Count;
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

        public void Init()
        {
            setting = new Setting();
            log = new Log();
            logPosition = new Log("Position");
            DealSpeedQueue = new Queue();                
            user = new User();
            redis = new RedisHelper();
            //统计在线用户的提交量
            StartStatThread(); 
        }
        
        public void DealActiveMessage(IWebSocketConnection socket)
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

        public void DealListMessage(IWebSocketConnection socket)
        {
            int no = 0;
            user.allRoles.ToList().ForEach(
                r => socket.Send((no++)+1 + ": " + r.ToString())
                );
            socket.Send("There are " + no + " clients online.");
        }

        public string DealQueryallMessage(string message)
        {
            string str = "";
            if (Setting.isSupportCloudApi)
            {
                str = redis.QueryallFromCloud(message);
            }
            return str;
        }
            
        public void getFromList(Role role, string message)
        {            
            List<string> list = redis.GetAllItemsFromList(message);
            string strmsg = "";
            int nlevel = Int32.Parse(Setting.level);
            if (list.Count >= nlevel)
            {
                //过滤空消息
                for (int i = 0; i < nlevel; i++)
                {
                    if (list[i].Length > 0)
                    {
                        strmsg = list[i];
                        role.Send(strmsg);
                    }
                }
                if (strmsg.Length > 0)
                {
                    string[] infoArray = strmsg.Split(' ');
                    for (int j = 0; j < infoArray.Length; j++)
                    {
                        if (infoArray[j] == "pv")
                        {
                            role.Done("bestmove " + infoArray[j + 1]);
                            Console.WriteLine("depth " + infoArray[2] + " bestmove " + infoArray[j + 1]);
                            return;
                        }
                    }
                }
            }
        }

        public void getFromList(string message)
        {
            Console.Write("0");
            List<string> list = redis.GetAllItemsFromList(message);
            string strmsg = "";
            int nlevel = Int32.Parse(Setting.level);
            if (list.Count >= nlevel)
            {
                //过滤空消息
                for (int i = 0; i < nlevel; i++)
                {
                    if (list[i].Length > 0)
                    {
                        strmsg = list[i];
                    }
                }
                if (strmsg.Length > 0)
                {
                    string[] infoArray = strmsg.Split(' ');
                    for (int j = 0; j < infoArray.Length; j++)
                    {
                        if (infoArray[j] == "pv")
                        {
                            //Console.WriteLine("depth " + infoArray[2] + " bestmove " + infoArray[j + 1]);
                            return;
                        }
                    }
                }
            }
        }

        public void StartStatThread()
        {
            Thread statThread = new Thread(new ThreadStart(StatThread));
            statThread.IsBackground = true;
            statThread.Start();
            nMsgQueuecount = 0;        
        }

        public Msg ParseJson(string jsonStr)
        {
            Msg msg;
            JavaScriptObject jsonObj = JavaScriptConvert.DeserializeObject<JavaScriptObject>(jsonStr);
            msg = new Msg(jsonObj["id"].ToString(), jsonObj["fen"].ToString());
            return msg;
        }

        public void ReadFile(string path)
        {
            StreamReader sr = new StreamReader(path, Encoding.Default);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                Console.WriteLine(line.ToString());
            }
        }
    }
}
