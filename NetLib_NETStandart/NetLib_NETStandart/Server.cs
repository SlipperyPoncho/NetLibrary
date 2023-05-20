using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NetLib_NETStandart.Packets;

// GET incoming partial packets
namespace NetLib_NETStandart {
    namespace Server {
        public class ServerEventArgs : EventArgs {
            public uint new_client_id { get; set; }
        }
        public class Server
        {
            //private Thread _serverRunThread;
            private Task t_serverRunTask;
            public bool Running { get => _serverRunning; }
            private bool _serverRunning = false;
            public Connection connection;

            public ConcurrentQueue<NetMessage> q_incomingMessages = new ConcurrentQueue<NetMessage>();

            public event EventHandler<ServerEventArgs> onNewConnection;

            public Server(int port) 
            { 
                connection = new Connection(port);
                connection.SetConnectionKey(1);
                connection.onNewConnection += NewClientConnected;
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
            
            public void NewClientConnected(object sender, ConnectionEventArgs args) {
                onNewConnection?.Invoke(this, new ServerEventArgs { new_client_id = args.new_client_id });
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
                                default:
                                    q_incomingMessages.Enqueue(msg);
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