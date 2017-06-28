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
using System.Data.SQLite;

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

        public Msg Json2Msg(string jsonStr)
        {           
            try
            {
                Msg msg;
                JavaScriptObject jsonObj = JavaScriptConvert.DeserializeObject<JavaScriptObject>(jsonStr);
                msg = new Msg(jsonObj["id"].ToString(), jsonObj["fen"].ToString());
                return msg;
            }
            catch (System.Exception ex)
            {
                return null;
            }           
        }

        public void SQLite_Test()
        {
            SQLiteConnection conn = null;
            string strSQLiteDB = Environment.CurrentDirectory;
            strSQLiteDB = strSQLiteDB.Substring(0, strSQLiteDB.LastIndexOf("\\"));
            strSQLiteDB = strSQLiteDB.Substring(0, strSQLiteDB.LastIndexOf("\\"));// 这里获取到了Bin目录  

            try
            {
                string dbPath = "Data Source=" + strSQLiteDB + "\\test.db";
                conn = new SQLiteConnection(dbPath);//创建数据库实例，指定文件位置    
                conn.Open();                        //打开数据库，若文件不存在会自动创建    

                string sql = "CREATE TABLE IF NOT EXISTS phone(ID integer, brand varchar(20), Memery varchar(50));";//建表语句    
                SQLiteCommand cmdCreateTable = new SQLiteCommand(sql, conn);
                cmdCreateTable.ExecuteNonQuery();//如果表不存在，创建数据表    

                SQLiteCommand cmdInsert = new SQLiteCommand(conn);
                cmdInsert.CommandText = "INSERT INTO phone(brand, Memery) VALUES('samsung', '三星')";//插入几条数据    
                cmdInsert.ExecuteNonQuery();
                cmdInsert.CommandText = "INSERT INTO phone(brand, Memery) VALUES('samsung', '三星')";//插入几条数据    
                cmdInsert.ExecuteNonQuery();
                cmdInsert.CommandText = "INSERT INTO phone(brand, Memery) VALUES('samsung', '三星')";//插入几条数据    
                cmdInsert.ExecuteNonQuery();

                conn.Close();
            }
            catch (Exception ex)
            {
            }
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
