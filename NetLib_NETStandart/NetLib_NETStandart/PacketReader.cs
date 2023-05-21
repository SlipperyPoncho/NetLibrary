using NetLib_NETStandart.Packets;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace NetLib_NETStandart {
    public static class PacketReader
    {
        public static int ReadInt(ref byte[] data, int index, out int result) 
        {
            result = BitConverter.ToInt32(data, index);
            return index + sizeof(int);
        }

        public static int ReadFloat(ref byte[] data, int index, out float result) {
            result = BitConverter.ToSingle(data, index);
            return index + sizeof(float);
        }

        public static int ReadBool(ref byte[] data, int index, out bool result) {
            result = BitConverter.ToBoolean(data, index);
            return index + sizeof(bool);
        }

        public static int ReadUint(ref byte[] data, int index, out uint result) {
            result = BitConverter.ToUInt32(data, index);
            return index + sizeof(uint);
        }

        public static Packet? ReadFromRaw(byte[] data)
        {
            Console.WriteLine("[PacketReader] reading from raw: ");
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();


            int packetID = BitConverter.ToInt32(data, 0);
            Console.WriteLine($" PacketID = {packetID}");

            PacketType packetType = (PacketType)BitConverter.ToInt32(data, sizeof(int));
            Console.WriteLine($" PacketType = {packetType}");

            uint sender = BitConverter.ToUInt32(data, sizeof(int) * 2);
            Console.WriteLine($" Sender = {sender}");

            int payloadLength = BitConverter.ToInt32(data, sizeof(int) * 3);
            Console.WriteLine($" Payload length = {payloadLength}\n");

            byte[] payloadData = new byte[payloadLength];

            Array.Copy(data, sizeof(int) * 4, payloadData, 0, payloadLength);

            watch.Stop();
            Console.WriteLine($"[PACKETREADER]Execution Time: {watch.ElapsedTicks}");
            switch (packetType)
            {
                case PacketType.TestPacket:
                    TestPacket testPacket = new TestPacket(Encoding.Unicode.GetString(payloadData))
                    {
                        PacketID = packetID,
                        PacketType = packetType,
                        Sender = sender,
                        
                    };
                    return testPacket;

                case PacketType.ConnectPacket:
                    ConnectPacket conPacket = new ConnectPacket(BitConverter.ToInt32(payloadData)) {
                        PacketID = packetID,
                        PacketType = packetType,
                        Sender = sender,
                    };
                    return conPacket;

                case PacketType.ConnectAckPacket:
                    ConnectAckPacket conAckPacket = new ConnectAckPacket(BitConverter.ToUInt32(payloadData)) {
                        PacketID = packetID,
                        PacketType = packetType,
                        Sender = sender,
                    };
                    return conAckPacket;
                
                case PacketType.HeartbeatPacket:
                    HeartbeatPacket heartbeatPacket = new HeartbeatPacket(new DateTime(BitConverter.ToInt64(payloadData)))
                    {
                        PacketID = packetID,
                        PacketType = packetType,
                        Sender = sender,
                    };
                    return heartbeatPacket;

                case PacketType.HeartbeatAckPacket:
                    HeartbeatAckPacket heartbeatAckPacket = new HeartbeatAckPacket(new DateTime(BitConverter.ToInt64(payloadData)))
                    {
                        PacketID = packetID,
                        PacketType = packetType,
                        Sender = sender,
                    };
                    return heartbeatAckPacket;

                case PacketType.DisconnectPacket:
                    DisconnectPacket disconnectPacket = new DisconnectPacket(Encoding.Unicode.GetString(payloadData))
                    {
                        PacketID = packetID,
                        PacketType = packetType,
                        Sender = sender,
                    };
                    return disconnectPacket;

                default:
                    PartialPacket pp = new PartialPacket(payloadData) {
                        PacketID = packetID,
                        PacketType = packetType,
                        Sender = sender,
                    };
                    return pp;
            }
        }

        public static Packet? ReadFromStream(NetworkStream stream)
        {
            Console.WriteLine("[PacketReader] reading from stream: ");

            BinaryReader binaryReader = new BinaryReader(stream);
            int packetID = binaryReader.ReadInt32();
            Console.WriteLine($" PacketID = {packetID}");

            PacketType packetType = (PacketType)binaryReader.ReadInt32();
            Console.WriteLine($" PacketType = {packetType}");

            uint sender = binaryReader.ReadUInt32();
            Console.WriteLine($" Sender = {sender}");

            int payloadLength = binaryReader.ReadInt32();
            Console.WriteLine($" Payload length = {payloadLength}\n");

            switch (packetType)
            {
                case PacketType.TestPacket:
                    TestPacket testPacket = new TestPacket(Encoding.Unicode.GetString(binaryReader.ReadBytes(payloadLength))) {
                        PacketID = packetID,
                        PacketType = packetType,
                        Sender = sender,
                    };
                    return testPacket;
                
                case PacketType.ConnectPacket:
                    ConnectPacket conPacket = new ConnectPacket(binaryReader.ReadInt32()) {
                        PacketID = packetID,
                        PacketType = packetType,
                        Sender = sender,
                    };
                    return conPacket;
                
                case PacketType.ConnectAckPacket:
                    ConnectAckPacket conAckPacket = new ConnectAckPacket(binaryReader.ReadUInt32()) {
                        PacketID = packetID,
                        PacketType = packetType,
                        Sender = sender,
                    };
                    return conAckPacket;

                case PacketType.HeartbeatPacket:
                    HeartbeatPacket heartbeatPacket = new HeartbeatPacket(new DateTime(binaryReader.ReadInt64()))
                    {
                        PacketID = packetID,
                        PacketType = packetType,
                        Sender = sender,
                    };
                    return heartbeatPacket;

                case PacketType.HeartbeatAckPacket:
                    HeartbeatAckPacket heartbeatAckPacket = new HeartbeatAckPacket(new DateTime(binaryReader.ReadInt64()))
                    {
                        PacketID = packetID,
                        PacketType = packetType,
                        Sender = sender,
                    };
                    return heartbeatAckPacket;

                case PacketType.DisconnectPacket:
                    DisconnectPacket disconnectPacket = new DisconnectPacket(Encoding.Unicode.GetString(binaryReader.ReadBytes(payloadLength)))
                    {
                        PacketID = packetID,   
                        PacketType = packetType,
                        Sender = sender,
                    };
                    return disconnectPacket;

                default:
                    PartialPacket pp = new PartialPacket(binaryReader.ReadBytes(payloadLength)) {
                        PacketID = packetID,
                        PacketType = packetType,
                        Sender = sender,
                    };
                    return pp;        
            }
        }
    }
}