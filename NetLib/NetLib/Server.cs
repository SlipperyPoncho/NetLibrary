using NetLib.Packets;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;


namespace NetLib {
    namespace Server {
        public struct ClientRepresentation //probably not needed
        {
            public IPEndPoint address;

        }

        public class Server
        {
            //private Thread _serverRunThread;
            private Task t_serverRunTask;
            public bool Running { get => _serverRunning; }
            private bool _serverRunning = false;
            private Connection connection;
            public Server(int port) 
            { 
                connection = new Connection(port);
                connection.SetConnectionKey(1);
                t_serverRunTask = new Task(_serverRunLoop);
            }

            public void Start()
            {
                _serverRunning = true;

                connection.Start();
                t_serverRunTask.Start();

                Console.WriteLine($"[Server] Successfully started! Listening for messages...");
            }

            public void SendString_All(string msg) {
                Console.WriteLine("[Server] start send all...");
                connection.SendToAll(new TestPacket(msg));
            }

            public void SendHeartbeat_All(DateTime time)
            {
                Console.WriteLine("[Server] start send all...");
                connection.SendToAll(new HeartbeatPacket(time));
            }

            public void Tick()
            {

            }

            //------------------------separate task
            private void _serverRunLoop() {
                if (!_serverRunning) return;

                while (_serverRunning) {
                    //read all incoming messages from connection
                    if (connection.Available()) {
                        while (connection.Available()) {
                            NetMessage msg = connection.GetMessage();

                            switch (msg.packet.PacketType) {
                                case PacketType.TestPacket:
                                    TestPacket tp = (TestPacket)msg.packet;
                                    Console.WriteLine($"[Server] Received TestPacket: \"{tp.Text}\"");
                                    break;
                            }

                        }
                    }
                    else {
                        Task.Delay(1); //sleepy time
                    }
                }

            }
        }
    }
}