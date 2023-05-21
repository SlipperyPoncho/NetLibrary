using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void start_server_scene() {
        SceneManager.LoadScene(1);
    }

    public void start_client_scene() {
        SceneManager.LoadScene(2);
    }
}
