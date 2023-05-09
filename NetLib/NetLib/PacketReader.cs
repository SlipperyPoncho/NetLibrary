using System.Net.Sockets;
using System.Text;

namespace NetLib
{
    public static class PacketReader
    {
        public static Packet? ReadFromRaw(byte[] data)
        {
            int packetID = BitConverter.ToInt32(data, 0);
            PacketType packetType = (PacketType)BitConverter.ToInt32(data, sizeof(int));
            int payloadLength = BitConverter.ToInt32(data, sizeof(int) * 2);
            byte[] payloadData = new byte[payloadLength];

            Array.Copy(data, sizeof(int) * 3, payloadData, 0, payloadLength);
            
            switch (packetType)
            {
                case PacketType.TestPacket:
                    TestPacket testPacket = new(Encoding.Unicode.GetString(payloadData))
                    {
                        PacketID = packetID,
                        PacketType = packetType
                    };
                    return testPacket;
                case PacketType.ConnectPacket:
                    ConnectPacket conPacket = new(BitConverter.ToInt32(payloadData));
                    return conPacket;
                case PacketType.HeartbeatPacket:
                    HeartbeatPacket heartbeatPacket = new(new DateTime(BitConverter.ToInt32(payloadData)));
                    return heartbeatPacket;
                default:
                    return null;
            }
        }

        public static Packet? ReadFromStream(NetworkStream stream)
        {
            Console.WriteLine("[PacketReader] reading from stream: ");
            BinaryReader binaryReader = new(stream);
            int packetID = binaryReader.ReadInt32();
            Console.WriteLine($" PacketID = {packetID}");
            PacketType packetType = (PacketType)binaryReader.ReadInt32();
            Console.WriteLine($" PacketType = {packetType}");
            int payloadLength = binaryReader.ReadInt32();
            Console.WriteLine($" Payload length = {payloadLength}\n");

            switch (packetType)
            {
                case PacketType.TestPacket:
                    TestPacket testPacket = new(Encoding.Unicode.GetString(binaryReader.ReadBytes(payloadLength)))
                    {
                        PacketID = packetID,
                        PacketType = packetType,
                    };
                    return testPacket;
                case PacketType.ConnectPacket:
                    ConnectPacket conPacket = new(binaryReader.ReadInt32());
                    return conPacket;
                case PacketType.HeartbeatPacket:
                    HeartbeatPacket heartbeatPacket = new(new DateTime(binaryReader.ReadInt32()));
                    return heartbeatPacket;
                default:
                    return null;        
            }
        }
    }
}