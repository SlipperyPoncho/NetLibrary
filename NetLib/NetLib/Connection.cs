using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Text;

namespace NetLib
{
    public class ClientInfo {
        public long sequence = 0;
        public float rtt = 0;
        public bool isAlive = true;

        public ConcurrentDictionary<long, IPacket> reliablePackets = new();
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
        
        private Thread _networkListener;
        private UdpClient client;  
        private bool _connectionRunning = false;
        private int port;

        public Connection(int port) {
             this.port = port;
             client = new(port);
        
             _networkListener = new Thread(new ThreadStart(_networkReceive));
        }
        
        public void Start() {
            _connectionRunning = true;
            _networkListener.Start();
        }
        
        public void Close() {
            _connectionRunning = false;
        }

        public void Send(IPEndPoint receiver, IPacket packet, bool reliable = false) { //this is retarted. it should somehow send it and then wait for the ack message. probably better to make it a separate async function entirely
            if (reliable) {
                if (!activeClients.TryGetValue(receiver, out ClientInfo? info)) return;  // is info a copy or a reference??? should i update it later?
                if (info == null) return;

                packet.isReliable = true;
                packet.seq = info.sequence;

                info.reliablePackets.TryAdd(packet.seq, packet);

                byte[] data = packet.getRaw();
                client.Send(data, receiver);
            }
            else{ //send-and-forget
                byte[] data = packet.getRaw();
                client.Send(data, receiver);
            }
        }

        public void registerActiveClient(IPEndPoint endPoint) {
            if (endPoint == null) return;
            activeClients.TryAdd(endPoint, new ClientInfo());
        }

        //separate thread
        private void _networkReceive() {
            if (!_connectionRunning) return;

            IPEndPoint connection = new IPEndPoint(IPAddress.Any, port);

            while (_connectionRunning) {
                if(client.Available > 0) {
                    byte[] data = client.Receive(ref connection);   //somewhere here it should check if it's an ack packet and then let the send shit know that it should stop knocking at the clients door
                    IPacket packet = PacketReader.readFromRaw(data);
                    NetMessage message = new NetMessage { packet = packet, RecieveTime = DateTime.Now };
                    q_incomingMessages.Enqueue(message);
                }
            }
        
        }

    }

}
