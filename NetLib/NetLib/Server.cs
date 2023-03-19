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
        public struct ClientRepresentation
        {
            public IPEndPoint address;

        }

        public class Server
        {

            private Connection connection;

            public Server(int port) 
            { 
                connection = new Connection(port, true);
            }

            public void Start()
            {
                connection.Start();
            }

            public void Tick()
            {

            }
        }
    }
}