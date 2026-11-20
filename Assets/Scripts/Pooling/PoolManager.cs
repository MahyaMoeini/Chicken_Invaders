using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages multiple ObjectPool instances keyed by prefab.
/// Place one PoolManager in a bootstrap / gameplay scene.
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Serializable]
    public class PoolConfig
    {
        public GameObject prefab;
        public int initialSize = 10;
    }

    [SerializeField] private List<PoolConfig> prewarmPools = new List<PoolConfig>();

    private readonly Dictionary<GameObject, ObjectPool> poolsByPrefab = new Dictionary<GameObject, ObjectPool>();
    private readonly Dictionary<GameObject, ObjectPool> poolsByInstance = new Dictionary<GameObject, ObjectPool>();
    private Transform poolsRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Scene-local pools so leftover gameplay objects do not survive level loads.

        poolsRoot = new GameObject("Pools").transform;
        poolsRoot.SetParent(transform, false);

        for (int i = 0; i < prewarmPools.Count; i++)
        {
            PoolConfig config = prewarmPools[i];
            if (config.prefab != null)
            {
                CreatePool(config.prefab, Mathf.Max(0, config.initialSize));
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public ObjectPool CreatePool(GameObject prefab, int initialSize)
    {
        if (prefab == null)
        {
            Debug.LogError("Cannot create a pool for a null prefab.");
            return null;
        }

        if (poolsByPrefab.TryGetValue(prefab, out ObjectPool existing))
        {
            return existing;
        }

        Transform poolParent = new GameObject($"{prefab.name}_Pool").transform;
        poolParent.SetParent(poolsRoot, false);

        ObjectPool pool = new ObjectPool(prefab, initialSize, poolParent, RegisterInstance);
        poolsByPrefab.Add(prefab, pool);
        return pool;
    }

    public GameObject Get(GameObject prefab)
    {
        ObjectPool pool = GetOrCreatePool(prefab, 0);
        GameObject instance = pool != null ? pool.Get() : null;
        if (instance != null)
        {
            PrefabVisualApplier.Apply(instance);
        }

        return instance;
    }

    public T Get<T>(GameObject prefab) where T : Component
    {
        GameObject instance = Get(prefab);
        return instance != null ? instance.GetComponent<T>() : null;
    }

    public void Release(GameObject prefab, GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        if (prefab != null && poolsByPrefab.TryGetValue(prefab, out ObjectPool pool))
        {
            pool.Release(instance);
            return;
        }

        Release(instance);
    }

    /// <summary>
    /// Returns an instance to the pool that created it.
    /// </summary>
    public void Release(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        if (poolsByInstance.TryGetValue(instance, out ObjectPool pool))
        {
            pool.Release(instance);
            return;
        }

        Debug.LogWarning($"PoolManager could not find a pool for '{instance.name}'. Destroying it.");
        Destroy(instance);
    }

    public void ResetAllPools()
    {
        foreach (ObjectPool pool in poolsByPrefab.Values)
        {
            pool.ResetPool();
        }
    }

    public void ClearAllPools()
    {
        foreach (ObjectPool pool in poolsByPrefab.Values)
        {
            pool.Clear();
        }

        poolsByPrefab.Clear();
        poolsByInstance.Clear();
    }

    private void RegisterInstance(GameObject instance, ObjectPool pool)
    {
        poolsByInstance[instance] = pool;
    }

    private ObjectPool GetOrCreatePool(GameObject prefab, int initialSize)
    {
        if (prefab == null)
        {
            return null;
        }

        if (poolsByPrefab.TryGetValue(prefab, out ObjectPool pool))
        {
            return pool;
        }

        return CreatePool(prefab, initialSize);
    }
}
