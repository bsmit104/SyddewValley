using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    public Animator animator;

    // lastMoveDir will be set to DOWN any time the player is not moving
    public Vector2 lastMoveDir = Vector2.down;

    public SwordSwing sword;

    // when input magnitude is <= this, we treat player as "idle"
    [Tooltip("If input magnitude is <= this value, player is considered idle and facing will be set to down.")]
    public float idleThreshold = 0.01f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.freezeRotation = true;
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // Raw input or normalized — keep normalized for consistent movement
        moveInput = new Vector2(moveX, moveY).normalized;

        // If we are moving enough, update lastMoveDir to movement direction
        if (moveInput.sqrMagnitude > idleThreshold * idleThreshold)
        {
            lastMoveDir = moveInput;
        }
        else
        {
            // If we are not moving (idle), force facing down
            lastMoveDir = Vector2.down;
        }

        // Animator updates (if used)
        if (animator != null)
        {
            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);
            animator.SetFloat("Speed", moveInput.magnitude);
        }

        // Attack input (space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (sword != null)
                sword.Swing(lastMoveDir);
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        Vector2 moveVelocity = moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveVelocity);
    }
}



// using UnityEngine;

// public class Movement : MonoBehaviour
// {
//     public float moveSpeed = 5f;

//     private Rigidbody2D rb;
//     private Vector2 moveInput;

//     public Animator animator;

//     public Vector2 lastMoveDir = Vector2.right; // store last direction facing
//     public SwordSwing sword; // drag the Sword object here

//     void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         rb.freezeRotation = true;
//     }

//     void Update()
//     {
//         float moveX = Input.GetAxis("Horizontal");
//         float moveY = Input.GetAxis("Vertical");

//         moveInput = new Vector2(moveX, moveY).normalized;

//         // Update last facing direction if moving
//         if (moveInput.sqrMagnitude > 0.01f)
//         {
//             lastMoveDir = moveInput;
//         }

//         // Animator parameters
//         if (animator != null)
//         {
//             animator.SetFloat("Horizontal", moveInput.x);
//             animator.SetFloat("Vertical", moveInput.y);
//             animator.SetFloat("Speed", moveInput.magnitude);
//         }

//         // ATTACK INPUT
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             sword.Swing(lastMoveDir);
//         }
//     }

//     void FixedUpdate()
//     {
//         Vector2 moveVelocity = moveInput * moveSpeed * Time.fixedDeltaTime;
//         rb.MovePosition(rb.position + moveVelocity);
//     }
// }


















// using UnityEngine;

// public class Movement : MonoBehaviour
// {
//     public float moveSpeed = 5f;  // Speed of the character

//     private Rigidbody2D rb;  // Rigidbody2D component for physics-based movement
//     private Vector2 moveInput;  // Store the player's input
//     public Animator animator;

//     void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();  // Get the Rigidbody2D component attached to the GameObject

//         // Ensure rotation is constrained
//         rb.freezeRotation = true;
//     }

//     void Update()
//     {
//         // Get input from the player
//         float moveX = Input.GetAxis("Horizontal");
//         float moveY = Input.GetAxis("Vertical");

//         // Create a vector with the input values
//         moveInput = new Vector2(moveX, moveY).normalized;  // Normalize to ensure consistent movement speed in all directions

//         animator.SetFloat("Horizontal", moveInput.x);
//         animator.SetFloat("Vertical", moveInput.y);
//         animator.SetFloat("Speed", moveInput.magnitude);
//     }

//     void FixedUpdate()
//     {
//         // Move the character by setting the position of the Rigidbody2D
//         Vector2 moveVelocity = moveInput * moveSpeed * Time.fixedDeltaTime;
//         rb.MovePosition(rb.position + moveVelocity);
//     }
// }