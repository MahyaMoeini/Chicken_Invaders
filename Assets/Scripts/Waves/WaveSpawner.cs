using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns one wave from WaveDataSO and tracks pending + active enemies/asteroids.
/// </summary>
public class WaveSpawner : MonoBehaviour
{
    private WaveDataSO currentWave;
    private int pendingSpawns;
    private readonly HashSet<EnemyController> activeEnemies = new HashSet<EnemyController>();
    private readonly HashSet<AsteroidController> activeAsteroids = new HashSet<AsteroidController>();
    private Coroutine spawnRoutine;
    private bool spawningComplete;
    private bool stopped;

    public int PendingSpawns => pendingSpawns;
    public int ActiveCount => activeEnemies.Count + activeAsteroids.Count;
    public bool IsWaveFullyResolved => spawningComplete && pendingSpawns <= 0 && ActiveCount <= 0;

    public void StopSpawning()
    {
        stopped = true;
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public void BeginWave(WaveDataSO wave)
    {
        StopSpawning();
        stopped = false;
        currentWave = wave;
        activeEnemies.Clear();
        activeAsteroids.Clear();
        spawningComplete = false;

        if (wave == null || wave.EnemyPrefab == null)
        {
            pendingSpawns = 0;
            spawningComplete = true;
            return;
        }

        pendingSpawns = wave.EnemyCount;
        spawnRoutine = StartCoroutine(SpawnRoutine(wave));
    }

    private IEnumerator SpawnRoutine(WaveDataSO wave)
    {
        if (wave.WaveType == WaveType.Asteroid)
        {
            yield return SpawnAsteroids(wave);
        }
        else
        {
            yield return SpawnChickens(wave);
        }

        spawningComplete = true;
        spawnRoutine = null;
    }

    private IEnumerator SpawnChickens(WaveDataSO wave)
    {
        Vector3 center = new Vector3(0f, 2.5f, 0f);
        Vector3[] slots = FormationUtility.BuildSlots(
            wave.FormationType,
            wave.EnemyCount,
            wave.FormationSpacing,
            center);

        for (int i = 0; i < wave.EnemyCount; i++)
        {
            if (stopped || !CanContinue())
            {
                pendingSpawns = 0;
                yield break;
            }

            Vector3 slot = i < slots.Length ? slots[i] : center;
            GameObject instance = PoolManager.Instance != null
                ? PoolManager.Instance.Get(wave.EnemyPrefab)
                : null;

            if (instance != null)
            {
                EnemyController enemy = instance.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.Initialize(wave, slot, this);
                    activeEnemies.Add(enemy);
                }
            }

            pendingSpawns = Mathf.Max(0, pendingSpawns - 1);
            if (i < wave.EnemyCount - 1)
            {
                yield return new WaitForSeconds(wave.SpawnInterval);
            }
        }
    }

    private IEnumerator SpawnAsteroids(WaveDataSO wave)
    {
        Camera cam = Camera.main;
        float halfWidth = cam != null && cam.orthographic ? cam.orthographicSize * cam.aspect : 5f;

        for (int i = 0; i < wave.EnemyCount; i++)
        {
            if (stopped || !CanContinue())
            {
                pendingSpawns = 0;
                yield break;
            }

            float x = Random.Range(-halfWidth + 0.5f, halfWidth - 0.5f);
            Vector3 spawnPos = new Vector3(x, 7.5f, 0f);
            GameObject instance = PoolManager.Instance != null
                ? PoolManager.Instance.Get(wave.EnemyPrefab)
                : null;

            if (instance != null)
            {
                AsteroidController asteroid = instance.GetComponent<AsteroidController>();
                if (asteroid != null)
                {
                    asteroid.Initialize(wave, this, spawnPos);
                    activeAsteroids.Add(asteroid);
                }
            }

            pendingSpawns = Mathf.Max(0, pendingSpawns - 1);
            if (i < wave.EnemyCount - 1)
            {
                yield return new WaitForSeconds(wave.SpawnInterval);
            }
        }
    }

    public void NotifyEnemyResolved(EnemyController enemy, bool died)
    {
        activeEnemies.Remove(enemy);
    }

    public void NotifyAsteroidResolved(AsteroidController asteroid, bool died)
    {
        activeAsteroids.Remove(asteroid);
    }

    private static bool CanContinue()
    {
        return GameManager.Instance == null || GameManager.Instance.State.Lives > 0;
    }
}
