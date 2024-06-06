using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 5f;  // Speed of the character

    private Rigidbody2D rb;  // Rigidbody2D component for physics-based movement
    private Vector2 moveInput;  // Store the player's input

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // Get the Rigidbody2D component attached to the GameObject

        // Ensure rotation is constrained
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Get input from the player
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // Create a vector with the input values
        moveInput = new Vector2(moveX, moveY).normalized;  // Normalize to ensure consistent movement speed in all directions
    }

    void FixedUpdate()
    {
        // Move the character by setting the position of the Rigidbody2D
        Vector2 moveVelocity = moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveVelocity);
    }
}