using UnityEngine;

/// <summary>
/// Pooled player laser. Damages IDamageable enemies once, then returns to the pool.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float despawnY = 8f;

    private float damage = 1f;
    private float speed = 12f;
    private bool hasHit;
    private Rigidbody2D rb;
    private Collider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    public void Initialize(float damageAmount, float moveSpeed)
    {
        damage = damageAmount;
        speed = moveSpeed;
        hasHit = false;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = transform.up * speed;
        }
    }

    public void OnSpawnFromPool()
    {
        hasHit = false;
        if (col != null)
        {
            col.enabled = true;
        }
    }

    public void OnReturnToPool()
    {
        hasHit = true;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Update()
    {
        if (hasHit)
        {
            return;
        }

        // Fallback movement if no Rigidbody2D is present.
        if (rb == null)
        {
            transform.position += transform.up * (speed * Time.deltaTime);
        }

        if (transform.position.y > despawnY)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || other == null)
        {
            return;
        }

        // Ignore the player and other player projectiles / gifts.
        if (other.CompareTag("Player") || other.GetComponent<Projectile>() != null)
        {
            return;
        }

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null || !damageable.IsAlive)
        {
            return;
        }

        hasHit = true;
        if (col != null)
        {
            col.enabled = false;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            VfxSpawner.LaserBloom(transform.position, sr.sprite);
        }

        damageable.ApplyDamage(damage);
        Despawn();
    }

    private void Despawn()
    {
        hasHit = true;
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
