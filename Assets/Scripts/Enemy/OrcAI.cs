using UnityEngine;

public class OrcAI : EnemyBase
{
    protected override void Awake()
    {
        maxHP = Random.Range(60, 81);
        minDamage = 15;
        maxDamage = 25;
        attackCooldown = 1.6f;
        base.Awake();
    }
}
