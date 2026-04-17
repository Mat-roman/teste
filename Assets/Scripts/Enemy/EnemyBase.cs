using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] protected int maxHP = 50;
    [SerializeField] protected int minDamage = 5;
    [SerializeField] protected int maxDamage = 10;
    [SerializeField] protected float detectionRange = 12f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackCooldown = 1.2f;

    protected int CurrentHP;
    protected Transform Player;
    protected NavMeshAgent Agent;
    protected Animator Anim;
    protected float NextAttack;
    private float _frozenUntil;

    protected virtual void Awake()
    {
        CurrentHP = maxHP;
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponentInChildren<Animator>();
    }

    protected virtual void Start()
    {
        var playerStats = FindObjectOfType<PlayerStats>();
        Player = playerStats == null ? null : playerStats.transform;
    }

    protected virtual void Update()
    {
        if (Player == null || CurrentHP <= 0)
        {
            return;
        }

        if (Time.time < _frozenUntil)
        {
            if (Agent != null)
            {
                Agent.isStopped = true;
            }

            return;
        }

        var distance = Vector3.Distance(transform.position, Player.position);
        if (distance > detectionRange)
        {
            return;
        }

        if (distance > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            TryAttack();
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentHP -= amount;
        var rb = GetComponent<Rigidbody>();
        if (rb != null && Player != null)
        {
            var dir = (transform.position - Player.position).normalized;
            rb.AddForce(dir * 2f, ForceMode.Impulse);
        }

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    public virtual void ApplyFreeze(float seconds)
    {
        _frozenUntil = Mathf.Max(_frozenUntil, Time.time + Mathf.Max(0f, seconds));
    }

    protected virtual void MoveTowardsPlayer()
    {
        if (Agent == null)
        {
            transform.position = Vector3.MoveTowards(transform.position, Player.position, Time.deltaTime * 2.5f);
            return;
        }

        Agent.isStopped = false;
        Agent.SetDestination(Player.position);
    }

    protected virtual void TryAttack()
    {
        if (Time.time < NextAttack)
        {
            return;
        }

        NextAttack = Time.time + attackCooldown;
        var damage = Random.Range(minDamage, maxDamage + 1);
        var combat = Player.GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.ReceiveDamage(damage);
        }
        else
        {
            var stats = Player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(damage);
            }
        }
    }

    protected virtual void Die()
    {
        DropLoot();
        Destroy(gameObject);
    }

    protected virtual void DropLoot()
    {
    }
}
