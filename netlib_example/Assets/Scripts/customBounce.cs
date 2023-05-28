using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class customBounce : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision) {
        //elastic collission if the other is player
        if (collision.gameObject.tag == "player") {
            Vector2 normal = collision.contacts[0].normal;
            GetComponent<Rigidbody2D>().AddForce(normal * collision.contacts[0].normalImpulse * 0.8f, ForceMode2D.Impulse);
        }

        //if (collision.gameObject.tag != "player") {
        //    Debug.Log("what");
        //    Vector2 d = Vector2.Dot(rb.velocity, collision.contacts[0].normal) * collision.contacts[0].normal;
        //    rb.velocity -= d;
        //}
    }
}
