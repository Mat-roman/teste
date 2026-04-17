using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float basicAttackCooldown = 1f;
    [SerializeField] private float heavyAttackCooldown = 3f;
    [SerializeField] private float fireballCooldown = 2f;
    [SerializeField] private float frostNovaCooldown = 2.5f;
    [SerializeField] private float healCooldown = 5f;
    [SerializeField] private float hitRange = 2.4f;
    [SerializeField] private float blockStaminaPerSecond = 10f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private ParticleSystem fireballEffect;
    [SerializeField] private ParticleSystem frostNovaEffect;
    [SerializeField] private ParticleSystem healEffect;

    private PlayerStats _stats;
    private float _nextBasic;
    private float _nextHeavy;
    private float _nextFireball;
    private float _nextFrostNova;
    private float _nextHeal;
    private bool _isBlocking;

    private void Awake()
    {
        _stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        HandleBlock();

        if (Input.GetMouseButtonDown(0))
        {
            TryBasicAttack();
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryHeavyAttack();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TryFireball();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TryFrostNova();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TryHeal();
        }
    }

    public void ReceiveDamage(int amount)
    {
        var final = _isBlocking ? Mathf.RoundToInt(amount * 0.4f) : amount;
        _stats.TakeDamage(final);
    }

    private void HandleBlock()
    {
        _isBlocking = Input.GetKey(KeyCode.Q);
        if (_isBlocking)
        {
            _stats.SpendStamina(Mathf.CeilToInt(blockStaminaPerSecond * Time.deltaTime));
            if (_stats.Stamina <= 0)
            {
                _isBlocking = false;
            }
        }
    }

    private void TryBasicAttack()
    {
        if (Time.time < _nextBasic)
        {
            return;
        }

        _nextBasic = Time.time + basicAttackCooldown;
        var damage = _stats.CalculateOutgoingDamage(10, 20);
        DealRaycastDamage(damage, hitRange);
    }

    private void TryHeavyAttack()
    {
        if (Time.time < _nextHeavy)
        {
            return;
        }

        _nextHeavy = Time.time + heavyAttackCooldown;
        var damage = _stats.CalculateOutgoingDamage(30, 50);
        DealRaycastDamage(damage, hitRange + 0.4f);
    }

    private void TryFireball()
    {
        if (Time.time < _nextFireball || !_stats.SpendMana(30))
        {
            return;
        }

        _nextFireball = Time.time + fireballCooldown;
        PlayEffect(fireballEffect);
        var damage = _stats.CalculateOutgoingDamage(25, 35);
        DealRaycastDamage(damage, 12f);
    }

    private void TryFrostNova()
    {
        if (Time.time < _nextFrostNova || !_stats.SpendMana(25))
        {
            return;
        }

        _nextFrostNova = Time.time + frostNovaCooldown;
        PlayEffect(frostNovaEffect);
        StartCoroutine(ApplyFrostNova());
    }

    private IEnumerator ApplyFrostNova()
    {
        var colliders = Physics.OverlapSphere(transform.position, 5f, hitMask);
        foreach (var col in colliders)
        {
            if (col.TryGetComponent<EnemyBase>(out var enemy))
            {
                enemy.TakeDamage(_stats.CalculateOutgoingDamage(15, 25));
                enemy.ApplyFreeze(2f);
            }
        }

        yield return null;
    }

    private void TryHeal()
    {
        if (Time.time < _nextHeal || !_stats.SpendMana(40))
        {
            return;
        }

        _nextHeal = Time.time + healCooldown;
        _stats.Heal(UnityEngine.Random.Range(50, 101));
        PlayEffect(healEffect);
    }

    private void DealRaycastDamage(int amount, float range)
    {
        var ray = new Ray(Camera.main != null ? Camera.main.transform.position : transform.position + Vector3.up, transform.forward);
        if (Physics.Raycast(ray, out var hit, range, hitMask) && hit.collider.TryGetComponent<EnemyBase>(out var enemy))
        {
            enemy.TakeDamage(amount);
        }
    }

    private static void PlayEffect(ParticleSystem fx)
    {
        if (fx != null)
        {
            fx.Play();
        }
    }
}
