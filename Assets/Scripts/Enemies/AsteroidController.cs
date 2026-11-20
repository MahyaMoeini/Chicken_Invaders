using UnityEngine;

/// <summary>
/// Pooled asteroid that falls from above, damages the player, and is destructible.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AsteroidController : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private int scoreValue = 50;
    [SerializeField] private float despawnY = -8f;

    private float currentHealth;
    private bool isDead;
    private WaveSpawner owner;
    private float moveSpeed;

    public bool IsAlive => !isDead && currentHealth > 0f;
    public bool IsBoss => false;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool CountsAsActive => !isDead;

    public void OnSpawnFromPool()
    {
        ResetHealth();
    }

    public void OnReturnToPool()
    {
        owner = null;
        isDead = false;
    }

    public void Initialize(WaveDataSO wave, WaveSpawner spawner, Vector3 spawnPosition)
    {
        owner = spawner;
        maxHealth = wave.EnemyHealth;
        moveSpeed = wave.MovementSpeed > 0f ? wave.MovementSpeed : fallSpeed;
        transform.position = spawnPosition;
        ResetHealth();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        transform.position += Vector3.down * (moveSpeed * Time.deltaTime);
        if (transform.position.y < despawnY)
        {
            LeaveWithoutDying();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || other == null)
        {
            return;
        }

        if (other.CompareTag("Player") || other.GetComponent<PlayerHealth>() != null)
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
            health?.TakeDamage();
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
        Die();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        VfxSpawner.Explosion(transform.position, 0.7f);
        owner?.NotifyAsteroidResolved(this, died: true);
        ReturnToPool();
    }

    private void LeaveWithoutDying()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        owner?.NotifyAsteroidResolved(this, died: false);
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
}
