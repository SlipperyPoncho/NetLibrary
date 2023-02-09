using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public class serverManager : MonoBehaviour
{
    Server server = new Server();

    Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

    [SerializeField]
    GameObject playerPrefab;

    private void Start() {
        server.StartServering();
        server.onNewPlayerConnected += spawnNewPlayer;
    }

    private void FixedUpdate() {
        server.Tick();

        for (int i = 1; i <= players.Count; i++) {
            server.UpdateClientPositions(i, players[i].transform);
        }

        if (server.inputs_queue.Count != 0) {
            while (server.inputs_queue.Count > 0) {
                (int, PlayerInput) input = server.inputs_queue.Dequeue();
                players[input.Item1].GetComponent<movement>().setInputs(input.Item2);
            }
        }

    }

    public void spawnNewPlayer(object sender, ServerEventArgs args) {
        players.Add(args.new_client_id, Instantiate(playerPrefab));
    }
}

[Serializable]
public struct PlayerInput {
    public bool W { get; set; }
    public bool A { get; set; }
    public bool S { get; set; }
    public bool D { get; set; }
}

[Serializable]
public struct PlayerPosition {
    public int client_id { get; set; }
    public float x_pos { get; set; }
    public float y_pos { get; set; }
    public float rot { get; set; }
}

public class ServerEventArgs : EventArgs {
    public int new_client_id { get; set; }
}

public class Server {
    readonly Dictionary<int, TcpClient> clients = new Dictionary<int, TcpClient>();
    TcpListener listener = new TcpListener(IPAddress.Any, 8000);
    int client_count = 1;

    public Queue<(int, PlayerInput)> inputs_queue = new Queue<(int, PlayerInput)>(); // queue of inputs to be processed

    public event EventHandler<ServerEventArgs> onNewPlayerConnected;

    public Server() {}

    public void StartServering() {
        listener.Start();
    }

    public void Tick() {

        if (listener.Pending()) { //check for incoming connections
            TcpClient client = listener.AcceptTcpClient();
            clients.Add(client_count, client);
            onNewPlayerConnected?.Invoke(this, new ServerEventArgs { new_client_id = client_count });
            Debug.Log($"Accepted connection: {client.Client.RemoteEndPoint}");
            client_count++;
        }

        if (clients.Count != 0)
        for (int i = 1; i <= clients.Count; i++) { // read incoming inputs
            NetworkStream stream = clients[i].GetStream();
            while (stream.DataAvailable) {
                PlayerInput inputs = (PlayerInput)new BinaryFormatter().Deserialize(stream);
                inputs_queue.Enqueue((i, inputs));
            }
        }
    }

    //private void ClientMovementStream(object obj_clientId) {
    //    int client_id = (int)obj_clientId;
    //    TcpClient client;
    //    lock (_threadlock) client = clients[client_id];
    //    NetworkStream stream = client.GetStream();
    //    //BinaryReader reader = new BinaryReader(stream);
    //    while (isRunning) {
    //        PlayerInput inputs = (PlayerInput)new BinaryFormatter().Deserialize(stream);
    //        lock (_threadlock) inputs_queue.Enqueue((client_id, inputs));
    //    }
    //}

    public void UpdateClientPositions(int client_id, Transform data) {
        foreach (TcpClient client in clients.Values) {
            NetworkStream stream = client.GetStream();
            PlayerPosition newpos = new() { client_id = client_id,
                x_pos = data.transform.position.x,
                y_pos = data.transform.position.y,
                rot = data.transform.rotation.eulerAngles.z
            };
            //Debug.Log($"new pos = x: {newpos.x_pos}, y: {newpos.y_pos}, rot: {newpos.rot}.");
            new BinaryFormatter().Serialize(stream, newpos);
            stream.Flush();
        }
    }
}

public class Client {
    private TcpClient client;
    private NetworkStream stream;
    public Queue<PlayerPosition> updated_queue = new Queue<PlayerPosition>(); // queue of updates to be processed

    public Client() {}

    public void StartClienting() {
        client = new TcpClient("25.42.87.228", 8000);
        stream = client.GetStream();
    }

    public void Tick() {
        while (stream.DataAvailable) { //read new positions
            PlayerPosition updatedPos = (PlayerPosition)new BinaryFormatter().Deserialize(stream);
            updated_queue.Enqueue(updatedPos);
        }
    }

    //private void ReceiveMessages(object obj_client) {
    //    TcpClient client = (TcpClient)obj_client;
    //    NetworkStream stream = client.GetStream();
    //    while (isRunning) {
    //        PlayerPosition updatedPos = (PlayerPosition)new BinaryFormatter().Deserialize(stream);
    //        lock (_threadlock) updated_queue.Enqueue(updatedPos);
    //    }
    //}

    public void WriteInputs(PlayerInput input) {
        NetworkStream stream = client.GetStream();
        new BinaryFormatter().Serialize(stream, input);
        stream.Flush();
    }
}