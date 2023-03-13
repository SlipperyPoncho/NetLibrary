using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Text;

namespace NetLib
{

    public class NetMessage {

        //public Packet packet { get; set; }
        public string message { get; set; }
        public DateTime RecieveTime { get; set; }
    }

    public class Connection
    {
        private Thread _networkListener;

        private UdpClient client;  

        private ConcurrentQueue<NetMessage> q_incomingMessages = new();

        private bool _connectionRunning = false;

        private int port;

       public Connection(int port) {
            client = new(port);
            this.port = port;
       }
       
       public void Start() {
           _connectionRunning = true;
           _networkListener = new Thread(new ThreadStart(_networkRun));
           _networkListener.Start();
       }
       
       public void Close() {
           _connectionRunning = false;
       }
       
       public void Send(IPEndPoint receiver, string msg) {
           byte[] sendbuff = Encoding.ASCII.GetBytes(msg);
           client.Send(sendbuff, receiver);
           Console.WriteLine("Sending message " + sendbuff.Length);
       }
       
       //separate thread
       private void _networkRun() {
           if (!_connectionRunning) return;
           IPEndPoint connection = new(IPAddress.Any, port);
           Console.WriteLine("Connection network listener started...");
       
           while (_connectionRunning) {
               bool canRead = client.Available > 0;
               if (canRead) {
                   byte[] data = client.Receive(ref connection);
                   Console.WriteLine($"Connection received broadcast from {connection}");
                    Console.WriteLine($"Message is as follows: {Encoding.ASCII.GetString(data, 0, data.Length)}");
                   q_incomingMessages.Enqueue(new NetMessage {
                       message = Encoding.ASCII.GetString(data, 0, data.Length),
                       RecieveTime = DateTime.Now
                   });
               }
           }
       
       }

    }
}
