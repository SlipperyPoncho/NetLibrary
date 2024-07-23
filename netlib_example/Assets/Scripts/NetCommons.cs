using NetLib_NETStandart;
using System.IO;

public enum customPacketType {
    PlayerInput,
    PlayerPosition,
    RBPosition,
    PlayerDisconnected,
    PlayerDead,
    RestartRound,
}

public struct PlayerInput {
    public bool W;
    public bool A;
    public bool S;
    public bool D;
    public bool Space;
}

public struct PlayerPosition {
    public uint client_id;
    public float X;
    public float Y;
    public float speedX;
    public float speedY;
    public float rotation;
    public bool spacePressed;
    
    public float ping;
    public int score;
}

public struct RBPosition {
    public int objectID;
    public float X;
    public float Y;
    public float speedX;
    public float speedY;
    public float rotation;
}

public struct PlayerDead {
    public uint client_id;
    public float X;
    public float Y;
}

public class RBpositionPacket : Packet{
    customPacketType c_packetType = customPacketType.RBPosition;
    RBPosition input;
    public RBpositionPacket(RBPosition input) {
        header.packetType = NetLib_NETStandart.PacketType.CustomPacket;
        this.input = input;
    }

    public static RBPosition read(byte[] payload) {
        int index = sizeof(int);
        index = PacketReader.ReadInt(ref payload, index, out int objectID);
        index = PacketReader.ReadFloat(ref payload, index, out float X);
        index = PacketReader.ReadFloat(ref payload, index, out float Y);
        index = PacketReader.ReadFloat(ref payload, index, out float speedX);
        index = PacketReader.ReadFloat(ref payload, index, out float speedY);
        PacketReader.ReadFloat(ref payload, index, out float rotation);

        return new RBPosition() { objectID = objectID, X = X, Y = Y, speedX = speedX, speedY = speedY, rotation = rotation };
    }

    public override byte[] GetRaw() {
        MemoryStream payloadstream = new MemoryStream();
        PacketBuilder.WriteInt(ref payloadstream, (int)c_packetType);
        PacketBuilder.WriteInt(ref payloadstream, input.objectID);
        PacketBuilder.WriteFloat(ref payloadstream, input.X);
        PacketBuilder.WriteFloat(ref payloadstream, input.Y);
        PacketBuilder.WriteFloat(ref payloadstream, input.speedX);
        PacketBuilder.WriteFloat(ref payloadstream, input.speedY);
        PacketBuilder.WriteFloat(ref payloadstream, input.rotation);
        header.payloadLength = (int)payloadstream.Length;

        MemoryStream stream = new MemoryStream();
        PacketBuilder.WriteHeader(ref stream, header);
        payloadstream.WriteTo(stream);

        return stream.ToArray();
    }
}

public class PlayerInputPacket : Packet {
    customPacketType c_packetType = customPacketType.PlayerInput;
    PlayerInput inputs;
    public PlayerInputPacket(PlayerInput inputs) {
        header.packetType = NetLib_NETStandart.PacketType.CustomPacket;
        this.inputs = inputs;
    }

    public static PlayerInput read(byte[] payload) {
        int index = sizeof(int);
        index = PacketReader.ReadBool(ref payload, index, out bool W);
        index = PacketReader.ReadBool(ref payload, index, out bool A);
        index = PacketReader.ReadBool(ref payload, index, out bool S);        
        index = PacketReader.ReadBool(ref payload, index, out bool D);
        PacketReader.ReadBool(ref payload, index, out bool Space);

        return new PlayerInput() { W = W, A = A, S = S, D = D, Space = Space };
    }

    public override byte[] GetRaw() {
        MemoryStream payloadstream = new MemoryStream();
        PacketBuilder.WriteInt(ref payloadstream, (int)c_packetType);
        PacketBuilder.WriteBool(ref payloadstream, inputs.W);
        PacketBuilder.WriteBool(ref payloadstream, inputs.A);
        PacketBuilder.WriteBool(ref payloadstream, inputs.S);
        PacketBuilder.WriteBool(ref payloadstream, inputs.D);
        PacketBuilder.WriteBool(ref payloadstream, inputs.Space);
        header.payloadLength = (int)payloadstream.Length;

        MemoryStream stream = new MemoryStream();
        PacketBuilder.WriteHeader(ref stream, header);
        payloadstream.WriteTo(stream);


        return stream.ToArray();
    }
}

public class PlayerPositionPacket : Packet {
    customPacketType c_packetType = customPacketType.PlayerPosition;
    PlayerPosition input;
    public PlayerPositionPacket(PlayerPosition input) {
        header.packetType = NetLib_NETStandart.PacketType.CustomPacket;
        this.input = input;
    }

