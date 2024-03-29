﻿// seq is no longer needed for packets (so is the isReliable flag)

using System;
using System.IO;

namespace NetLib_NETStandart.Packets {
    public class ConnectPacket : Packet {
        private int udpPort;
        public int UdpPort { get => udpPort; set => udpPort = value; }

        public ConnectPacket(int port) {
            udpPort = port;
            PacketType = PacketType.ConnectPacket;
        }

        public override byte[] GetRaw() {
            MemoryStream stream = new MemoryStream();

            byte[] data = BitConverter.GetBytes(PacketID);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes((int)PacketType);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(Sender);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(sizeof(int));
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(udpPort);
            stream.Write(data, 0, data.Length);

            return stream.ToArray();
        }
    }
}