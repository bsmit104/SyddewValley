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

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;
    private float lastAttackTime;

    private bool playerInRange = false;
    private bool isKnockedBack = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        if (playerTransform == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
        }
        else
        {
            playerHealth = playerTransform.GetComponent<PlayerHealth>();
        }

        if (playerTransform == null)
            Debug.LogError("Enemy: Player not found!");
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = dist <= chaseRange;
    }

    private void FixedUpdate()
    {
        if (playerTransform == null || isKnockedBack) return;

        if (playerInRange)
        {
            ChasePlayer();
        }
    }

    private void ChasePlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        Vector2 targetPos = rb.position + direction * chaseSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);

        if (spriteRenderer != null)
            spriteRenderer.flipX = direction.x < 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        playerHealth?.TakeDamage(damageAmount);
        lastAttackTime = Time.time;

        // ONE knockback at a time
        if (!isKnockedBack)
            StartCoroutine(KnockbackRoutine());
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;

        Vector2 startPos = rb.position;
        Vector2 awayFromPlayer = (startPos - (Vector2)playerTransform.position).normalized;

        // Don't knockback outside chase range
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


// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Enemy : MonoBehaviour
// {
//     public float idleTimeMin = 1.0f;
//     public float idleTimeMax = 3.0f;
//     public float walkTimeMin = 1.0f;
//     public float walkTimeMax = 3.0f;
//     public float walkSpeed = 2.0f;
//     public float chaseDistance = 5.0f;
//     public float attackStrength = 10.0f; // Default attack strength for all enemies
//     public Transform player;

//     protected Rigidbody2D rb;
//     //protected Animator animator;
//     protected Vector2 movement;
//     protected float idleTimer;
//     protected float walkTimer;
//     protected bool isChasing = false;

//     protected virtual void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         //animator = GetComponent<Animator>();

//         idleTimer = Random.Range(idleTimeMin, idleTimeMax);
//         walkTimer = Random.Range(walkTimeMin, walkTimeMax);

//         movement = Random.insideUnitCircle.normalized;
//     }

//     protected virtual void Update()
//     {
//         if (!isChasing)
//         {
//             idleTimer -= Time.deltaTime;

//             if (idleTimer <= 0f)
//             {
//                 walkTimer -= Time.deltaTime;
//                 if (walkTimer > 0f)
//                 {
//                     MoveCharacter(movement);
//                 }
//                 else
//                 {
//                     movement = Random.insideUnitCircle.normalized;
//                     walkTimer = Random.Range(walkTimeMin, walkTimeMax);
//                 }
//             }

//             float distanceToPlayer = Vector2.Distance(transform.position, player.position);
//             if (distanceToPlayer < chaseDistance)
//             {
//                 StartChase();
//             }
//         }
//     }

//     protected virtual void FixedUpdate()
//     {
//         if (isChasing)
//         {
//             Vector2 direction = (player.position - transform.position).normalized;
//             MoveCharacter(direction);
//         }
//     }

//     protected void MoveCharacter(Vector2 direction)
//     {
//         rb.MovePosition((Vector2)transform.position + (direction * walkSpeed * Time.fixedDeltaTime));
//     }

//     protected void StartChase()
//     {
//         isChasing = true;
//         Debug.Log("chase state started");
//         //animator.SetBool("isChasing", true);
//     }

//     protected virtual void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             AttackPlayer(other);
//         }
//     }

//     protected virtual void AttackPlayer(Collider2D playerCollider)
//     {
//         Debug.Log("attack");
//         PlayerHealth playerHealth = playerCollider.gameObject.GetComponent<PlayerHealth>();
//         if (playerHealth != null)
//         {
//             playerHealth.TakeDamage((int)attackStrength);
//             // Optionally, add specific attack animations or effects here
//         }
//     }
// }