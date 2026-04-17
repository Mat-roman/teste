using UnityEngine;

public class DragonAI : EnemyBase
{
    protected override void Awake()
    {
        maxHP = Random.Range(150, 201);
        minDamage = 30;
        maxDamage = 50;
        detectionRange = 20f;
        attackRange = 6f;
        attackCooldown = 1.8f;
        base.Awake();
    }
}
