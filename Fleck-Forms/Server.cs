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
        public void Start()
        {
            Engine comm = new Engine();
            comm.Start();
            
            FleckLog.Level = LogLevel.Debug;            
            var server = new WebSocketServer("ws://0.0.0.0:"+Setting.port);

            server.Start(socket =>
                {
                    socket.OnOpen = () =>
                        {
                            comm.OnOpen(socket);
                        };
                    socket.OnClose = () =>
                        {
                            comm.OnClose(socket);
                        };
                    socket.OnMessage = message =>
                        {
                            comm.OnMessage(socket, message);
                        };
                });

            var input = Console.ReadLine();
            while (input != "exit")
            {
//                 foreach (var socket in allSockets.ToList())
//                 {
//                     socket.Send(input);
//                 }
                input = Console.ReadLine();
            }
        }
    }  
}
