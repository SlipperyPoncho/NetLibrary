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

        public int PacketID { get => packetID; set => packetID = value; }
        public PacketType PacketType { get => packetType; set => packetType = value; }

        public abstract byte[] GetRaw();
    }

    public class TestPacket : Packet
    {
        private string text;

        public string Text { get => text; set => text = value; }

        public TestPacket(string text)
        {
            this.text = text;
        }

        public override byte[] GetRaw()
        {
            MemoryStream stream = new();

            byte[] data = BitConverter.GetBytes(PacketID);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes((int)PacketType);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(text.Length * sizeof(char));
            stream.Write(data, 0, data.Length);

            data = Encoding.ASCII.GetBytes(text);
            stream.Write(data, 0, data.Length);

            return stream.ToArray();
        }
    }
}