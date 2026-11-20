using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Queue-based pool for a single prefab.
/// Expands automatically when empty and ignores duplicate releases.
/// </summary>
public class ObjectPool
{
    private readonly GameObject prefab;
    private readonly Transform parent;
    private readonly Action<GameObject, ObjectPool> registerInstance;
    private readonly Queue<GameObject> available = new Queue<GameObject>();
    private readonly HashSet<GameObject> availableSet = new HashSet<GameObject>();
    private readonly HashSet<GameObject> tracked = new HashSet<GameObject>();

    public GameObject Prefab => prefab;
    public int AvailableCount => available.Count;
    public int TrackedCount => tracked.Count;

    public ObjectPool(
        GameObject prefab,
        int initialSize,
        Transform parent,
        Action<GameObject, ObjectPool> registerInstance = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.registerInstance = registerInstance;

        for (int i = 0; i < initialSize; i++)
        {
            CreateAndStoreInactive();
        }
    }

    public GameObject Get()
    {
        GameObject instance = available.Count > 0 ? DequeueAvailable() : CreateInstance();

        if (instance == null)
        {
            return null;
        }

        availableSet.Remove(instance);
        instance.SetActive(true);
        NotifySpawn(instance);
        return instance;
    }

    public void Release(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        // Prevent double-release of the same object.
        if (availableSet.Contains(instance))
        {
            return;
        }

        if (!tracked.Contains(instance))
        {
            Debug.LogWarning($"ObjectPool tried to release an untracked object: {instance.name}");
            return;
        }

        NotifyReturn(instance);
        instance.SetActive(false);

        if (parent != null)
        {
            instance.transform.SetParent(parent, false);
        }

        available.Enqueue(instance);
        availableSet.Add(instance);
    }

    public void ResetPool()
    {
        List<GameObject> copy = new List<GameObject>(tracked);
        foreach (GameObject instance in copy)
        {
            if (instance != null)
            {
                Release(instance);
            }
        }
    }

    public void Clear()
    {
        foreach (GameObject instance in tracked)
        {
            if (instance != null)
            {
                UnityEngine.Object.Destroy(instance);
            }
        }

        available.Clear();
        availableSet.Clear();
        tracked.Clear();
    }

    private GameObject DequeueAvailable()
    {
        while (available.Count > 0)
        {
            GameObject candidate = available.Dequeue();
            if (candidate != null)
            {
                return candidate;
            }
        }

        return null;
    }

    private void CreateAndStoreInactive()
    {
        GameObject instance = CreateInstance();
        if (instance == null)
        {
            return;
        }

        instance.SetActive(false);
        available.Enqueue(instance);
        availableSet.Add(instance);
    }

    private GameObject CreateInstance()
    {
        if (prefab == null)
        {
            Debug.LogError("ObjectPool cannot create instances because the prefab is null.");
            return null;
        }

        GameObject instance = UnityEngine.Object.Instantiate(prefab, parent);
        instance.name = $"{prefab.name}_pooled";
        tracked.Add(instance);
        registerInstance?.Invoke(instance, this);
        return instance;
    }

    private static void NotifySpawn(GameObject instance)
    {
        IPoolable[] poolables = instance.GetComponentsInChildren<IPoolable>(true);
        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i].OnSpawnFromPool();
        }
    }

    private static void NotifyReturn(GameObject instance)
    {
        IPoolable[] poolables = instance.GetComponentsInChildren<IPoolable>(true);
        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i].OnReturnToPool();
        }
    }
}
