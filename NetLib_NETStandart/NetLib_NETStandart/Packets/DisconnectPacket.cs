using System;
using System.IO;
using System.Text;

namespace NetLib_NETStandart.Packets {
    public class DisconnectPacket : Packet 
    {
        private string msg;

        public string Msg { get => msg; set => msg = value; }

        public DisconnectPacket(string msg) 
        {
            this.msg = msg;
            PacketType = PacketType.DisconnectPacket;
        }

        public override byte[] GetRaw() 
        {
            MemoryStream stream = new MemoryStream();

            byte[] data = BitConverter.GetBytes(PacketID);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes((int)PacketType);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(Sender);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(msg.Length * sizeof(char));
            stream.Write(data, 0, data.Length);

            data = Encoding.Unicode.GetBytes(msg);
            stream.Write(data, 0, data.Length);

            return stream.ToArray();
        }
    }
}
