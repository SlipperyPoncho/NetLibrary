using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;


// seq is no longer needed for packets (so is the isReliable flag)
namespace NetLib_NETStandart {
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

    public class PartialPacket : Packet {
        private byte[] payload;
        public byte[] Payload { get => payload; private set => payload = value; }
        public PartialPacket(byte[] payload) {
            this.payload = payload;
        }

        public override byte[] GetRaw() {
            MemoryStream stream = new MemoryStream();

            byte[] data = BitConverter.GetBytes(PacketID);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes((int)PacketType);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(Sender);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(payload.Length);
            stream.Write(data, 0, data.Length);

            stream.Write(payload, 0, payload.Length);

            return stream.ToArray();
        }
    }
}