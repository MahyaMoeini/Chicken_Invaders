using UnityEngine;

/// <summary>
/// Simple health target for combat testing and future enemies/eggs/asteroids.
/// Bosses mark IsBoss so bombs deal percent damage instead of an instant kill.
/// </summary>
public class DamageableTarget : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private bool isBoss;
    [SerializeField] private bool overrideGiftDropChance;
    [SerializeField] [Range(0f, 1f)] private float giftDropChance = 0.25f;
    [SerializeField] private bool releaseToPoolOnDeath;

    private float currentHealth;
    private bool isDead;

    public bool IsAlive => !isDead && currentHealth > 0f;
    public bool IsBoss => isBoss;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void OnSpawnFromPool()
    {
        ResetHealth();
    }

    public void OnReturnToPool()
    {
        isDead = false;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
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

        if (!isBoss)
        {
            float chance = overrideGiftDropChance
                ? giftDropChance
                : WeaponGiftSpawner.DefaultDropChance;
            WeaponGiftSpawner.TrySpawn(transform.position, chance);
        }

        if (releaseToPoolOnDeath && PoolManager.Instance != null)
        {
            PoolManager.Instance.Release(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
