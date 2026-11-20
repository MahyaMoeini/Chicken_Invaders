using UnityEngine;

public enum EnemyState
{
    Entering = 0,
    InFormation = 1,
    Diving = 2,
    Dead = 3
}

/// <summary>
/// Pooled chicken enemy: waypoint entry, formation idle, optional dive, egg drops.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour, IDamageable, IPoolable
{
    [Header("Defaults (overridden by WaveDataSO)")]
    [SerializeField] private float maxHealth = 2f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] [Range(0f, 1f)] private float giftDropChance = 0.2f;
    [SerializeField] private bool canDive = true;
    [SerializeField] private bool canDropEggs = true;
    [SerializeField] private GameObject eggPrefab;

    private float currentHealth;
    private bool isDead;
    private EnemyState state = EnemyState.Dead;

    private WaveSpawner owner;
    private Vector3 formationSlot;
    private Vector3[] entryPath;
    private int pathIndex;
    private float eggTimer;
    private float diveTimer;
    private DiveAttackSettings diveSettings;
    private Vector2 eggIntervalRange;
    private float diveSpeed;
    private Vector3 diveDirection = Vector3.down;

    public bool IsAlive => !isDead && currentHealth > 0f;
    public bool IsBoss => false;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public EnemyState State => state;
    public bool CountsAsActive => state != EnemyState.Dead;

    public void OnSpawnFromPool()
    {
        isDead = false;
        state = EnemyState.Entering;
        pathIndex = 0;
    }

    public void OnReturnToPool()
    {
        owner = null;
        state = EnemyState.Dead;
        isDead = false;
    }

    public void Initialize(WaveDataSO wave, Vector3 slot, WaveSpawner spawner)
    {
        owner = spawner;
        formationSlot = slot;
        maxHealth = wave.EnemyHealth;
        currentHealth = maxHealth;
        moveSpeed = wave.MovementSpeed;
        eggIntervalRange = wave.EggDropIntervalRange;
        diveSettings = wave.DiveAttack;
        diveSpeed = diveSettings.diveSpeed;
        diveTimer = diveSettings.diveCooldown;
        eggTimer = Random.Range(eggIntervalRange.x, eggIntervalRange.y);
        isDead = false;
        state = EnemyState.Entering;
        pathIndex = 0;
        entryPath = BuildEntryPath(wave, slot);
        transform.position = entryPath.Length > 0 ? entryPath[0] : slot + Vector3.up * 6f;
    }

    private void Update()
    {
        if (isDead || state == EnemyState.Dead)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.State.Lives <= 0)
        {
            return;
        }

        switch (state)
        {
            case EnemyState.Entering:
                UpdateEntering();
                break;
            case EnemyState.InFormation:
                UpdateInFormation();
                break;
            case EnemyState.Diving:
                UpdateDiving();
                break;
        }
    }

    private void UpdateEntering()
    {
        if (entryPath == null || entryPath.Length == 0)
        {
            ArriveInFormation();
            return;
        }

        Vector3 target = pathIndex < entryPath.Length ? entryPath[pathIndex] : formationSlot;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            pathIndex++;
            if (pathIndex >= entryPath.Length)
            {
                ArriveInFormation();
            }
        }
    }

    private void ArriveInFormation()
    {
        transform.position = formationSlot;
        state = EnemyState.InFormation;
        diveTimer = diveSettings.diveCooldown;
    }

    private void UpdateInFormation()
    {
        transform.position = Vector3.MoveTowards(transform.position, formationSlot, moveSpeed * Time.deltaTime);

        if (canDropEggs)
        {
            TickEggs();
        }

        if (!canDive)
        {
            return;
        }

        diveTimer -= Time.deltaTime;
        if (diveTimer > 0f)
        {
            return;
        }

        diveTimer = diveSettings.diveCooldown;
        if (Random.value <= diveSettings.diveChance)
        {
            BeginDive();
        }
    }

    private void BeginDive()
    {
        state = EnemyState.Diving;
        Transform player = FindPlayerTransform();
        Vector3 target = player != null ? player.position : transform.position + Vector3.down * 8f;
        diveDirection = (target - transform.position).normalized;
        if (diveDirection.sqrMagnitude < 0.01f)
        {
            diveDirection = Vector3.down;
        }
    }

    private void UpdateDiving()
    {
        transform.position += diveDirection * (diveSpeed * Time.deltaTime);

        // Dive chickens never drop eggs.
        if (transform.position.y < -8f)
        {
            LeaveWithoutDying();
        }
    }

    private void TickEggs()
    {
        // Diving chickens never drop eggs.
        if (state == EnemyState.Diving)
        {
            return;
        }

        eggTimer -= Time.deltaTime;
        if (eggTimer > 0f || eggPrefab == null || PoolManager.Instance == null)
        {
            return;
        }

        eggTimer = Random.Range(eggIntervalRange.x, eggIntervalRange.y);
        GameObject egg = PoolManager.Instance.Get(eggPrefab);
        if (egg != null)
        {
            egg.transform.position = transform.position + Vector3.down * 0.4f;
            EggProjectile projectile = egg.GetComponent<EggProjectile>();
            projectile?.Launch();
        }
    }

    public void ApplyDamage(float amount)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        currentHealth -= amount;
        if (currentHealth > 0f)
        {
            return;
        }

        currentHealth = 0f;
        Die(awardScore: true);
    }

    private void Die(bool awardScore)
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        state = EnemyState.Dead;

        if (awardScore && GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        VfxSpawner.Explosion(transform.position, 0.85f);
        VfxSpawner.FeatherBurst(transform.position);
        WeaponGiftSpawner.TrySpawn(transform.position, giftDropChance);
        owner?.NotifyEnemyResolved(this, died: true);
        ReturnToPool();
    }

    private void LeaveWithoutDying()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        state = EnemyState.Dead;
        owner?.NotifyEnemyResolved(this, died: false);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Release(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || other == null)
        {
            return;
        }

        // Contact damage while diving (and also if player flies into formation).
        if (other.CompareTag("Player") || other.GetComponent<PlayerHealth>() != null)
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
            health?.TakeDamage();
        }
    }

    private static Transform FindPlayerTransform()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }

    private static Vector3[] BuildEntryPath(WaveDataSO wave, Vector3 slot)
    {
        if (wave.EntryWaypoints != null && wave.EntryWaypoints.Count > 0)
        {
            Vector3[] custom = new Vector3[wave.EntryWaypoints.Count + 1];
            for (int i = 0; i < wave.EntryWaypoints.Count; i++)
            {
                custom[i] = wave.EntryWaypoints[i];
            }

            custom[custom.Length - 1] = slot;
            return custom;
        }

        // Default curved path from above the screen into the formation slot.
        Vector3 start = new Vector3(slot.x + Mathf.Sign(slot.x + 0.01f) * -3f, 7.5f, 0f);
        Vector3 mid = new Vector3(slot.x * 0.4f, 4.5f, 0f);
        Vector3 bend = new Vector3(slot.x + 1.2f, 3.2f, 0f);
        return new[] { start, mid, bend, slot };
    }
}
