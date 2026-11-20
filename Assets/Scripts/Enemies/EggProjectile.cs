using UnityEngine;

/// <summary>
/// Pooled enemy egg. Falls down, damages the player, and can be destroyed by bullets/bombs.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EggProjectile : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] private float fallSpeed = 3.5f;
    [SerializeField] private float despawnY = -8f;
    [SerializeField] private float maxHealth = 1f;

    private float currentHealth;
    private bool isDead;

    public bool IsAlive => !isDead && currentHealth > 0f;
    public bool IsBoss => false;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public void OnSpawnFromPool()
    {
        ResetEgg();
    }

    public void OnReturnToPool()
    {
        isDead = false;
    }

    public void Launch()
    {
        ResetEgg();
    }

    private void ResetEgg()
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

        transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
        if (transform.position.y < despawnY)
        {
            Despawn();
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
            Despawn();
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
        Despawn();
    }

    private void Despawn()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        VfxSpawner.EggBreak(transform.position);
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
