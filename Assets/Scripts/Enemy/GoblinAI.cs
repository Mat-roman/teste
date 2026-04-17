using UnityEngine;

public class GoblinAI : EnemyBase
{
    protected override void Awake()
    {
        maxHP = Random.Range(20, 31);
        minDamage = 5;
        maxDamage = 10;
        attackCooldown = 0.8f;
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
        if (CurrentHP > 0 && CurrentHP < maxHP * 0.3f && Player != null)
        {
            var fleeDir = (transform.position - Player.position).normalized;
            transform.position += fleeDir * Time.deltaTime * 3f;
        }
    }
}
