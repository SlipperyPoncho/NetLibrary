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
    public int clientID;
    public float X;
    public float Y;
    public float rotation;
}

public class PlayerInputPacket : Packet {
    customPacketType c_packetType = customPacketType.PlayerInput;
    bool[] inputs;
    public PlayerInputPacket(bool[] inputs) {
        this.inputs = inputs;
        PacketType = NetLib_NETStandart.PacketType.CustomPacket;
    }

    public static bool[] readInputs(byte[] payload) {
        payload = PacketReader.ReadBool(payload, out bool W);
        payload = PacketReader.ReadBool(payload, out bool A);
        payload = PacketReader.ReadBool(payload, out bool S);
        payload = PacketReader.ReadBool(payload, out bool D);

        return new bool[] { W, A, S, D };
    }

    public override byte[] GetRaw() {
        MemoryStream stream = new MemoryStream();

        byte[] data = BitConverter.GetBytes(PacketID);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes((int)PacketType);
        stream.Write(data, 0, data.Length);
        
        data = BitConverter.GetBytes(Sender);
        stream.Write(data, 0, data.Length);

        data = BitConverter.GetBytes((int)c_packetType);
        stream.Write(data, 0, data.Length);

        for(int i = 0; i < 4; i++) {
            data = BitConverter.GetBytes(inputs[i]);
            stream.Write(data, 0, data.Length);
        }

        return stream.ToArray();
    }
}