using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastZombie : Enemy
{
    public float increasedWalkSpeed = 4.0f;

    protected override void Start()
    {
        base.Start();
        walkSpeed = increasedWalkSpeed;
    }

    protected override void AttackPlayer(Collider2D playerCollider)
    {
        PlayerHealth playerHealth = playerCollider.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage((int)attackStrength);
            // Optionally, add specific attack animations or effects for fast zombie
        }
    }
}