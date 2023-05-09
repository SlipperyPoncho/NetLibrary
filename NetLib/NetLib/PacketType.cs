using System;

namespace NetLib
{
    public enum PacketType
    {
        TestPacket,
        ConnectPacket,
        ConnectAckPacket,
        HeartbeatPacket,
        HeartbeatAckPacket,
        DisconnectPacket,
    }
}
