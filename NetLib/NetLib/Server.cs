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
                connection = new Connection(port);
            }

            public void Start()
            {
                connection.Start();
            }

            public void Tick()
            {

            }

            public void Send()
            {

            }
        }
    }
}





/*
 * UdpClient listener = new(port);
                IPEndPoint clientEP = new(IPAddress.Any, port);

                try
                {
                    while (true)
                    {
                        Console.WriteLine("Waiting for broadcast...");
                        byte[] bytes = listener.Receive(ref clientEP);



                        Console.WriteLine($"Received broadcast from {clientEP} :");
                        Console.WriteLine($" {Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");
                    }
                } catch (SocketException e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    listener.Close();
                } */