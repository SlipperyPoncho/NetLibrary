using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clientManager : MonoBehaviour
{
    Client client = new Client();

    Dictionary<int, GameObject> player_representations = new Dictionary<int, GameObject>();

    [SerializeField]
    private GameObject player_representation;

    private void Start() {
        client.StartClienting();
    }

    private void FixedUpdate() {
        client.Tick();

        if (client.updated_queue.Count != 0) {
            while (client.updated_queue.Count > 0) {
                PlayerPosition newPos = client.updated_queue.Dequeue();
                if (!player_representations.TryGetValue(newPos.client_id, out _)) {
                    player_representations.Add(newPos.client_id, Instantiate(player_representation));
                }
                player_representations[newPos.client_id].transform.position = new Vector2(newPos.x_pos, newPos.y_pos);
                player_representations[newPos.client_id].transform.rotation = Quaternion.Euler(0, 0, newPos.rot);
            }
        }

        PlayerInput newInput = new PlayerInput {
            W = Input.GetKey(KeyCode.W),
            A = Input.GetKey(KeyCode.A),
            S = Input.GetKey(KeyCode.S),
            D = Input.GetKey(KeyCode.D)
        };
        client.WriteInputs(newInput);
    }
}
