using UnityEngine;

public class SkeletonAI : EnemyBase
{
    protected override void Awake()
    {
        maxHP = Random.Range(40, 51);
        minDamage = 12;
        maxDamage = 18;
        attackCooldown = 1f;
        base.Awake();
    }
}
