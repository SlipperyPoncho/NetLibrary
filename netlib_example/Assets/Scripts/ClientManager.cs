using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetLib_NETStandart;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class ClientManager : MonoBehaviour
{
    Client client;

    private ConcurrentQueue<PlayerPosition> pos_queue = new ConcurrentQueue<PlayerPosition>();
    Dictionary<uint, GameObject> player_representations = new Dictionary<uint, GameObject>();

    [SerializeField]
    private GameObject player_representation;

    private void Start() {
        Debug.Log("Client started!");
        client = new Client(new IPEndPoint(IPAddress.Parse("25.57.228.113"), 11000));
        client.Start();
        Debug.Log("Client started!");
        //t_messageLoop = new Task(messageReceiveLoop);
        //t_messageLoop.Start();
    }

    private void FixedUpdate() {
        Debug.Log("Incoming messages: " + client.q_incomingMessages.Count);
        messageReceiveLoop();

        while (pos_queue.Count > 0) {
            pos_queue.TryDequeue(out PlayerPosition newpos);
            if(!player_representations.TryGetValue(newpos.client_id, out _)) {
                player_representations.Add(newpos.client_id, Instantiate(player_representation));
            }
            player_representations[newpos.client_id].transform.position = new Vector2(newpos.X, newpos.Y);
            player_representations[newpos.client_id].transform.rotation = Quaternion.Euler(0, 0, newpos.rotation);
        }


        PlayerInput newInput = new PlayerInput {
            W = Input.GetKey(KeyCode.W),
            A = Input.GetKey(KeyCode.A),
            S = Input.GetKey(KeyCode.S),
            D = Input.GetKey(KeyCode.D),
        };
        SendInputs(newInput);
    }

    private void SendInputs(PlayerInput inputs) {
        PlayerInputPacket pp = new PlayerInputPacket(inputs);
        client.connection.SendUDP(1, pp);
    }

    private void messageReceiveLoop() {
        while (client.q_incomingMessages.Count > 0) { // read incoming messages (probably a separate task)
            Debug.Log("helloo???");
            client.q_incomingMessages.TryDequeue(out NetMessage msg);
            PartialPacket packet = (PartialPacket)msg.packet;
            byte[] payload = packet.Payload;
            PacketReader.ReadInt(ref payload, 0, out int packetType);
            switch (packetType) {
                case (int)customPacketType.PlayerPosition:
                    PlayerPosition newpos = PlayerPositionPacket.read(payload);
                    Debug.Log(newpos.client_id);
                    pos_queue.Enqueue(newpos);
                    break;
                default:
                    Debug.Log("POOP POOP");
                    break;
            }
        }
    }
}
