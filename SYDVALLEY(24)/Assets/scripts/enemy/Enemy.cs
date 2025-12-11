using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Detection & Chase")]
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float chaseSpeed = 3f;

    [Header("Attack")]
    [SerializeField] private int damageAmount = 20;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 1.5f;
    [SerializeField] private float knockbackDuration = 0.15f;

    // REMOVE the serialized field! We find player at runtime
    // [SerializeField] private Transform playerTransform;  ← DELETE THIS LINE

    private Transform playerTransform;          // Now private, set at runtime
    private PlayerHealth playerHealth;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private float lastAttackTime;
    private bool playerInRange = false;
    private bool isKnockedBack = false;

    // Direction tracking
    private float lastDirX = 0f;
    private float lastDirY = -1f; // default down

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        // CRITICAL: Find player at runtime instead of relying on prefab reference
        FindPlayer();

        // Initialize facing direction
        if (animator != null)
        {
            animator.SetFloat("LastInputX", lastDirX);
            animator.SetFloat("LastInputY", lastDirY);
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
        else
        {
            Debug.LogWarning($"{name}: Player with tag 'Player' not found in scene!");
        }
    }

    private void OnEnable()
    {
        // In case enemy is pooled or re-enabled, re-find player
        if (playerTransform == null)
            FindPlayer();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = dist <= chaseRange;

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (playerTransform == null || isKnockedBack) return;

        if (playerInRange)
        {
            ChasePlayer();
        }
        else
        {
            UpdateAnimatorDirection(Vector2.zero);
        }
    }

    private void ChasePlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        UpdateAnimatorDirection(direction);

        Vector2 targetPos = rb.position + direction * chaseSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);

        if (spriteRenderer != null)
            spriteRenderer.flipX = direction.x < 0f && Mathf.Abs(direction.x) > Mathf.Abs(direction.y);
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = playerInRange && !isKnockedBack;

        animator.SetBool("isWalking", isMoving);
        animator.SetFloat("InputX", lastDirX);
        animator.SetFloat("InputY", lastDirY);
        animator.SetFloat("LastInputX", lastDirX);
        animator.SetFloat("LastInputY", lastDirY);
    }

    private void UpdateAnimatorDirection(Vector2 moveDirection)
    {
        if (moveDirection.sqrMagnitude < 0.01f) return;

        float newDirX = 0f;
        float newDirY = 0f;

        if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
        {
            newDirX = Mathf.Sign(moveDirection.x);
            newDirY = 0f;
        }
        else
        {
            newDirX = 0f;
            newDirY = Mathf.Sign(moveDirection.y);
        }

        lastDirX = newDirX;
        lastDirY = newDirY;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (Time.time < lastAttackTime + attackCooldown) return;
        if (playerHealth == null) return;

        playerHealth.TakeDamage(damageAmount);
        lastAttackTime = Time.time;

        if (!isKnockedBack)
            StartCoroutine(KnockbackRoutine());
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;

        Vector2 startPos = rb.position;
        Vector2 awayFromPlayer = (startPos - (Vector2)playerTransform.position).normalized;

        float currentDist = Vector2.Distance(startPos, playerTransform.position);
        float maxAllowed = chaseRange - currentDist - 0.2f;
        float actualDist = Mathf.Min(knockbackDistance, Mathf.Max(0f, maxAllowed));

        Vector2 targetPos = startPos + awayFromPlayer * actualDist;

        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / knockbackDuration;
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, t));
            yield return null;
        }

        rb.MovePosition(targetPos);
        isKnockedBack = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = playerInRange ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}

// // Enemy.cs - Updated version with full NPC-style animation support
// using System.Collections;
// using UnityEngine;

// public class Enemy : MonoBehaviour
// {
//     [Header("Detection & Chase")]
//     [SerializeField] private float chaseRange = 5f;
//     [SerializeField] private float chaseSpeed = 3f;

//     [Header("Attack")]
//     [SerializeField] private int damageAmount = 20;
//     [SerializeField] private float attackCooldown = 1f;

//     [Header("Knockback")]
//     [SerializeField] private float knockbackDistance = 1.5f;
//     [SerializeField] private float knockbackDuration = 0.15f;

//     [Header("References")]
//     [SerializeField] private Transform playerTransform;

//     // Animation variables (same pattern as ScheduledWaypointMover)
//     private Animator animator;
//     private SpriteRenderer spriteRenderer;
//     private PlayerHealth playerHealth;

//     private Rigidbody2D rb;
//     private float lastAttackTime;

//     private bool playerInRange = false;
//     private bool isKnockedBack = false;

//     // Direction tracking - exactly like your NPCs
//     private float lastDirX = 0f;
//     private float lastDirY = -1f; // default down (common for top-down games)

