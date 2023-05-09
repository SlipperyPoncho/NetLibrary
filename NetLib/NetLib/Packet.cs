using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;


// seq is no longer needed for packets (so is the isReliable flag)
namespace NetLib
{

    public abstract class Packet
    {
        private int packetID;
        private PacketType packetType;
        private uint sender_key;
        

        public int PacketID { get => packetID; set => packetID = value; }
        public PacketType PacketType { get => packetType; set => packetType = value; }
        public uint Sender { get => sender_key; set => sender_key = value; }

        public abstract byte[] GetRaw();
    }


    public class ConnectPacket : Packet {
        private int udpPort;
        public int UdpPort { get => udpPort; set => udpPort = value; }

        public ConnectPacket(int port) {
            this.udpPort = port;
            PacketType = PacketType.ConnectPacket;
        }

        public override byte[] GetRaw() {
            MemoryStream stream = new();

            byte[] data = BitConverter.GetBytes(PacketID);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes((int)PacketType);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes((uint)Sender);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(sizeof(int));
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(udpPort);
            stream.Write(data, 0, data.Length);

            return stream.ToArray();
        }
    }

    public class ConnectAckPacket : Packet {
        private uint key;
        public uint Key { get => key; set => key = value; }
        public ConnectAckPacket(uint key) {
            this.key = key;
            PacketType = PacketType.ConnectAckPacket;
        }

        public override byte[] GetRaw() {
            MemoryStream stream = new();
            byte[] data = BitConverter.GetBytes(PacketID);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes((int)PacketType);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes((uint)Sender);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(sizeof(uint));
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes((uint)key);
            stream.Write(data, 0, data.Length);

            return stream.ToArray();
        }
    }

    public class TestPacket : Packet
    {
        private string text;

        public string Text { get => text; set => text = value; }

        public TestPacket(string text)
        {
            this.text = text;
            PacketType = PacketType.TestPacket;
        }

        public override byte[] GetRaw()
        {
            MemoryStream stream = new();

            byte[] data = BitConverter.GetBytes(PacketID);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes((int)PacketType);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes((uint)Sender);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(text.Length * sizeof(char));
            stream.Write(data, 0, data.Length);

            data = Encoding.Unicode.GetBytes(text);
            stream.Write(data, 0, data.Length);

            return stream.ToArray();
        }
    }
}