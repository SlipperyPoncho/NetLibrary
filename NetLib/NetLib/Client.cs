using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetLib
{
    public class Client
    {
        public Client() { }

        Socket sender;

        public void Start(int port)
        {
            sender = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Tick()
        {

        }

        public void Send(string ip, string mes)
        {
            IPAddress broadcast = IPAddress.Parse(ip);

            byte[] sendbuf = Encoding.ASCII.GetBytes(mes);
            IPEndPoint ep = new(broadcast, 11000);

            sender.SendTo(sendbuf, ep);

            Console.WriteLine("Message sent to the broadcast address");
            Console.ReadLine();
        }
    }
}
