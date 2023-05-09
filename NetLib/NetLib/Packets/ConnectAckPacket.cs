using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLib.Packets {
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
}
