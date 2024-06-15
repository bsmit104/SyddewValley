using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float idleTimeMin = 1.0f;
    public float idleTimeMax = 3.0f;
    public float walkTimeMin = 1.0f;
    public float walkTimeMax = 3.0f;
    public float walkSpeed = 2.0f;
    public float chaseDistance = 5.0f;
    public float attackStrength = 10.0f; // Default attack strength for all enemies
    public Transform player;

    protected Rigidbody2D rb;
    //protected Animator animator;
    protected Vector2 movement;
    protected float idleTimer;
    protected float walkTimer;
    protected bool isChasing = false;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //animator = GetComponent<Animator>();

        idleTimer = Random.Range(idleTimeMin, idleTimeMax);
        walkTimer = Random.Range(walkTimeMin, walkTimeMax);

        movement = Random.insideUnitCircle.normalized;
    }

    protected virtual void Update()
    {
        if (!isChasing)
        {
            idleTimer -= Time.deltaTime;

            if (idleTimer <= 0f)
            {
                walkTimer -= Time.deltaTime;
                if (walkTimer > 0f)
                {
                    MoveCharacter(movement);
                }
                else
                {
                    movement = Random.insideUnitCircle.normalized;
                    walkTimer = Random.Range(walkTimeMin, walkTimeMax);
                }
            }

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer < chaseDistance)
            {
                StartChase();
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        if (isChasing)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            MoveCharacter(direction);
        }
    }

    protected void MoveCharacter(Vector2 direction)
    {
        rb.MovePosition((Vector2)transform.position + (direction * walkSpeed * Time.fixedDeltaTime));
    }

    protected void StartChase()
    {
        isChasing = true;
        Debug.Log("chase state started");
        //animator.SetBool("isChasing", true);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AttackPlayer(other);
        }
    }

    protected virtual void AttackPlayer(Collider2D playerCollider)
    {
        Debug.Log("attack");
        PlayerHealth playerHealth = playerCollider.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage((int)attackStrength);
            // Optionally, add specific attack animations or effects here
        }
    }
}