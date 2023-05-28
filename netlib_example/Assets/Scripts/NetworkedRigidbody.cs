using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NetworkedRigidbody : MonoBehaviour
{
    private Rigidbody2D rb;
    public int id = 1;
    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }
    public void SetSpeed(Vector2 speed) {
        rb.velocity = speed;        
    }
}
