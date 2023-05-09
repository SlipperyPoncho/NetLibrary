using NetLib.Packets;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetLib {
    public class Client
    {

        private bool _clientRunning = false;
        private Thread _clientRunThread;
        private Connection connection;
        private IPEndPoint serverEndPoint;

        public Client(IPEndPoint serverEndPoint) 
        {
            this.serverEndPoint = serverEndPoint;
            connection = new(0);

            _clientRunThread = new Thread(new ThreadStart(_clientRunLoop));
        }

        public void Start()
        {
            _clientRunning = true;
            connection.Start();
            _clientRunThread.Start();
            Console.WriteLine($"[Client] Successfully started!");
            connection.ConnectToServer(serverEndPoint);
        }

        public void Tick()
        {

        }

        public void SendString(string msg)
        {
            //connection.SendTCP(1, new TestPacket(msg));
            connection.SendUDP(1, new TestPacket(msg));
        }

        public void SendDisconnect(string msg)
        {
            //connection.SendTCP(1, new TestPacket(msg));
            connection.SendUDP(1, new DisconnectPacket(msg));
        }

        private void _clientRunLoop() {
            if (!_clientRunning) return;

            while (_clientRunning) {
                //read all incoming messages from connection
                if (connection.Available()) {
                    while (connection.Available()) {
                        NetMessage msg = connection.GetMessage();

                        switch (msg.packet.PacketType) {
                            case PacketType.TestPacket:
                                TestPacket tp = (TestPacket)msg.packet;
                                Console.WriteLine($"[Client] Received TestPacket: \"{tp.Text}\"");
                                break;
                        }

                    }
                }
                else {
                    Thread.Sleep(1); //sleepy time
                }
            }

        }
    }
}
