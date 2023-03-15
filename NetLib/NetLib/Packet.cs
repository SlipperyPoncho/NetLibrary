namespace NetLib
{
    public enum PacketType
    {
        Unknown = 0,
        ConnectionRequest = 1,
        ConnectionAccept = 2,
        ConnectionDismiss = 3,
        Heartbeat = 4,
        ApplicationData = 5,
        Disconnect = 6,
        Acknowledgement = 7
    }

    public class Packet
    {
        public bool IsAcknowleged = false;
        public static int PacketID { get; set; }
        public static byte[]? PayloadData { get; set; }
        
        public Packet()
        {
            PacketID = 0;
            PayloadData = Array.Empty<byte>();
        }

        public Packet(int packetID, byte[] payloadData)
        {
            PacketID = packetID;
            PayloadData = payloadData;
        }

        public static byte[] Serialize()
        {
            MemoryStream stream = new();

            byte[] data = BitConverter.GetBytes(PacketID);
            stream.Write(data, 0, data.Length);

            #pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
            data = BitConverter.GetBytes(PayloadData.Length);
            #pragma warning restore CS8602 // Разыменование вероятной пустой ссылки.

            stream.Write(data, 0, data.Length);

            stream.Write(PayloadData, 0, PayloadData.Length);

            return stream.ToArray();
        }

        public static Packet Deserialize(byte[] data)
        {
            int packetID = BitConverter.ToInt32(data, 0);

            int payloadLength = BitConverter.ToInt32(data, sizeof(int));

            byte[] payloadData = new byte[payloadLength];
            Array.Copy(data, sizeof(int) * 2, payloadData, 0, payloadLength);

            Packet packet = new(packetID, payloadData);
            return packet;
        }
    }
}