using System.Text;

namespace NetLib
{
    public class TestPacket : Packet
    {
        private string text;

        public string Text { get => text; set => text = value; }

        public TestPacket(string text)
        {
            this.text = text;
            PacketType = PacketType.TestPacket;
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

            data = Encoding.Unicode.GetBytes(text);
            stream.Write(data, 0, data.Length);

            return stream.ToArray();
        }
    }
}