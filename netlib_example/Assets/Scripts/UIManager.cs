using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public Dictionary<uint, GameObject> players = new Dictionary<uint, GameObject>();
    public GameObject player_info_row;
    public VerticalLayoutGroup playerPanel;
    public Button exitButton;


    public UnityEvent onExitPressed;
    public void ExitButton() {
        onExitPressed?.Invoke();
    }

    public void AddNewPlayerInfo(uint client_id, string name) {
        players.Add(client_id, Instantiate(player_info_row));
        players[client_id].transform.SetParent(playerPanel.transform);
        players[client_id].transform.Find("name").GetComponent<TMPro.TextMeshProUGUI>().text = name;
    }

    public void SetPlayerScore(uint client_id, int score) {
        players[client_id].transform.Find("score").GetComponent<TMPro.TextMeshProUGUI>().text = score.ToString();
    }

    public void SetPlayerPing(uint client_id, float ping) {
        if (players.TryGetValue(client_id, out _))
            players[client_id].transform.Find("ping").GetComponent<TMPro.TextMeshProUGUI>().text = $"({ping}ms)";
    }
}
