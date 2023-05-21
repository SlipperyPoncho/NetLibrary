using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour {
    [SerializeField]
    float acceleration = 1f;
    [SerializeField]
    float max_speed = 10f;

    Rigidbody2D rb;

    PlayerInput input = new PlayerInput();

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void setInputs(PlayerInput new_input) {
        input = new_input;
    }

    private void FixedUpdate() {
        float x_component = (input.A ? -1 : 0) + (input.D ? 1 : 0);
        float y_component = (input.S ? -1 : 0) + (input.W ? 1 : 0);
        Vector2 move_dir = new Vector2(x_component, y_component);
        rb.AddForce(move_dir.normalized * acceleration);
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, max_speed);
    }
}
