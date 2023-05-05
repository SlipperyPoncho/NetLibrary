using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Text;


//TODO: fix all nullable underlines cause they piss me off
// properly handle disconnections (that would probably work through some sort of disconnectpacket + heartbeats
namespace NetLib
{
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
        private ConcurrentQueue<NetMessage> q_incomingMessages = new();
        public ConcurrentDictionary<IPEndPoint, ClientInfo> activeClients = new();

        private CancellationTokenSource t_cts = new();
        private CancellationToken t_ct;
        private Task t_networkTcpListener;
        private Task t_networkUdpListener;

        private TcpListener tcpListener;
        private UdpClient udpListener;
        //private UdpClient udpClient;
        
        private bool _connectionRunning = false;
        private int port;

        public Connection(int port = 0) {
            this.port = port;

            tcpListener = new(IPAddress.Loopback, port);
            udpListener = new(port);

            t_ct = t_cts.Token;
            t_networkTcpListener = new Task(_networkTcpReceive, t_ct);
            t_networkUdpListener = new Task(_networkUdpReceive, t_ct);
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

        public bool Available() {
            return !q_incomingMessages.IsEmpty;
        }

        public NetMessage GetMessage() {
            q_incomingMessages.TryDequeue(out NetMessage result);
            return result;
        }

        public void Connect(IPEndPoint receiver) {
            TcpClient client = new(receiver.Address.ToString(), receiver.Port);
            registerActiveClient(receiver, client);
            SendTCP(receiver, new ConnectPacket(((IPEndPoint)udpListener.Client.LocalEndPoint!).Port));
        }

        public void SendTCP(IPEndPoint receiver, Packet packet) { // i have no idea with this networkstream ting if i should read first to clear the stream... probably a TODO thing
            Console.WriteLine($"[TCP] Sending message to {receiver}");
            byte[] data = packet.GetRaw();

            if (!activeClients.TryGetValue(receiver, out ClientInfo? client)) return; //get client
            if (client == null) return;

            NetworkStream stream = client.clientTCP.GetStream();
            stream.Write(data, 0, data.Length);
        }

        public void SendUDP(IPEndPoint receiver, Packet packet) {
            byte[] data = packet.GetRaw();
            activeClients.TryGetValue(receiver, out ClientInfo client);
            Console.WriteLine($"[UDP] Sending message to {client.clientUDPEndPoint}");
            udpListener.Send(data, client.clientUDPEndPoint);
        }

        public void SendToAll(Packet packet, bool useTcp = false) {
            Console.WriteLine("[Connection] Starting parallel foreach...");
            Parallel.ForEach(activeClients, (KeyValuePair<IPEndPoint, ClientInfo> client) => {
                if (useTcp) SendTCP(client.Key, packet);
                else SendUDP(client.Key, packet);
            });
        }

        public void registerActiveClient(IPEndPoint endPoint, TcpClient client) { ////client side ONLY
            if (endPoint == null) return;
            activeClients.TryAdd(endPoint, new ClientInfo() { clientTCP = client, clientUDPEndPoint = endPoint} );
        }


        private bool handle_internal_packets(IPEndPoint sender, Packet packet) {
            switch (packet.PacketType) {
                case PacketType.ConnectPacket:
                    Console.WriteLine("[Connection] Got connectpacket, bruh");
                    if (!activeClients.TryGetValue(sender, out ClientInfo? client)) return true; // what
                    if(client == null) return true;                                              //also what

                    ConnectPacket cp = (ConnectPacket)packet;
                    client.clientUDPEndPoint = new IPEndPoint(sender.Address, cp.UdpPort);

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

                    //NetworkStream stream = client.GetStream();    //This does not work for more than one client (???)
                    //if (stream.DataAvailable) {
                    //    while (stream.DataAvailable) {
                    //        Console.WriteLine($"[TCP] Expecting ConnectPacket, reading...");
                    //        Packet? packet = PacketReader.ReadFromStream(stream);
                    //        if (packet != null) {
                    //            if (packet.PacketType == PacketType.ConnectPacket) {
                    //                IPEndPoint sender = client.Client.RemoteEndPoint as IPEndPoint;
                    //                ConnectPacket cp = (ConnectPacket)packet;
                    //                ClientInfo newClient = new ClientInfo { clientTCP = client, clientUDP = new() };
                    //                newClient.clientUDP.Connect(sender.Address, cp.UdpPort);
                    //                activeClients.TryAdd(sender, newClient);
                    //                Console.WriteLine($"[TCP] Successfully connected client {sender}!\n");
                    //            }
                    //            else Console.WriteLine("    Packet was not of type ConnectPacket");
                    //        }
                    //        else Console.WriteLine("    ConnectPacket was null...");
                    //    }
                    //}

                    if (client.Client.RemoteEndPoint != null)
                        activeClients.TryAdd(client.Client.RemoteEndPoint as IPEndPoint, new ClientInfo { clientTCP = client }); //accept connection and add to active connections

                    //might want to do smth here like send a greeting message or whatever
                    //or probably should read the connectPacket uuhhhh idk
                    SendTCP(client.Client.RemoteEndPoint as IPEndPoint, new TestPacket("oh, hi!!"));
                }

                Parallel.ForEach(activeClients, (KeyValuePair<IPEndPoint, ClientInfo> client) => {  //read all incoming messages (this is probably wrong i have no idea)
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


        private void _networkUdpReceive() { //TODO: internal messages
            if (!_connectionRunning) return;
        
            IPEndPoint connection = new IPEndPoint(IPAddress.Any, 0);
        
            while (_connectionRunning && !t_ct.IsCancellationRequested) {
                if(udpListener.Available > 0) { //UDP Listen
                    byte[] data = udpListener.Receive(ref connection);
                    Console.WriteLine($"[UDP] Received message from {connection}, reading...");
                    Packet? packet = PacketReader.ReadFromRaw(data);
                    if(packet != null)
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
