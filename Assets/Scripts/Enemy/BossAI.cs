using UnityEngine;

public class BossAI : EnemyBase
{
    private int _phase = 1;

    protected override void Awake()
    {
        maxHP = Random.Range(500, 701);
        minDamage = 50;
        maxDamage = 80;
        detectionRange = 25f;
        attackRange = 3f;
        attackCooldown = 1.4f;
        base.Awake();
    }

    protected override void Update()
    {
        UpdatePhase();
        base.Update();
    }

    private void UpdatePhase()
    {
        var hpPercent = CurrentHP / (float)maxHP;
        var nextPhase = hpPercent <= 0.33f ? 3 : hpPercent <= 0.66f ? 2 : 1;
        if (nextPhase == _phase)
        {
            return;
        }

        _phase = nextPhase;
        switch (_phase)
        {
            case 2:
                attackCooldown = 1.1f;
                break;
            case 3:
                attackCooldown = 0.8f;
                break;
        }
    }
}
