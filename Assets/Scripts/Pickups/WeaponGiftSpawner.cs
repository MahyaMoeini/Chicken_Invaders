using UnityEngine;

/// <summary>
/// Scene service that spawns pooled weapon gifts with a configurable drop chance.
/// Place one in the test/gameplay scene and assign the gift prefab.
/// </summary>
public class WeaponGiftSpawner : MonoBehaviour
{
    public static WeaponGiftSpawner Instance { get; private set; }

    [SerializeField] private GameObject giftPrefab;
    [SerializeField] [Range(0f, 1f)] private float defaultDropChance = 0.25f;

    public static float DefaultDropChance =>
        Instance != null ? Instance.defaultDropChance : 0.25f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static void TrySpawn(Vector3 position, float chance)
    {
        if (Instance == null || Instance.giftPrefab == null || PoolManager.Instance == null)
        {
            return;
        }

        if (Random.value > Mathf.Clamp01(chance))
        {
            return;
        }

        GameObject gift = PoolManager.Instance.Get(Instance.giftPrefab);
        if (gift != null)
        {
            gift.transform.position = position;
        }
    }

    /// <summary>
    /// Manual/test helper to force-spawn a gift at a world position.
    /// </summary>
    public void SpawnAt(Vector3 position)
    {
        if (giftPrefab == null || PoolManager.Instance == null)
        {
            return;
        }

        GameObject gift = PoolManager.Instance.Get(giftPrefab);
        if (gift != null)
        {
            gift.transform.position = position;
        }
    }
}
