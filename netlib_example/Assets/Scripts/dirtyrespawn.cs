using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dirtyrespawn : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision) {
        collision.gameObject.transform.position = new Vector2(0, 0);
    }
}
