using System.Net.Sockets;
using System.Runtime.CompilerServices;


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
}