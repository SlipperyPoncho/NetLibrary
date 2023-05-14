using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLib_NETStandart.Packets {
    public class ConnectAckPacket : Packet {
        private uint key;
        public uint Key { get => key; set => key = value; }
        public ConnectAckPacket(uint key) {
            this.key = key;
            PacketType = PacketType.ConnectAckPacket;
        }

        public override byte[] GetRaw() {
            MemoryStream stream = new MemoryStream();
            byte[] data = BitConverter.GetBytes(PacketID);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes((int)PacketType);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(Sender);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(sizeof(uint));
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(key);
            stream.Write(data, 0, data.Length);

            return stream.ToArray();
        }
    }
}
