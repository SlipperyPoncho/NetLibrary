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
    private Dictionary<uint, float> playersPing = new Dictionary<uint, float>();
    private Dictionary<uint, int> playersScore = new Dictionary<uint, int>();
    private List<uint> deadPlayers = new List<uint>();
    private Queue<uint> players_to_delete = new Queue<uint>();
    private NetworkedRigidbody[] level_bodies;

    [SerializeField]
    GameObject playerPrefab;

    private UIManager ui;

    public float playerSpawnRange = 5f;

    void Start()
    {
        level_bodies = FindObjectsOfType<NetworkedRigidbody>();
        ui = FindObjectOfType<UIManager>();
        Debug.Log(level_bodies.Length);
        server = new Server(11000);
        server.Start();
        server.onNewConnection += spawnNewPlayer;
        server.onHeartbeat += ping_update;

        server.onClientDisconnect += ((object sender, ServerEventArgs args) => {
            Debug.Log($"HOLY SHIT!!! {args.client_id} disconnected!!!");
            players_to_delete.Enqueue(args.client_id);
            
        });

        Debug.Log("Server started!");
    }

    public void ping_update(object sender, ServerEventArgs args) {
        playersPing = args.heartbeatInfo;
    }

    public void spawnNewPlayer(object sender, ServerEventArgs args) {
        Debug.Log("New player connected!");
        players_to_spawn.Enqueue(args.client_id);
        playersScore.Add(args.client_id, 0);
    }

    public void UpdateLevel() {
        if (level_bodies == null) return;
        foreach(NetworkedRigidbody rb in level_bodies) {
            RBPosition newpos = new() {
                objectID = rb.id,
                X = rb.transform.position.x,
                Y = rb.transform.position.y,
                speedX = rb.GetComponent<Rigidbody2D>().velocity.x,
                speedY = rb.GetComponent<Rigidbody2D>().velocity.y,
                rotation = rb.transform.rotation.eulerAngles.z,
            };
            RBpositionPacket rpp = new RBpositionPacket(newpos);
            foreach (KeyValuePair<uint, GameObject> client_send in players)
                server.connection.SendUDP(client_send.Key, rpp);
        }
    }

    public void UpdateClientPositions() {
        foreach (KeyValuePair<uint, GameObject> client in players) {
            if (deadPlayers.Contains(client.Key)) continue;
            players.TryGetValue(client.Key, out GameObject gameObject);
            Transform data = gameObject.transform;

            float ping = 0;
            if (playersPing != null)
                if(playersPing.TryGetValue(client.Key, out float new_ping))
                    ping = new_ping;

            playersScore.TryGetValue(client.Key, out int score);

            PlayerPosition newpos = new() {
                client_id = client.Key,
                X = data.transform.position.x,
                Y = data.transform.position.y,
                speedX = gameObject.GetComponent<Rigidbody2D>().velocity.x,
                speedY = gameObject.GetComponent<Rigidbody2D>().velocity.y,
                rotation = data.transform.rotation.eulerAngles.z,
                spacePressed = gameObject.GetComponent<Movement>().input.Space,
                ping = ping,
                score = score,
            };
            PlayerPositionPacket pp = new PlayerPositionPacket(newpos);
            
            foreach (KeyValuePair<uint, GameObject> client_send in players)
                server.connection.SendUDP(client_send.Key, pp);
        }
    }

    public Vector2 newSpawnPos(int i) {
        if (players.Count == 1) return new Vector2(0, 0);
        float p = (float)i / ((float)players.Count-1f);
        Debug.Log($"{p} => {-playerSpawnRange + (2 * playerSpawnRange * p)}");
        return new Vector2(-playerSpawnRange + (2 * playerSpawnRange * p), 0);
    }

    private void FixedUpdate() {
        while(players_to_delete.Count > 0) {
            uint client_id = players_to_delete.Dequeue();
            Destroy(ui.players[client_id].gameObject);
            ui.players.Remove(client_id);
            Destroy(players[client_id].gameObject);
            players.Remove(client_id);
            playersScore.Remove(client_id);
            server.connection.SendToAll(new PlayerDisconnectedPacket(client_id));
        }

        while(players_to_spawn.Count > 0) {
            players_to_spawn.TryDequeue(out uint new_player_id);
            players.Add(new_player_id, Instantiate(playerPrefab));
            ui.AddNewPlayerInfo(new_player_id, $"Player {new_player_id - 1}");
            players[new_player_id].GetComponent<Movement>().onPlayerDead.AddListener(() => {
                Debug.Log($"player {new_player_id} died!!");
                if (!deadPlayers.Contains(new_player_id)) {
                    deadPlayers.Add(new_player_id);
                    server.connection.SendToAll(new PlayerDeadPacket(new PlayerDead { client_id = new_player_id, X = players[new_player_id].transform.position.x, Y = players[new_player_id].transform.position.y }), false);
                }

                if(deadPlayers.Count >= players.Count - 1) {
                    uint winner = 0;
                    foreach(var player in players) {
                        if (!deadPlayers.Contains(player.Key)) {
                            winner = player.Key;
                            break;
                        }
                    }
                    if (winner != 0) playersScore[winner]++;
                    server.connection.SendToAll(new RestartRoundPacket(winner));
                    deadPlayers.Clear();
                    int i = 0;
                    foreach (var player in players) {
                        player.Value.transform.position = newSpawnPos(i);
                        player.Value.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
                        i++;
                    }
                }
            });
        }

        while(server.q_incomingMessages.Count > 0) { // read incoming messages (probably a separate task)
            server.q_incomingMessages.TryDequeue(out NetMessage msg);
            PartialPacket packet = (PartialPacket)msg.packet;
            byte[] payload = packet.Payload;
            PacketReader.ReadInt(ref payload, 0, out int packetType);
            switch (packetType) {
                case (int)customPacketType.PlayerInput:
                    PlayerInput inputs = PlayerInputPacket.read(payload);
                    inputs_queue.Enqueue((packet.header.sender, inputs));
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

        UpdateLevel();

        if(playersPing!=null)
            foreach (KeyValuePair<uint, float> client in playersPing) 
                ui.SetPlayerPing(client.Key, client.Value);

    }
}
