using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private void Awake() {
        SceneManager.LoadScene(3, LoadSceneMode.Additive);
    }
}
