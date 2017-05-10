using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Text;


namespace Fleck.aiplay
{    
    class Server
    {
       

        static void Main()
        {
            Comm comm = new Comm();
            comm.Start();
            
            FleckLog.Level = LogLevel.Debug;
            var allSockets = new List<IWebSocketConnection>();
            var allRoles = new List<Role>();
            var server = new WebSocketServer("ws://0.0.0.0:"+Setting.port);

            

            server.Start(socket =>
                {
                    socket.OnOpen = () =>
                        {
                            var role = new Role();
                            role.connection = socket;
                            role.currentTime = System.DateTime.Now;
                            allRoles.Add(role);
                            allSockets.Add(socket);
                            Console.WriteLine(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " Connected!");
                            comm.WriteInfo(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " Connected!");
                        };
                    socket.OnClose = () =>
                        {
                            foreach (var r in allRoles.ToList())
                            {
                                if (r.connection == socket)
                                {
                                    allRoles.Remove(r);  
                                }
                            }

                            Console.WriteLine(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " Close!");
                            comm.WriteInfo(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " Close!");

                            allSockets.Remove(socket);                                
   
                        };
                    socket.OnMessage = message =>
                        {
                            string[] sArray = message.Split(' ');
                            if (sArray[0] == "listmessage" && sArray[1] == "at" && sArray.Length == 3)
                            {
                                int seek = Int32.Parse(sArray[2]);
                                if (seek < allRoles.Count)
                                {
                                    var r = allRoles[seek];
                                    var connection = r.connection;
                                    socket.Send(connection.ConnectionInfo.ClientIpAddress + ":" + connection.ConnectionInfo.ClientPort.ToString() + " " + r.currentTime.ToString() + " open");
                                    foreach (var m in r.message.ToList())
                                    {
                                        socket.Send(connection.ConnectionInfo.ClientIpAddress + ":" + connection.ConnectionInfo.ClientPort.ToString() + " " + m);
                                    }
                                }
                            }
                            if (message == "listallmessage")
                            {
                                foreach (var r in allRoles.ToList())
                                {
                                    var connection = r.connection;
                                    socket.Send(connection.ConnectionInfo.ClientIpAddress + ":" + connection.ConnectionInfo.ClientPort.ToString() + " " + r.currentTime.ToString() + " open");
                                    foreach (var m in r.message.ToList())
                                    {
                                        socket.Send(connection.ConnectionInfo.ClientIpAddress + ":" + connection.ConnectionInfo.ClientPort.ToString() + " " + m);
                                    }                              
                                }
                                return;                                
                            }

                            int index = allSockets.IndexOf(socket);
                            var role = allRoles[index];
                            role.message.Add(DateTime.Now.ToString()+" "+message);
                            
                            //comm.WriteInfo(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " " + message);        

                            if (message.IndexOf("queryall") != -1)
                            {
                               // Console.WriteLine(comm.QueryallFromCloud(message));
                                socket.Send(comm.QueryallFromCloud(message));
                                return;                                
                            }
                            if (message == "HeartBeat")
                            {
                                return;
                            }  
                            if (message == "count")
                            {
                                socket.Send("There are " + allSockets.Count + " clients online.");
                            }  
                            if (message == "list")
                            {
                                int no = 1;
                                allSockets.ToList().ForEach(
                                    s => socket.Send((no++) + ": " + s.ConnectionInfo.ClientIpAddress + ":" + s.ConnectionInfo.ClientPort.ToString())
                                    );
                                socket.Send("There are " + allSockets.Count + " clients online.");
                            }
                            if (message == "msgcount")
                            {
                                socket.Send("There are " + comm.getMsgQueueCount() + " messages haven't deal.");
                            }
                            if (message == "dealspeed")
                            {
                                socket.Send("The deal speed is " + comm.getDealspeed() + " peer minute.");
                            }
                            if (message == "timeout")
                            {
                                socket.Send("The thinktimeout is " + comm.getThinktimeout() + " second.");
                            } 
                            if (message == "depth")
                            {
                                 socket.Send(comm.getDepth());                               
                            }
                            if (message == "cloudapi")
                            {
                                if (comm.getSupportCloudApi())   socket.Send("true");
                                else socket.Send("false");                              
                            }
                            if (message == "reload")
                            {
                                comm.LoadXml();
                            }
                            if (message == "reset")
                            {
                                comm.resetEngine();
                            } 
                            //过滤命令
                            if (message.IndexOf("position") == -1)
                            {
                               // socket.Send(message);
                                Console.WriteLine(message);
                                return;
                            }                            
                            
                            List<string> list = comm.GetAllItemsFromList(message);
                            if (list.Count >= Int32.Parse(Setting.level))
                            {
                                comm.WriteInfo(message);
                                comm.WriteInfo("getItemFromList");
                                Console.WriteLine("getFromList");
                                string strmsg;
                                for (int i = 0; i < Int32.Parse(Setting.level); i++)
                                {
                                    strmsg = list[i];
                                    socket.Send(strmsg);
                                    comm.WriteInfo(strmsg);
                                }
                                return;
                            }

                            Msg msg = new Msg();
                            msg.connection = socket;
                            msg.message = message;
                            comm.OnMessage(msg);
                            Console.WriteLine("There are " + comm.getMsgQueueCount() + " messages haven't deal.");
                            return;
                        };
                });


            var input = Console.ReadLine();
            while (input != "exit")
            {
                foreach (var socket in allSockets.ToList())
                {
                    socket.Send(input);
                }
                input = Console.ReadLine();
            }
        }
    }  
}
