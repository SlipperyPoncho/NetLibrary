using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetLib_NETStandart;
using System.IO;
using System;

public enum customPacketType {
    PlayerInput,
    PlayerPosition,
}

public struct PlayerInput {
    public bool W;
    public bool A;
    public bool S;
    public bool D;
}

public struct PlayerPosition {
    public uint client_id;
    public float X;
    public float Y;
    public float rotation;
}

public class PlayerInputPacket : Packet {
    customPacketType c_packetType = customPacketType.PlayerInput;
    PlayerInput inputs;
    public PlayerInputPacket(PlayerInput inputs) {
        this.inputs = inputs;
        PacketType = NetLib_NETStandart.PacketType.CustomPacket;
    }

    public static PlayerInput read(byte[] payload) {
        int index = sizeof(int);
        index = PacketReader.ReadBool(ref payload, index, out bool W);
        index = PacketReader.ReadBool(ref payload, index, out bool A);
        index = PacketReader.ReadBool(ref payload, index, out bool S);
        PacketReader.ReadBool(ref payload, index, out bool D);

        return new PlayerInput() { W = W, A = A, S = S, D = D };
    }

    public override byte[] GetRaw() {
        MemoryStream stream = new MemoryStream();

        byte[] data = BitConverter.GetBytes(PacketID);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes((int)PacketType);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes(Sender);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes(sizeof(bool) * 4 + sizeof(int));
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes((int)c_packetType);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes(inputs.W);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes(inputs.A);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes(inputs.S);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes(inputs.D);
        stream.Write(data, 0, data.Length);

        return stream.ToArray();
    }
}

public class PlayerPositionPacket : Packet {
    customPacketType c_packetType = customPacketType.PlayerPosition;
    PlayerPosition input;
    public PlayerPositionPacket(PlayerPosition input) {
        this.input = input;
        PacketType = NetLib_NETStandart.PacketType.CustomPacket;
    }

    public static PlayerPosition read(byte[] payload) {
        int index = sizeof(int);
        index = PacketReader.ReadUint(ref payload, index, out uint client_id);
        index = PacketReader.ReadFloat(ref payload, index, out float X);
        index = PacketReader.ReadFloat(ref payload, index, out float Y);
        PacketReader.ReadFloat(ref payload, index, out float rotation);

        return new PlayerPosition() { client_id = client_id, X = X, Y = Y, rotation = rotation };
    }

    public override byte[] GetRaw() {
        MemoryStream stream = new MemoryStream();

        byte[] data = BitConverter.GetBytes(PacketID);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes((int)PacketType);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes(Sender);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes(sizeof(int) + sizeof(uint) + sizeof(float) * 3);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes((int)c_packetType);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes(input.client_id);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes(input.X);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes(input.Y);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes(input.rotation);
        stream.Write(data, 0, data.Length);


        return stream.ToArray();
    }
}