using System;

namespace NetLib
{
    public enum PacketType
    {
        TestPacket,
        ConnectPacket,
        ConnectAckPacket,
        HeartbeatPacket,
        DisconnectPacket,
    }
}
