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

        public static int ReadLong(ref byte[] data, int index, out long result) {
            result = BitConverter.ToInt64(data, index);
            return index + sizeof(long);
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

        public static int ReadString(ref byte[] data, int index, out string result) {
            int msg_length = BitConverter.ToInt32(data, index);
            result = Encoding.Unicode.GetString(data, index + sizeof(int), msg_length);
            return index + sizeof(int) + msg_length;
        }

        public static int ReadBytes(ref byte[] data, int index, int size, out byte[] result) {
            result = new byte[size];
            Array.Copy(data, index, result, 0, size);
            return index + size;
        }

        public static Packet? ReadFromRaw(byte[] data)
        {
            Console.WriteLine("[PacketReader] reading from raw: ");
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            PacketType packetType = (PacketType)BitConverter.ToInt32(data, 0);
            Console.WriteLine($" PacketType = {packetType}");

            uint sender = BitConverter.ToUInt32(data, sizeof(int));
            Console.WriteLine($" Sender = {sender}");

            int payloadLength = BitConverter.ToInt32(data, sizeof(int) * 2);
            Console.WriteLine($" Payload length = {payloadLength}\n");

            byte[] payloadData = new byte[payloadLength];

            Array.Copy(data, sizeof(int) * 3, payloadData, 0, payloadLength);

            watch.Stop();
            Console.WriteLine($"[PACKETREADER]Execution Time: {watch.ElapsedTicks}");
            switch (packetType)
            {
                case PacketType.TestPacket:
                    TestPacket testPacket = new TestPacket(Encoding.Unicode.GetString(payloadData));
                    testPacket.header.sender = sender;
                    return testPacket;

                case PacketType.ConnectPacket:
                    ConnectPacket conPacket = new ConnectPacket(BitConverter.ToInt32(payloadData));
                    conPacket.header.sender = sender;
                    return conPacket;

                case PacketType.ConnectAckPacket:
                    ConnectAckPacket conAckPacket = new ConnectAckPacket(BitConverter.ToUInt32(payloadData));
                    conAckPacket.header.sender = sender;
                    return conAckPacket;
                
                case PacketType.HeartbeatPacket:
                    HeartbeatPacket heartbeatPacket = new HeartbeatPacket(new DateTime(BitConverter.ToInt64(payloadData)));
                    heartbeatPacket.header.sender = sender;
                    return heartbeatPacket;

                case PacketType.HeartbeatAckPacket:
                    HeartbeatAckPacket heartbeatAckPacket = new HeartbeatAckPacket(new DateTime(BitConverter.ToInt64(payloadData)));
                    heartbeatAckPacket.header.sender = sender;
                    return heartbeatAckPacket;

                case PacketType.DisconnectPacket:
                    DisconnectPacket disconnectPacket = new DisconnectPacket(Encoding.Unicode.GetString(payloadData));
                    disconnectPacket.header.sender = sender;
                    return disconnectPacket;

                default:
                    PartialPacket pp = new PartialPacket(payloadData);
                    pp.header.packetType = packetType;
                    pp.header.sender = sender;
                    return pp;
            }
        }

        public static Packet? ReadFromStream(NetworkStream stream)
        {
            Console.WriteLine("[PacketReader] reading from stream: ");

            byte[] packet_data;
            byte[] read_data = new byte[1024];
            using(MemoryStream ms = new MemoryStream()) {
                int numBytesRead;
                while (stream.DataAvailable) {
                    numBytesRead = stream.Read(read_data, 0, read_data.Length);
                    ms.Write(read_data, 0, numBytesRead);
                }
                packet_data = ms.ToArray();
            }
            int index = 0;
            Console.WriteLine($" Packet size = {packet_data.Length}");
            index = ReadInt(ref packet_data, index, out int i_packetType);
            PacketType packetType = (PacketType)i_packetType;
            Console.WriteLine($" PacketType = {packetType}");
            index = ReadUint(ref packet_data, index, out uint sender);
            Console.WriteLine($" Sender = {sender}");
            index = ReadInt(ref packet_data, index, out int payloadLength);
            Console.WriteLine($" Payload length = {payloadLength}\n");



            switch (packetType)
            {
                case PacketType.TestPacket:
                    ReadString(ref packet_data, index, out string tp_msg);
                    TestPacket testPacket = new TestPacket(tp_msg);
                    testPacket.header.sender = sender;
                    return testPacket;
                
                case PacketType.ConnectPacket:
                    ReadInt(ref packet_data, index, out int cp_port);
                    ConnectPacket conPacket = new ConnectPacket(cp_port);
                    conPacket.header.sender = sender;
                    return conPacket;
                
                case PacketType.ConnectAckPacket:
                    ReadUint(ref packet_data, index, out uint cap_key);
                    ConnectAckPacket conAckPacket = new ConnectAckPacket(cap_key);
                    conAckPacket.header.sender = sender;
                    return conAckPacket;

                case PacketType.HeartbeatPacket:
                    ReadLong(ref packet_data, index, out long hp_stamp);
                    HeartbeatPacket heartbeatPacket = new HeartbeatPacket(new DateTime(hp_stamp));
                    heartbeatPacket.header.sender = sender;
                    return heartbeatPacket;

                case PacketType.HeartbeatAckPacket:
                    ReadLong(ref packet_data, index, out long hap_stamp);
                    HeartbeatAckPacket heartbeatAckPacket = new HeartbeatAckPacket(new DateTime(hap_stamp));
                    heartbeatAckPacket.header.sender = sender;
                    return heartbeatAckPacket;

                case PacketType.DisconnectPacket:
                    ReadString(ref packet_data, index, out string dp_msg);
                    DisconnectPacket disconnectPacket = new DisconnectPacket(dp_msg);
                    disconnectPacket.header.sender = sender;
                    return disconnectPacket;

                default:
                    ReadBytes(ref packet_data, index, payloadLength, out byte[] pp_payload);
                    PartialPacket pp = new PartialPacket(pp_payload);
                    pp.header.packetType = packetType;
                    pp.header.sender = sender;
                    return pp;        
            }
        }
    }
}