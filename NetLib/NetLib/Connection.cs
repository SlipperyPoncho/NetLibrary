using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Text;


//TODO: fix all nullable underlines cause they piss me off
// properly handle disconnections (that would probably work through some sort of disconnectpacket + heartbeats
// something else... i forgor 
namespace NetLib
{
    public class ClientInfo {
        public TcpClient? clientSocket;

        public long sequence = 0;
        public float rtt = 0;
        public bool isAlive = true;
    }


    public struct NetMessage {
        public Packet packet { get; set; }
        //public string message { get; set; }
        public DateTime RecieveTime { get; set; }
    }

    public class Connection
    {
        private ConcurrentQueue<NetMessage> q_incomingMessages = new();
        public ConcurrentDictionary<IPEndPoint, ClientInfo> activeClients = new();

        private Thread _networkTcpistener;
        private Thread _networkUdpListener;

        private TcpListener tcpListener;
        private UdpClient udpClient;
        
        private bool _connectionRunning = false;
        private int port;

        public Connection(int port = 0) {
            this.port = port;
            udpClient = new(port);

            tcpListener = new(IPAddress.Loopback, port);

            _networkTcpistener = new Thread(new ThreadStart(_networkTcpReceive));
            _networkUdpListener = new Thread(new ThreadStart(_networkUdpReceive));
        }
        
        public void Start() {
            _connectionRunning = true;

            tcpListener.Start();
            _networkUdpListener.Start();
            _networkTcpistener.Start();

        }
        
        public void Close() {
            _connectionRunning = false;
            tcpListener.Stop();
        }

        public bool Available() {
            return !q_incomingMessages.IsEmpty;
        }

        public NetMessage GetMessage() {
            q_incomingMessages.TryDequeue(out NetMessage result);
            return result;
        }

        public void Connect(IPEndPoint receiver) {
            TcpClient client = new(receiver.Address.ToString(), receiver.Port);
            TestPacket tp = new TestPacket("Hello!");
            NetworkStream stream = client.GetStream();
            stream.Write(tp.GetRaw());

            registerActiveClient(client.Client.RemoteEndPoint as IPEndPoint, client);
        }

        public void SendTCP(IPEndPoint receiver, Packet packet) { // i have no idea with this networkstream ting if i should read first to clear the stream... probably a TODO thing
            byte[] data = packet.GetRaw();

            if (!activeClients.TryGetValue(receiver, out ClientInfo? client)) return; //get client
            if (client == null) return;

            NetworkStream stream = client.clientSocket.GetStream();
            stream.Write(data, 0, data.Length);
        }

        public void SendUDP(IPEndPoint receiver, Packet packet) { 
            byte[] data = packet.GetRaw();
            udpClient.Send(data, receiver);
        }

        public void registerActiveClient(IPEndPoint endPoint, TcpClient client) {
            if (endPoint == null) return;
            activeClients.TryAdd(endPoint, new ClientInfo() { clientSocket = client} );
        }


        //-----------------------------------------separate threads

        private void _networkTcpReceive() {
            if (!_connectionRunning) return;

            while (_connectionRunning) {

                if (tcpListener.Pending()) { //check for incoming connections
                    TcpClient client = tcpListener.AcceptTcpClient(); //blocking 
                    Console.WriteLine($"New client connected: {client.Client.RemoteEndPoint}"); //TODO: some sort of client checking so that we dont accept random connections
                    if (client.Client.RemoteEndPoint != null)
                        activeClients.TryAdd(client.Client.RemoteEndPoint as IPEndPoint, new ClientInfo { clientSocket = client }); //accept connection and add to active connections

                    //might want to do smth here like send a greeting message or whatever
                    //or probably should read the connectPacket uuhhhh idk
                    SendTCP(client.Client.RemoteEndPoint as IPEndPoint, new TestPacket("oh, hi!!"));
                }

                Parallel.ForEach(activeClients, (KeyValuePair<IPEndPoint, ClientInfo> client) => {  //read all incoming messages (this is probably wrong i have no idea)
                    NetworkStream stream = client.Value.clientSocket.GetStream();
                    if (stream.DataAvailable) {
                        while (stream.DataAvailable) {
                            Console.WriteLine("[TCP] Received message, reading...");
                            Packet? packet = PacketReader.ReadFromStream(stream);
                            if(packet != null) 
                                q_incomingMessages.Enqueue(new NetMessage { packet = packet, RecieveTime = DateTime.Now });
                        }
                    }
                });

            }
        }

        //separate thread
        private void _networkUdpReceive() {
            if (!_connectionRunning) return;

            IPEndPoint connection = new IPEndPoint(IPAddress.Any, 0);

            while (_connectionRunning) {
                if(udpClient.Available > 0) { //UDP Listen
                    Console.WriteLine("[UDP] Received message, reading...");
                    byte[] data = udpClient.Receive(ref connection);
                    Packet? packet = PacketReader.ReadFromRaw(data);
                    if(packet != null) 
                        q_incomingMessages.Enqueue(new NetMessage { packet = packet, RecieveTime = DateTime.Now });
                }
                else {
                    Thread.Sleep(1); //nothing to do => sleepy time 
                }
            }
        
        }

    }
}
