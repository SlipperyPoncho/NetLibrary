namespace NetLib.Packets
{
    public class HeartbeatAckPacket : Packet
    {
        private DateTime timeStamp;
        public DateTime TimeStamp { get => timeStamp; set => timeStamp = value; }

        public HeartbeatAckPacket(DateTime stamp)
        {
            timeStamp = stamp;
            PacketType = PacketType.HeartbeatAckPacket;
        }

        public override byte[] GetRaw()
        {
            MemoryStream stream = new();

            byte[] data = BitConverter.GetBytes(PacketID);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes((int)PacketType);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(Sender);
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(sizeof(long));
            stream.Write(data, 0, data.Length);

            data = BitConverter.GetBytes(timeStamp.Ticks);
            stream.Write(data, 0, data.Length);

            return stream.ToArray();
        }
    }
}
