using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
                    TestPacket testPacket = new(Encoding.ASCII.GetString(payloadData))
                    {
                        PacketID = packetID,
                        PacketType = packetType
                    };
                    return testPacket;
                default:
                    return null;
            }
        }

        public static Packet? ReadFromStream(NetworkStream stream)
        {
            BinaryReader binaryReader = new(stream);
            int packetID = binaryReader.ReadInt32();
            PacketType packetType = (PacketType)binaryReader.ReadInt32();
            _ = binaryReader.ReadInt32();

            switch (packetType)
            {
                case PacketType.TestPacket:
                    TestPacket testPacket = new(binaryReader.ReadString())
                    {
                        PacketID = packetID,
                        PacketType = packetType,
                    };
                    return testPacket;
                default:
                    return null;        
            }
        }
    }
}