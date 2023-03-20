using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Text;


//TODO: to properly test this shit we need a proper Packet class and a proper static PacketReader class soooo i absolutely cannot guarantee this works xDDDD :)))))
namespace NetLib
{
    public class ClientInfo {
        public TcpClient clientSocket;

        public long sequence = 0;
        public float rtt = 0;
        public bool isAlive = true;
    }


    public struct NetMessage {
        public IPacket packet { get; set; }
        //public string message { get; set; }
        public DateTime RecieveTime { get; set; }
    }

    public class Connection
    {
        public ConcurrentQueue<NetMessage> q_incomingMessages = new();
        public ConcurrentDictionary<IPEndPoint, ClientInfo> activeClients = new();

        private Thread _networkTcpistener;
        private Thread _networkUdpListener;

        private TcpListener tcpListener;
        private UdpClient udpClient;  
        
        private bool _connectionRunning = false;
        private bool isServer = false;
        private int port;

        public Connection(int port, bool shouldListenForConnections = false) {
            this.port = port;
            udpClient = new(port);

            if (shouldListenForConnections) {
                tcpListener = new(IPAddress.Loopback, port);
                isServer = true;

                _networkTcpistener = new Thread(new ThreadStart(_networkTcpReceive));
            }

            _networkUdpListener = new Thread(new ThreadStart(_networkUdpReceive));
        }
        
        public void Start() {
            _connectionRunning = true;

            _networkUdpListener.Start();

            if (isServer) {
                tcpListener.Start();
                _networkTcpistener.Start();
            }
        }
        
        public void Close() {
            _connectionRunning = false;
        }

        public void SendTCP(IPEndPoint receiver, IPacket packet) { // i have no idea with this networkstream ting if i should read first to clear the stream... probably a TODO thing
            byte[] data = packet.getRaw();

            if (!activeClients.TryGetValue(receiver, out ClientInfo? client)) return; //get client
            if (client == null) return;

            using NetworkStream stream = client.clientSocket.GetStream();
            stream.Write(data, 0, data.Length);
        }

        public void SendUDP(IPEndPoint receiver, IPacket packet) { 
            byte[] data = packet.getRaw();
            udpClient.Send(data, receiver);
        }

        public void registerActiveClient(IPEndPoint endPoint) {
            if (endPoint == null) return;
            activeClients.TryAdd(endPoint, new ClientInfo());
        }

        private void _networkTcpReceive() {
            if (!_connectionRunning || !isServer) return;

            while (_connectionRunning) {

                if (tcpListener.Pending()) { //check for incoming connections
                    using TcpClient client = tcpListener.AcceptTcpClient(); //blocking 
                    Console.WriteLine($"New client connected: {client.Client.RemoteEndPoint}"); //TODO: some sort of client checking so that we dont accept random connections
                    if (client.Client.RemoteEndPoint != null)
                        activeClients.TryAdd(client.Client.RemoteEndPoint as IPEndPoint, new ClientInfo { clientSocket = client }); //accept connection and add to active connections

                    //might want to do smth here like send a greeting message or whatever
                    //or probably should read the connectPacket uuhhhh idk
                }

                Parallel.ForEach(activeClients, (KeyValuePair<IPEndPoint, ClientInfo> client) => {  //read all incoming messages (this is probably wrong i have no idea)
                    NetworkStream stream = client.Value.clientSocket.GetStream();
                    if (stream.DataAvailable) {
                        while (stream.DataAvailable) {
                            IPacket packet = PacketReader.readFromStream(stream);
                            q_incomingMessages.Enqueue(new NetMessage { packet = packet, RecieveTime = DateTime.Now });
                        }
                    }
                });

            }
        }

        //separate thread
        private void _networkUdpReceive() {
            if (!_connectionRunning) return;

            IPEndPoint connection = new IPEndPoint(IPAddress.Any, port);

            while (_connectionRunning) {
                if(udpClient.Available > 0) { //UDP Listen
                    byte[] data = udpClient.Receive(ref connection);
                    IPacket packet = PacketReader.readFromRaw(data);
                    NetMessage message = new NetMessage { packet = packet, RecieveTime = DateTime.Now };
                    q_incomingMessages.Enqueue(message);
                }
                else {
                    Thread.Sleep(1); //nothing to do => sleepy time 
                }
            }
        
        }

    }

}
