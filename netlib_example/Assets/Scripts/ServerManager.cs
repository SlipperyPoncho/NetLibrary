using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetLib_NETStandart.Server;
using NetLib_NETStandart;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class ServerManager : MonoBehaviour
{
    Server server;

    private ConcurrentQueue<uint> players_to_spawn = new ConcurrentQueue<uint>();

    private Queue<(uint, PlayerInput)> inputs_queue = new Queue<(uint, PlayerInput)> ();
    private Dictionary<uint, GameObject> players = new Dictionary<uint, GameObject> ();

    [SerializeField]
    GameObject playerPrefab;

    void Start()
    {
        server = new Server(11000);
        server.Start();
        server.onNewConnection += spawnNewPlayer;

        Debug.Log("Server started!");
    }

    public void spawnNewPlayer(object sender, ServerEventArgs args) {
        Debug.Log("New player connected!");
        players_to_spawn.Enqueue(args.new_client_id);
    }

    public void UpdateClientPositions() {
        foreach (KeyValuePair<uint, GameObject> client in players) { 
            players.TryGetValue(client.Key, out GameObject gameObject);
            Transform data = gameObject.transform;

            PlayerPosition newpos = new() {
                client_id = client.Key,
                X = data.transform.position.x,
                Y = data.transform.position.y,
                rotation = data.transform.rotation.eulerAngles.z,
            };
            PlayerPositionPacket pp = new PlayerPositionPacket(newpos);
            
            foreach (KeyValuePair<uint, GameObject> client_send in players)
                server.connection.SendUDP(client_send.Key, pp);
        }
    }

    private void FixedUpdate() {
        while(players_to_spawn.Count > 0) {
            players_to_spawn.TryDequeue(out uint new_player_id);
            players.Add(new_player_id, Instantiate(playerPrefab));
        }

        while(server.q_incomingMessages.Count > 0) { // read incoming messages (probably a separate task)
            server.q_incomingMessages.TryDequeue(out NetMessage msg);
            PartialPacket packet = (PartialPacket)msg.packet;
            byte[] payload = packet.Payload;
            PacketReader.ReadInt(ref payload, 0, out int packetType);
            Debug.Log("[Server] Received custom packet of type: " + packetType);
            Debug.Log(payload.Length);
            switch (packetType) {
                case (int)customPacketType.PlayerInput:
                    PlayerInput inputs = PlayerInputPacket.read(payload);
                    inputs_queue.Enqueue((packet.Sender, inputs));
                    break;
                default:
                    Debug.Log("POOP POOP");
                    break;
            }
        }

        if(players.Count > 0) 
        if (inputs_queue.Count > 0) {
            while (inputs_queue.Count > 0) {
                (uint, PlayerInput) input = inputs_queue.Dequeue();
                players[input.Item1].GetComponent<Movement>().setInputs(input.Item2);
            }
        }

        UpdateClientPositions();
    }
}
