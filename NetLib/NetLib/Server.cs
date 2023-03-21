using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;


namespace NetLib
{
    namespace Server
    {
        public struct ClientRepresentation //probably not needed
        {
            public IPEndPoint address;

        }

        public class Server
        {
            private Thread _serverRunThread;
            private bool _serverRunning = false;
            private Connection connection;
            public Server(int port) 
            { 
                connection = new Connection(port);
                _serverRunThread = new Thread(new ThreadStart(_serverRunLoop));
            }

            public void Start()
            {
                _serverRunning = true;

                connection.Start();
                _serverRunThread.Start();

                Console.WriteLine($"[Server] Successfully started! Listening for messages...");
            }

            public void Tick()
            {

            }

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
                        Thread.Sleep(1); //sleepy time
                    }
                }

            }
        }
    }
}