//     private void Awake()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         animator = GetComponent<Animator>();
//         spriteRenderer = GetComponent<SpriteRenderer>();
//     }

//     private void Start()
//     {
//         rb.bodyType = RigidbodyType2D.Kinematic;
//         rb.gravityScale = 0f;

//         if (playerTransform == null)
//         {
//             var playerObj = GameObject.FindGameObjectWithTag("Player");
//             if (playerObj != null) return;

//             playerTransform = playerObj.transform;
//             playerHealth = playerObj.GetComponent<PlayerHealth>();
//         }
//         else
//         {
//             playerHealth = playerTransform.GetComponent<PlayerHealth>();
//         }

//         // Initialize animator with default facing direction
//         UpdateAnimatorDirection(Vector2.down);
//     }

//     private void Update()
//     {
//         if (playerTransform == null) return;

//         float dist = Vector2.Distance(transform.position, playerTransform.position);
//         playerInRange = dist <= chaseRange;

//         // Always update animation based on movement direction (even when standing)
//         UpdateAnimation();
//     }

//     private void FixedUpdate()
//     {
//         if (playerTransform == null || isKnockedBack) return;

//         if (playerInRange)
//         {
//             ChasePlayer();
//         }
//         else
//         {
//             // Stop walking animation when not chasing
//             UpdateAnimatorDirection(Vector2.zero);
//         }
//     }

//     private void ChasePlayer()
//     {
//         Vector2 direction = (playerTransform.position - transform.position).normalized;

//         // Update facing direction BEFORE moving (so animation is correct on the same frame)
//         UpdateAnimatorDirection(direction);

//         Vector2 targetPos = rb.position + direction * chaseSpeed * Time.fixedDeltaTime;
//         rb.MovePosition(targetPos);

//         // Flip sprite horizontally if needed (optional - only if you don't use 8-dir animations)
//         // if (spriteRenderer != null)
//         //     spriteRenderer.flipX = direction.x < 0f && Mathf.Abs(direction.x) > Mathf.Abs(direction.y);
//     }

//     private void UpdateAnimation()
//     {
//         if (animator == null) return;

//         bool isMoving = playerInRange && !isKnockedBack;

//         animator.SetBool("isWalking", isMoving);
//         animator.SetFloat("InputX", lastDirX);
//         animator.SetFloat("InputY", lastDirY);
//         animator.SetFloat("LastInputX", lastDirX);
//         animator.SetFloat("LastInputY", lastDirY);
//     }

//     private void UpdateAnimatorDirection(Vector2 moveDirection)
//     {
//         if (moveDirection.sqrMagnitude < 0.01f)
//             return; // Don't change last direction when standing still

//         float newDirX = 0f;
//         float newDirY = 0f;

//         // Prioritize horizontal if equal, or pure cardinal direction
//         if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
//         {
//             newDirX = Mathf.Sign(moveDirection.x);
//             newDirY = 0f;
//         }
//         else
//         {
//             newDirX = 0f;
//             newDirY = Mathf.Sign(moveDirection.y);
//         }

//         lastDirX = newDirX;
//         lastDirY = newDirY;
//     }

//     private void OnCollisionEnter2D(Collision2D collision)
//     {
//         if (!collision.gameObject.CompareTag("Player")) return;
//         if (Time.time < lastAttackTime + attackCooldown) return;

//         playerHealth?.TakeDamage(damageAmount);
//         lastAttackTime = Time.time;

//         if (!isKnockedBack)
//             StartCoroutine(KnockbackRoutine());
//     }

//     private IEnumerator KnockbackRoutine()
//     {
//         isKnockedBack = true;

//         Vector2 startPos = rb.position;
//         Vector2 awayFromPlayer = (startPos - (Vector2)playerTransform.position).normalized;

//         float currentDist = Vector2.Distance(startPos, playerTransform.position);
//         float maxAllowed = chaseRange - currentDist - 0.2f;
//         float actualDist = Mathf.Min(knockbackDistance, Mathf.Max(0f, maxAllowed));

//         Vector2 targetPos = startPos + awayFromPlayer * actualDist;

//         float elapsed = 0f;
//         while (elapsed < knockbackDuration)
//         {
//             elapsed += Time.fixedDeltaTime;
//             float t = elapsed / knockbackDuration;

//             rb.MovePosition(Vector2.Lerp(startPos, targetPos, t));
//             yield return null;
//         }

//         rb.MovePosition(targetPos);
//         isKnockedBack = false;
//     }

//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.color = playerInRange ? Color.red : Color.yellow;
//         Gizmos.DrawWireSphere(transform.position, chaseRange);
//     }
// }

