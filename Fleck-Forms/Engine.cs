using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using Fleck_Forms;

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
        public Queue OutputEngineQueue;
        // 为保证线程安全，使用一个锁来保护_task的访问
        readonly static object _locker = new object();
        // 通过 _wh 给工作线程发信号
        static EventWaitHandle _wh = new AutoResetEvent(false);
        public bool bJson { get; set; }
        public bool bRedis { get; set; }
        public int getMsgQueueCount()
        {
            if (InputEngineQueue == null)
            {
                return 0;
            }
             return InputEngineQueue.Count;
        }

        public bool CheckInputEngineQueue()
        {
            if((InputEngineQueue != null) && (InputEngineQueue.Count > 0)) 
            {
                return true;
            }
            return false;
        }

        public void OutputEngineQueueEnqueue(string line, bool save = false)
        {
            if (OutputEngineQueue == null || line == null || line.Length == 0)
            {
                return;
            }
            if (save)
            {
                WriteInfo(line);
            }
            lock (OutputEngineQueue)
            {
                OutputEngineQueue.Enqueue(line);
            }
        }

        public string OutputEngineQueueDequeue()
        {
            if (OutputEngineQueue == null)
            {
                return null;
            }
            lock (OutputEngineQueue)
            {
                return (string)OutputEngineQueue.Dequeue();
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
                OutputEngineQueueEnqueue("[error] KillPipeThread " + ex.Message, true);
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
                OutputEngineQueueEnqueue(line); 
                line = reader.ReadLine();
                OutputEngineQueueEnqueue(line);
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
                            OutputEngineQueueEnqueue("depth " + intDepth.ToString() + " " + line);
                            currentRole.Done(line);
                            _wh.Set();  // 给工作线程发信号
                        }
                        Thread.Sleep(10);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _wh.Set();  // 给工作线程发信号
                OutputEngineQueueEnqueue("[error] PipeThread " + ex.Message, true);
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
            //WriteInfo(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " " + message);
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
                            OutputEngineQueueEnqueue(str);
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
                InputEngineQueueEnqueue(role);
            }          
        }

        private void InputEngineQueueEnqueue(Role role)
        {
            try
            {
                lock (_locker)
                {
                    InputEngineQueue.Enqueue(role);
                }
            }
            catch (System.Exception ex)
            {
                OutputEngineQueueEnqueue(ex.Message);
            }
          
        }

        public Role InputEngineQueueDequeue()
        {
            Role role = null;
            try
            {
                lock (_locker)
                {

                    role = InputEngineQueue.Dequeue();
                }
            }
            catch (System.Exception ex)
            {
                OutputEngineQueueEnqueue(ex.Message);
            }            

            return role;
        
        }

        public void CustomerThread()
        {            
            while (true)
            {
                Msg msg = null;
                try
                {
                    if (InputEngineQueue.Count > 0)
                    {
                        currentRole = InputEngineQueueDequeue();
                        msg = currentRole.GetCurrentMsg();
                    }

                    if (msg != null)
                    {
                        EngineDeal(msg.message);  // 任务不为null时，处理并保存数据     
                        _wh.WaitOne();   //等待信号
                    }

                    Thread.Sleep(100);
                }
                catch (System.Exception ex)
                {                    
                    OutputEngineQueueEnqueue("[error] GetFromEngine " + ex.Message, true);
                    InputEngineQueueDequeue();
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
                _wh.Set();  // 给工作线程发信号
            }
            else
            {
                OutputEngineQueue.Enqueue("getFromEngine");
                PipeWriter.Write(message + "\r\n");
                PipeWriter.Write("go depth " + Setting.level + "\r\n");
                Thread.Sleep(50);
            }
        }

//         public int VerifyFEN(string s) 
//         {
// 	        s = s.Replace(/[\r\n]/, '');
// 	        s = s.Replace(/%20/, ' ');
// 	        s = s.Replace(/\+/, ' ');
// 	        s = s.Replace(/ b.*/g, ' b');
// 	        s = s.Replace(/ w.*/g, ' w');
// 	        s = s.Replace(/ r.*/g, ' w');
// 	        if (s.Replace(/\+/) != -1) {
// 		        s = s.Substring(0, s.IndexOf(/\+/));
// 	        }
// 
// 	        var a = new Array();
// 	        var sum = 0;
// 	        var w = new String(s.Substring(s.length - 2, 2));
// 	        w = w.toLowerCase();
// 	        if (w != ' w' && w != ' b') {
// 		        return (0);
// 	        }
// 	        s = s.substr(0, s.length - 2);
// 	        a = String(s).split(/\//);
// 	        if (a.length != 10) {
// 		        return (0);
// 	        }
// 	        for (var x = 0; x < 10; x++) {
// 		        sum = 0;
// 		        if (String(a[x]).search(/[^1-9kabnrcpKABNRCP]/) != -1) {
// 			        return (0);
// 		        }
// 		        a[x] = String(a[x]).replace(/[kabnrcpKABNRCP]/g, '1');
// 		        while (String(a[x]).length != 0) {
// 			        sum = sum + Number(String(a[x]).charAt(0));
// 			        a[x] = String(a[x]).substr(1);
// 		        }
// 		        if (sum != 9) {
// 			        return (0);
// 		        }
// 	        }
// 	        return (1);
//         }

        public void stopEngine()
        {
            bLock = false;
            KillPipeThread();
            OutputEngineQueueEnqueue("引擎停止！");
        }
    }
}
