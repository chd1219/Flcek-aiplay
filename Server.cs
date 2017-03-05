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
            var server = new WebSocketServer("ws://0.0.0.0:"+Setting.port);     
            
            server.Start(socket =>
                {
                    socket.OnOpen = () =>
                        {
                            allSockets.Add(socket);
                            Console.WriteLine(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " Connected!");
                            comm.WriteInfo(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " Connected!");
                        };
                    socket.OnClose = () =>
                        {
                            Console.WriteLine(socket.ConnectionInfo.ClientIpAddress + ":"+ socket.ConnectionInfo.ClientPort.ToString() + " Close!");
                            comm.WriteInfo(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " Close!");
                            allSockets.Remove(socket);
                        };
                    socket.OnMessage = message =>
                        {
                            comm.WriteInfo(socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort.ToString() + " " + message);        

                            if (message.IndexOf("queryall") != -1)
                            {
                                socket.Send(comm.QueryallFromCloud(message));
                                return;
                                //Console.WriteLine(comm.QueryallFromCloud(message));
                            }

                            if (message == "list")
                            {
                                int no = 1;
                                allSockets.ToList().ForEach(
                                    s => socket.Send((no++) + ": " + s.ConnectionInfo.ClientIpAddress + ":" + s.ConnectionInfo.ClientPort.ToString())
                                    );
                                socket.Send("There are " + allSockets.Count + " clients online.");
                            }
                            if (message == "count")
                            {
                                socket.Send("There are " + allSockets.Count + " clients online.");
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
                                comm.ReloadXml();
                            } 
                            //过滤命令
                            if (message.IndexOf("position") == -1)
                            {
                               // socket.Send(message);
                                Console.WriteLine(message);
                                return;
                            }
                                 
                            Msg msg = new Msg();
                            msg.connection = socket;
                            msg.message = message;

                            comm.OnMessage(msg);

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
