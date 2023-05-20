using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Text;
using NetLib_NETStandart.Packets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


//TODO: fix all nullable underlines cause they piss me off
// properly handle disconnections (that would probably work through some sort of disconnectpacket + heartbeats
namespace NetLib_NETStandart {

    public class ConnectionEventArgs : EventArgs {
        public uint new_client_id { get; set; }
    }

    public class ClientInfo {
        public TcpClient? clientTCP;
        public IPEndPoint? clientUDPEndPoint;
        public long sequence = 0;
        public float rtt = 0;
        public bool isAlive = true;
    }


    public struct NetMessage {
        public Packet packet { get; set; }
        public DateTime RecieveTime { get; set; }
    }

    public class Connection
    {
        private ConcurrentQueue<NetMessage> q_incomingMessages = new ConcurrentQueue<NetMessage>();
        //public ConcurrentDictionary<IPEndPoint, ClientInfo> activeClients = new();
        public ConcurrentDictionary<uint, ClientInfo> activeClients = new ConcurrentDictionary<uint, ClientInfo>();
        private uint max_connections = 100;
        private Queue<uint> availableIds;

        private uint connection_key = 0;
        public void SetConnectionKey(uint key) { this.connection_key = key; }

        private CancellationTokenSource t_cts = new CancellationTokenSource();
        private CancellationToken t_ct;
        private Task t_networkTcpListener;
        private Task t_networkUdpListener;

        private TcpListener tcpListener;
        private UdpClient udpListener;
        //private UdpClient udpClient;
        
        private bool _connectionRunning = false;
        private int port;

        public event EventHandler<ConnectionEventArgs> onNewConnection;
        public Connection(int port = 0) {
            this.port = port;

            tcpListener = new TcpListener(IPAddress.Any, port);
            //if (port != 0) port+=2;
            udpListener = new UdpClient(port);

            t_ct = t_cts.Token;
            t_networkTcpListener = new Task(_networkTcpReceive, t_ct);
            t_networkUdpListener = new Task(_networkUdpReceive, t_ct);

            availableIds = new Queue<uint>();
            for (uint i = 2; i <= max_connections; i++) availableIds.Enqueue(i); // 1 is the server i guess
        }

        ~Connection() {
            udpListener.Close();
        }
        
        public void Start() {
            _connectionRunning = true;

            tcpListener.Start();
            t_networkTcpListener.Start();
            t_networkUdpListener.Start();

            

            Console.WriteLine($"[Connection] Started connection: \n" +
                $"Local address:\n" +
                $"  [TCP] - {tcpListener.LocalEndpoint}\n" +
                $"  [UDP] - {udpListener.Client.LocalEndPoint}(listener)\n");

        }
        
        public async void Close() { //TODO: send a disconnect packet
            _connectionRunning = false;

            t_cts.Cancel();
            await t_networkTcpListener;
            await t_networkUdpListener;
            t_networkTcpListener.Dispose();
            t_networkUdpListener.Dispose();

            tcpListener.Stop();
            //udpClient.Close();

            activeClients.Clear();
        }

        private uint GetAvailableClientId() {
            if (availableIds.Count > 0)
                return availableIds.Dequeue();
            Console.WriteLine("Ran out of client id's");
            return 0;
        }

        public bool Available() {
            return !q_incomingMessages.IsEmpty;
        }

        public NetMessage GetMessage() {
            q_incomingMessages.TryDequeue(out NetMessage result);
            return result;
        }

        public void ConnectToServer(IPEndPoint receiver) {
            TcpClient client = new TcpClient(receiver.Address.ToString(), receiver.Port);
            //IPEndPoint serverUDP = new IPEndPoint(receiver.Address, receiver.Port + 2);
            activeClients.TryAdd(1, new ClientInfo() { clientTCP = client, clientUDPEndPoint = receiver });
            SendTCP(1, new ConnectPacket(((IPEndPoint)udpListener.Client.LocalEndPoint!).Port));
        }

        public void SendTCP(uint receiver, Packet packet) { // i have no idea with this networkstream ting if i should read first to clear the stream... probably a TODO thing
            Console.WriteLine($"[TCP] Sending message to {receiver}");
            packet.Sender = connection_key;
            byte[] data = packet.GetRaw();

            if (!activeClients.TryGetValue(receiver, out ClientInfo? client)) return; //get client
            if (client == null) return;

            NetworkStream stream = client.clientTCP.GetStream();
            stream.Write(data, 0, data.Length);
        }

        public void SendUDP(uint receiver, Packet packet) {
            packet.Sender = connection_key;
            byte[] data = packet.GetRaw();
            activeClients.TryGetValue(receiver, out ClientInfo client);
            Console.WriteLine($"[UDP] Sending message to {receiver}");
            udpListener.Send(data, data.Length, client.clientUDPEndPoint);
        }

        public void SendToAll(Packet packet, bool useTcp = false) {
            Console.WriteLine("[Connection] Starting parallel foreach...");
            Parallel.ForEach(activeClients, (KeyValuePair<uint, ClientInfo> client) => {
                if (useTcp) SendTCP(client.Key, packet);
                else SendUDP(client.Key, packet);
            });
        }

