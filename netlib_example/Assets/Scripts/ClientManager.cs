using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Search;
using NetLib_NETStandart;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using UnityEngine.Events;

public class ClientManager : MonoBehaviour
{
    Client client;

    private ConcurrentQueue<PlayerPosition> pos_queue = new ConcurrentQueue<PlayerPosition>();
    private ConcurrentQueue<RBPosition> levelpos_queue = new ConcurrentQueue<RBPosition>();

    Dictionary<uint, GameObject> player_representations = new Dictionary<uint, GameObject>();
    List<uint> deadPlayers = new List<uint>();

    [SerializeField]
    private GameObject player_representation;
    [SerializeField]
    private GameObject player_prefab;
    private GameObject player_instance;

    public UnityEvent<uint> OnConnected;
    private Dictionary<int, Rigidbody2D> levelBodies;

    public PlayerPositionTracker tracker;
    private UIManager ui;

    public GameObject playerDeadEffect;

    private void Start() {
        levelBodies = new Dictionary<int, Rigidbody2D>();
        var b = FindObjectsOfType<NetworkedRigidbody>(); 
        foreach(NetworkedRigidbody rb in b) {
            levelBodies.Add(rb.id, rb.GetComponent<Rigidbody2D>());
        }
        Debug.Log(b.Length);

        tracker = FindObjectOfType<PlayerPositionTracker>();
        ui = FindObjectOfType<UIManager>();

        ui.onExitPressed.AddListener(disconnect);

        player_instance = Instantiate(player_prefab);
        player_instance.GetComponent<Movement>().use_local_inputs = true;
        tracker.AddNewTracker(player_instance);

        client = new Client(new IPEndPoint(IPAddress.Parse("25.57.228.113"), 11000));
        client.Start();
        Debug.Log("Client started!");
        //t_messageLoop = new Task(messageReceiveLoop);
        //t_messageLoop.Start();
    }

    private void disconnect() {
        Debug.Log("fuck it");
        client.connection.Close();
        Application.Quit();
    }

    private void FixedUpdate() {
        if (!client.connected) return;
        else if (client.connected && !player_representations.TryGetValue(client.client_id, out _)) {
            player_representations.Add(client.client_id, player_instance);
            tracker.AddNewTracker(player_representations[client.client_id]);
            player_representations[client.client_id].transform.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = $"Player {client.client_id - 1} (YOU)";
            ui.AddNewPlayerInfo(client.client_id, $"Player {client.client_id - 1}");
        }
        messageReceiveLoop();

        while(levelpos_queue.Count > 0) {
            levelpos_queue.TryDequeue(out RBPosition newpos);
            var body = levelBodies[newpos.objectID];
            body.transform.position = new Vector2(newpos.X, newpos.Y);
            body.transform.rotation = Quaternion.Euler(0, 0, newpos.rotation);
            body.velocity = new Vector2(newpos.speedX, newpos.speedY);
        }

        while (pos_queue.Count > 0) {
            pos_queue.TryDequeue(out PlayerPosition newpos);
            if(!player_representations.TryGetValue(newpos.client_id, out _)) {
                player_representations.Add(newpos.client_id, Instantiate(player_representation));
                tracker.AddNewTracker(player_representations[newpos.client_id]);
                player_representations[newpos.client_id].transform.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = $"Player {newpos.client_id-1}";
                ui.AddNewPlayerInfo(newpos.client_id, $"Player {newpos.client_id - 1}");
            }
            if (!deadPlayers.Contains(newpos.client_id)) {
                player_representations[newpos.client_id].transform.position = new Vector2(newpos.X, newpos.Y);
                player_representations[newpos.client_id].transform.rotation = Quaternion.Euler(0, 0, newpos.rotation);
                player_representations[newpos.client_id].GetComponent<Rigidbody2D>().velocity = new Vector2(newpos.speedX, newpos.speedY);
                player_representations[newpos.client_id].transform.Find("border").GetComponent<SpriteRenderer>().enabled = newpos.spacePressed;
                ui.SetPlayerPing(newpos.client_id, newpos.ping);
                ui.SetPlayerScore(newpos.client_id, newpos.score);
            }
        }


        PlayerInput newInput = new PlayerInput {
            W = Input.GetKey(KeyCode.W),
            A = Input.GetKey(KeyCode.A),
            S = Input.GetKey(KeyCode.S),
            D = Input.GetKey(KeyCode.D),
            Space = Input.GetKey(KeyCode.Space),
        };
        SendInputs(newInput);
    }

    private void SendInputs(PlayerInput inputs) {
        PlayerInputPacket pp = new PlayerInputPacket(inputs);
        client.connection.SendUDP(1, pp);
    }

    private void hidePlayer(GameObject player) {
        player.GetComponent<Collider2D>().enabled = false;
        player.GetComponent<Rigidbody2D>().simulated = false;
        player.GetComponentInChildren<TMPro.TextMeshProUGUI>().enabled = false;
        //player.GetComponent<Movement>().enabled = false;
        player.GetComponent<SpriteRenderer>().enabled = false;
        player.transform.position = new Vector2(0, 0);
    }

    private void unhidePlayers() {
        foreach (var player in player_representations) {
            player.Value.GetComponent<Collider2D>().enabled = true;
            player.Value.GetComponent<Rigidbody2D>().simulated = true;
            player.Value.GetComponent<SpriteRenderer>().enabled = true;
            player.Value.GetComponentInChildren<TMPro.TextMeshProUGUI>().enabled = true;
            player.Value.transform.position = new Vector2(0, 0);
        }
    }

    private void messageReceiveLoop() {
        while (client.q_incomingMessages.Count > 0) { // read incoming messages (probably a separate task)
            client.q_incomingMessages.TryDequeue(out NetMessage msg);
            PartialPacket packet = (PartialPacket)msg.packet;
            byte[] payload = packet.Payload;
            PacketReader.ReadInt(ref payload, 0, out int packetType);
            switch (packetType) {
                case (int)customPacketType.PlayerDisconnected:
                    uint player_id = PlayerDisconnectedPacket.read(payload);
                    tracker.RemoveTracker(player_representations[player_id]);
                    Destroy(ui.players[player_id].gameObject);
                    ui.players.Remove(player_id);
                    Destroy(player_representations[player_id].gameObject);
                    player_representations.Remove(player_id);
                    break;
                case (int)customPacketType.PlayerPosition:
                    PlayerPosition pp_newpos = PlayerPositionPacket.read(payload);
                    pos_queue.Enqueue(pp_newpos);
                    break;
                case (int)customPacketType.RBPosition:
                    RBPosition rp_newpos = RBpositionPacket.read(payload);
                    levelpos_queue.Enqueue(rp_newpos);
                    break;
                case (int)customPacketType.PlayerDead:
                    PlayerDead pd_pos = PlayerDeadPacket.read(payload);
                    Vector3 pos = new Vector3(pd_pos.X, pd_pos.Y, 0);
                    deadPlayers.Add(pd_pos.client_id);
                    hidePlayer(player_representations[pd_pos.client_id]);
                    Instantiate(playerDeadEffect, pos, Quaternion.identity);
                    break;
                case (int)customPacketType.RestartRound:
                    deadPlayers.Clear();
                    unhidePlayers();
                    break;
                default:
                    Debug.Log("POOP POOP");
                    break;
            }
        }
    }
}
