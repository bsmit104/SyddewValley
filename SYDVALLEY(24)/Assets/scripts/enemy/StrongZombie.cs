using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrongZombie : Enemy
{
    public float increasedAttackStrength = 20.0f; // Increased attack strength for this type

    protected override void Start()
    {
        base.Start();
        attackStrength = increasedAttackStrength; // Set the attack strength for this type
    }

    protected override void AttackPlayer()
    {
        // Custom attack behavior for strong zombie
        Debug.Log("Attacking player strongly with strength: " + attackStrength);
        // Implement strong zombie attack logic here
    }
}