        public void registerActiveClient(IPEndPoint endPoint, TcpClient client) { ////client side ONLY
            if (endPoint == null) return;
            uint newId = GetAvailableClientId();
            activeClients.TryAdd(newId, new ClientInfo() { clientTCP = client, clientUDPEndPoint = endPoint });
            //activeClients.TryAdd(endPoint, new ClientInfo() { clientTCP = client, clientUDPEndPoint = endPoint} );
        }


        private bool handle_internal_packets(uint sender, Packet packet) {
            switch (packet.PacketType) {
                case PacketType.ConnectPacket:
                    Console.WriteLine("[Connection] Got connectpacket, bruh");
                    if (!activeClients.TryGetValue(sender, out ClientInfo? client)) return true; // what
                    if(client == null) return true;                                              // also what

                    ConnectPacket cp = (ConnectPacket)packet;
                    client.clientUDPEndPoint = new IPEndPoint((client.clientTCP.Client.RemoteEndPoint as IPEndPoint).Address, cp.UdpPort);

                    ConnectAckPacket cap = new ConnectAckPacket(sender);
                    SendTCP(sender, cap);

                    onNewConnection?.Invoke(this, new ConnectionEventArgs { new_client_id = sender });

                    return true;

                case PacketType.ConnectAckPacket:
                    ConnectAckPacket cap_recv = (ConnectAckPacket)packet;
                    this.connection_key = cap_recv.Key;
                    Console.WriteLine($"[Connection] НОВЫЙ ГОД получили в подарок ключ!!: {cap_recv.Key}");
                    return true;

                case PacketType.HeartbeatPacket:
                    HeartbeatPacket hp = (HeartbeatPacket)packet;
                    HeartbeatAckPacket hap = new HeartbeatAckPacket(hp.TimeStamp);
                    SendUDP(sender, hap);
                    return true;

                case PacketType.HeartbeatAckPacket:
                    HeartbeatAckPacket hap_recv = (HeartbeatAckPacket)packet;
                    Console.WriteLine($"[Connection] heartbeat: {(DateTime.Now - hap_recv.TimeStamp).Milliseconds}ms");
                    return true;

                case PacketType.DisconnectPacket:
                    activeClients.TryRemove(sender, out _);
                    availableIds.Enqueue(sender);
                    Console.WriteLine($"[Connection] Client {sender} disconnected!");
                    return true;

                default: return false;
            }
        }

        //-----------------------------------------separate threads
        private void _networkTcpReceive() {
            if (!_connectionRunning) return;

            while (_connectionRunning && !t_ct.IsCancellationRequested) {

                if (tcpListener.Pending()) { //check for incoming connections
                    TcpClient client = tcpListener.AcceptTcpClient(); //blocking 
                    Console.WriteLine($"New client connected: {client.Client.RemoteEndPoint}"); //TODO: some sort of client checking so that we dont accept random connections

                    uint newId = 0;
                    if (client.Client.RemoteEndPoint != null) {
                        newId = GetAvailableClientId();
                        activeClients.TryAdd(newId, new ClientInfo { clientTCP = client }); //accept connection and add to active connections
                    }

                    //might want to do smth here like send a greeting message or whatever
                    //or probably should read the connectPacket uuhhhh idk
                    SendTCP(newId, new TestPacket("oh, hi!!"));
                }

                Parallel.ForEach(activeClients, (KeyValuePair<uint, ClientInfo> client) => {  //read all incoming messages (this is probably wrong i have no idea)
                    NetworkStream stream = client.Value.clientTCP.GetStream();
                    if (stream.DataAvailable) {
                        while (stream.DataAvailable) {
                            Console.WriteLine($"[TCP] Received message from {client.Key}, reading...");
                            Packet? packet = PacketReader.ReadFromStream(stream);
                            Console.WriteLine($"{packet!.PacketType}");
                            if(packet != null) 
                                if(!handle_internal_packets(client.Key, packet))
                                    q_incomingMessages.Enqueue(new NetMessage { packet = packet, RecieveTime = DateTime.Now });
                        }
                    }
                });

            }

            t_ct.ThrowIfCancellationRequested();
        }


        private void _networkUdpReceive() { 
            if (!_connectionRunning) return;
        
            IPEndPoint connection = new IPEndPoint(IPAddress.Any, 0);
        
            while (_connectionRunning && !t_ct.IsCancellationRequested) {
                if(udpListener.Available > 0) { //UDP Listen
                    byte[] data = udpListener.Receive(ref connection);
                    Packet? packet = PacketReader.ReadFromRaw(data);
                    uint sender = packet.Sender;
                    Console.WriteLine($"[UDP] Received message from {sender}, reading...");
                    if (packet != null)
                        if (!handle_internal_packets(sender, packet))
                            q_incomingMessages.Enqueue(new NetMessage { packet = packet, RecieveTime = DateTime.Now });
                }
                else {
                    Task.Delay(1); //nothing to do => sleepy time 
                }
            }
        
            t_ct.ThrowIfCancellationRequested();
        }

    }
}
