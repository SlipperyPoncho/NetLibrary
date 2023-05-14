using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetLib_NETStandart.Server;
using NetLib_NETStandart;

public class ServerManager : MonoBehaviour
{
    Server server;
    void Start()
    {
        server = new Server(11000);
        server.Start();
    }

    private void FixedUpdate() {
        if(server.q_incomingMessages.Count > 0) { // read incoming messages
            server.q_incomingMessages.TryDequeue(out NetMessage msg);
            PartialPacket packet = (PartialPacket)msg.packet;
            byte[] payload = PacketReader.ReadInt(packet.Payload, out int packetType);
            switch (packetType) {
                case (int)customPacketType.PlayerInput:
                    bool[] inputs = PlayerInputPacket.readInputs(payload);

                    break;
                default:
                    Debug.Log("POOP POOP");
                    break;
            }
        }
    }
}
