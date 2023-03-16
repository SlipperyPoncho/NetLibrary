using System;

namespace NetLib
{

    public static class PacketReader {
        public static IPacket readFromRaw(byte[] data) { return null; }
    }

    public interface IPacket {
        public bool isReliable { get; set; }
        public byte[] getRaw();
        public long seq { get; set; }
    }


    public class Packet : IPacket {
        public bool isReliable { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public long seq { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public byte[] getRaw() {
            throw new NotImplementedException();
        }
    }

    public class ACKPacket : Packet {
        public long seq = 0;
    }
}
