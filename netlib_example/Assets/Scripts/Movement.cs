using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour {
    [SerializeField]
    float acceleration = 1f;
    [SerializeField]
    float jumpForce = 10f;

    Rigidbody2D rb;

    public PlayerInput input = new PlayerInput();

    private bool lastUpInput = false;

    public bool use_local_inputs = false;

    private SpriteRenderer spacerenderer;

    public UnityEvent onPlayerDead;


    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        spacerenderer = transform.Find("border").GetComponent<SpriteRenderer>();
    }

    public void setInputs(PlayerInput new_input) {
        input = new_input;
    }

    private bool isGrounded() {
        RaycastHit2D hit = Physics2D.Raycast(transform.position - new Vector3(0, transform.localScale.y+0.1f, 0), Vector2.down, 0.05f);
        return hit.collider != null;
    }

    private void FixedUpdate() {
        if (use_local_inputs) setInputs(new PlayerInput { 
            W = Input.GetKey(KeyCode.W),
            A = Input.GetKey(KeyCode.A),
            S = Input.GetKey(KeyCode.S),
            D = Input.GetKey(KeyCode.D),
            Space = Input.GetKey(KeyCode.Space),
        });

        spacerenderer.enabled = input.Space;

        float x_component = (input.A ? -1 : 0) + (input.D ? 1 : 0);
        float y_component = (input.S ? -1 : 0) + (input.W ? 1 : 0);

        if(isGrounded() && (input.W && input.W != lastUpInput)) {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (input.Space) {
            rb.mass = 10;
        }
        else {
            rb.mass = 1;
        }

        Vector2 move_dir = new Vector2(x_component, y_component * 0.5f);
        rb.AddForce(move_dir.normalized * acceleration);
        //rb.velocity = Vector2.ClampMagnitude(rb.velocity, max_speed);

        lastUpInput = input.W;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.transform.tag == "death") {
            onPlayerDead?.Invoke();
        }
    }
}