    public static PlayerPosition read(byte[] payload) {
        int index = sizeof(int);
        index = PacketReader.ReadUint(ref payload, index, out uint client_id);
        index = PacketReader.ReadFloat(ref payload, index, out float X);
        index = PacketReader.ReadFloat(ref payload, index, out float Y);
        index = PacketReader.ReadFloat(ref payload, index, out float speedX);
        index = PacketReader.ReadFloat(ref payload, index, out float speedY);
        index = PacketReader.ReadFloat(ref payload, index, out float rotation);
        index = PacketReader.ReadBool(ref payload, index, out bool spacePressed);
        index = PacketReader.ReadFloat(ref payload, index, out float ping);
        PacketReader.ReadInt(ref payload, index, out int score);

        return new PlayerPosition() { client_id = client_id, X = X, Y = Y, speedX = speedX, speedY = speedY, rotation = rotation, spacePressed = spacePressed, ping = ping, score = score };
    }

    public override byte[] GetRaw() {
        MemoryStream payloadstream = new MemoryStream();
        PacketBuilder.WriteInt(ref payloadstream, (int)c_packetType);
        PacketBuilder.WriteUint(ref payloadstream, input.client_id);
        PacketBuilder.WriteFloat(ref payloadstream, input.X);
        PacketBuilder.WriteFloat(ref payloadstream, input.Y);
        PacketBuilder.WriteFloat(ref payloadstream, input.speedX);
        PacketBuilder.WriteFloat(ref payloadstream, input.speedY);
        PacketBuilder.WriteFloat(ref payloadstream, input.rotation);
        PacketBuilder.WriteBool(ref payloadstream, input.spacePressed);
        PacketBuilder.WriteFloat(ref payloadstream, input.ping);
        PacketBuilder.WriteInt(ref payloadstream, input.score);
        header.payloadLength = (int)payloadstream.Length;

        MemoryStream stream = new MemoryStream();
        PacketBuilder.WriteHeader(ref stream, header);
        payloadstream.WriteTo(stream);

        return stream.ToArray();
    }
}

public class PlayerDisconnectedPacket : Packet {
    customPacketType c_packetType = customPacketType.PlayerDisconnected;
    uint player_id;
    public PlayerDisconnectedPacket(uint input) {
        header.packetType = NetLib_NETStandart.PacketType.CustomPacket;
        this.player_id = input;
    }

    public static uint read(byte[] payload) {
        int index = sizeof(int);
        PacketReader.ReadUint(ref payload, index, out uint player_id);

        return player_id;
    }

    public override byte[] GetRaw() {
        MemoryStream payloadstream = new MemoryStream();
        PacketBuilder.WriteInt(ref payloadstream, (int)c_packetType);
        PacketBuilder.WriteUint(ref payloadstream, player_id);
        header.payloadLength = (int)payloadstream.Length;

        MemoryStream stream = new MemoryStream();
        PacketBuilder.WriteHeader(ref stream, header);
        payloadstream.WriteTo(stream);

        return stream.ToArray();
    }
}

public class PlayerDeadPacket : Packet {
    customPacketType c_packetType = customPacketType.PlayerDead;
    PlayerDead input;
    public PlayerDeadPacket(PlayerDead input) {
        header.packetType = NetLib_NETStandart.PacketType.CustomPacket;
        this.input = input;
    }

    public static PlayerDead read(byte[] payload) {
        int index = sizeof(int);
        index = PacketReader.ReadUint(ref payload, index, out uint client_id);
        index = PacketReader.ReadFloat(ref payload, index, out float X);
        PacketReader.ReadFloat(ref payload, index, out float Y);

        return new PlayerDead { client_id = client_id, X = X, Y = Y };
    }

    public override byte[] GetRaw() {
        MemoryStream payloadstream = new MemoryStream();
        PacketBuilder.WriteInt(ref payloadstream, (int)c_packetType);
        PacketBuilder.WriteUint(ref payloadstream, input.client_id);
        PacketBuilder.WriteFloat(ref payloadstream, input.X);
        PacketBuilder.WriteFloat(ref payloadstream, input.Y);
        header.payloadLength = (int)payloadstream.Length;

        MemoryStream stream = new MemoryStream();
        PacketBuilder.WriteHeader(ref stream, header);
        payloadstream.WriteTo(stream);

        return stream.ToArray();
    }
}

public class RestartRoundPacket : Packet {
    customPacketType c_packetType = customPacketType.RestartRound;
    uint winner_id;
    public RestartRoundPacket(uint input) {
        header.packetType = NetLib_NETStandart.PacketType.CustomPacket;
        this.winner_id = input;
    }

    public static uint read(byte[] payload) {
        int index = sizeof(int);
        PacketReader.ReadUint(ref payload, index, out uint winner_id);

        return winner_id;
    }

    public override byte[] GetRaw() {
        MemoryStream payloadstream = new MemoryStream();
        PacketBuilder.WriteInt(ref payloadstream, (int)c_packetType);
        PacketBuilder.WriteUint(ref payloadstream, winner_id);
        header.payloadLength = (int)payloadstream.Length;

        MemoryStream stream = new MemoryStream();
        PacketBuilder.WriteHeader(ref stream, header);
        payloadstream.WriteTo(stream);

        return stream.ToArray();
    }
}