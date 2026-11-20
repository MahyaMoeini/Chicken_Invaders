using UnityEngine;

/// <summary>
/// Falling weapon gift. Collecting it raises weapon power by one (max 3).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WeaponGiftPickup : MonoBehaviour, IPoolable
{
    [SerializeField] private float fallSpeed = 2.5f;
    [SerializeField] private float despawnY = -8f;

    private bool collected;
    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    public void OnSpawnFromPool()
    {
        collected = false;
        if (col != null)
        {
            col.enabled = true;
        }
    }

    public void OnReturnToPool()
    {
        collected = true;
    }

    private void Update()
    {
        if (collected)
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
        if (collected || other == null)
        {
            return;
        }

        if (!other.CompareTag("Player") && other.GetComponent<PlayerHealth>() == null)
        {
            return;
        }

        Collect();
    }

    private void Collect()
    {
        collected = true;
        if (col != null)
        {
            col.enabled = false;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.IncreaseWeaponPower();
        }

        Despawn();
    }

    private void Despawn()
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
