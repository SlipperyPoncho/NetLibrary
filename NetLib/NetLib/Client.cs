using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetLib
{
    public class Client
    {

        private Connection connection;
        private IPEndPoint serverEndPoint;

        public Client(IPEndPoint serverEndPoint, int port) 
        {
            this.serverEndPoint = serverEndPoint;
            connection = new(port);
            connection.Start();
        }

        public void Start()
        {
            connection.Start();
        }

        public void Tick()
        {

        }

        public void Send(string msg)
        {
            connection.Send(serverEndPoint, msg);
        }
    }
}
