using System.Collections;
using UnityEngine;

/// <summary>
/// Drives ordered rounds from a LevelDataSO using WaveSpawner.
/// </summary>
[RequireComponent(typeof(WaveSpawner))]
public class WaveManager : MonoBehaviour
{
    [SerializeField] private LevelDataSO levelData;
    [SerializeField] private float delayBetweenRounds = 2f;
    [SerializeField] private bool autoStart = true;

    private WaveSpawner spawner;
    private int currentRoundIndex;
    private bool levelRunning;
    private bool waitingForRoundClear;
    private Coroutine advanceRoutine;

    public int CurrentRoundNumber => currentRoundIndex + 1;
    public int ActiveEnemies => spawner != null ? spawner.ActiveCount : 0;
    public int PendingSpawns => spawner != null ? spawner.PendingSpawns : 0;
    public LevelDataSO LevelData => levelData;

    private void Awake()
    {
        spawner = GetComponent<WaveSpawner>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDied -= HandlePlayerDied;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        if (autoStart)
        {
            StartLevel();
        }
    }

    private void Update()
    {
        if (!levelRunning || waitingForRoundClear || spawner == null)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.State.Lives <= 0)
        {
            StopLevel();
            return;
        }

        if (spawner.IsWaveFullyResolved)
        {
            waitingForRoundClear = true;
            advanceRoutine = StartCoroutine(CompleteRoundAndAdvance());
        }
    }

    public void StartLevel()
    {
        if (levelData == null || levelData.Waves == null || levelData.Waves.Count == 0)
        {
            Debug.LogWarning("WaveManager: assign a LevelDataSO with waves.");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLevel(levelData.LevelIndex);
            GameManager.Instance.SetRound(1);
        }

        levelRunning = true;
        currentRoundIndex = 0;
        StartRound(currentRoundIndex);
    }

    public void StopLevel()
    {
        levelRunning = false;
        waitingForRoundClear = false;
        spawner?.StopSpawning();
        if (advanceRoutine != null)
        {
            StopCoroutine(advanceRoutine);
            advanceRoutine = null;
        }
    }

    private void StartRound(int index)
    {
        if (levelData == null || index < 0 || index >= levelData.Waves.Count)
        {
            return;
        }

        waitingForRoundClear = false;
        WaveDataSO wave = levelData.Waves[index];
        int roundNumber = index + 1;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetRound(roundNumber);
        }

        GameEvents.RaiseRoundStarted(roundNumber);
        Debug.Log($"[Wave] Round {roundNumber} started ({wave.name}, {wave.WaveType}).");
        spawner.BeginWave(wave);
    }

    private IEnumerator CompleteRoundAndAdvance()
    {
        int finishedRound = CurrentRoundNumber;
        GameEvents.RaiseRoundCompleted(finishedRound);
        Debug.Log($"[Wave] Round {finishedRound} completed.");

        if (currentRoundIndex >= levelData.Waves.Count - 1)
        {
            levelRunning = false;
            GameEvents.RaiseLevelCompleted(levelData.LevelIndex);
            Debug.Log($"[Wave] Level {levelData.LevelIndex} completed.");
            advanceRoutine = null;
            yield break;
        }

        yield return new WaitForSeconds(delayBetweenRounds);

        if (!levelRunning || (GameManager.Instance != null && GameManager.Instance.State.Lives <= 0))
        {
            advanceRoutine = null;
            yield break;
        }

        currentRoundIndex++;
        StartRound(currentRoundIndex);
        advanceRoutine = null;
    }

    private void HandlePlayerDied()
    {
        StopLevel();
    }
}
