using UnityEngine;

/// <summary>
/// Optional Play Mode helper for Phase 1 manual verification.
/// Attach temporarily to any GameObject, enter Play Mode, then remove when done.
/// </summary>
public class Phase1ArchitectureTester : MonoBehaviour
{
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private GameObject poolTestPrefab;
    [SerializeField] private int poolGetCount = 5;

    private void Start()
    {
        if (runOnStart)
        {
            RunChecks();
        }
    }

    [ContextMenu("Run Phase 1 Checks")]
    public void RunChecks()
    {
        Debug.Log("=== Phase 1 Architecture Checks ===");

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null. Add a GameManager to the scene.");
            return;
        }

        GameEvents.ClearAll();

        int livesEvents = 0;
        int scoreEvents = 0;
        int bombEvents = 0;
        int powerEvents = 0;
        int roundEvents = 0;
        int levelEvents = 0;
        int deathEvents = 0;

        GameEvents.OnLivesChanged += _ => livesEvents++;
        GameEvents.OnScoreChanged += _ => scoreEvents++;
        GameEvents.OnBombCountChanged += _ => bombEvents++;
        GameEvents.OnWeaponPowerChanged += _ => powerEvents++;
        GameEvents.OnRoundChanged += _ => roundEvents++;
        GameEvents.OnLevelChanged += _ => levelEvents++;
        GameEvents.OnPlayerDied += () => deathEvents++;

        GameManager gm = GameManager.Instance;
        gm.InitializeNewGame();

        GameState state = gm.State;
        bool defaultsOk =
            state.CurrentLevel == 1 &&
            state.CurrentRound == 1 &&
            state.Lives == 3 &&
            state.Score == 0 &&
            state.BombCount == 2 &&
            state.WeaponPowerLevel == 1;

        Debug.Log(defaultsOk
            ? "PASS: Default GameState values"
            : $"FAIL: Unexpected defaults L{state.CurrentLevel} R{state.CurrentRound} lives={state.Lives} score={state.Score} bombs={state.BombCount} power={state.WeaponPowerLevel}");

        gm.AddScore(250);
        gm.SetWeaponPower(99);
        gm.UseBomb();
        gm.SetLives(0);

        bool clampsOk =
            gm.State.Score == 250 &&
            gm.State.WeaponPowerLevel == 3 &&
            gm.State.BombCount == 1 &&
            gm.State.Lives == 0 &&
            deathEvents == 1;

        Debug.Log(clampsOk
            ? "PASS: Clamps + score/bomb/death events"
            : $"FAIL: clamps/events score={gm.State.Score} power={gm.State.WeaponPowerLevel} bombs={gm.State.BombCount} lives={gm.State.Lives} deaths={deathEvents}");

        bool eventsOk =
            scoreEvents > 0 &&
            powerEvents > 0 &&
            bombEvents > 0 &&
            livesEvents > 0 &&
            levelEvents > 0 &&
            roundEvents > 0;

        Debug.Log(eventsOk
            ? "PASS: GameEvents fired for state changes"
            : $"FAIL: event counts score={scoreEvents} power={powerEvents} bomb={bombEvents} lives={livesEvents} level={levelEvents} round={roundEvents}");

        if (PoolManager.Instance == null)
        {
            Debug.LogWarning("PoolManager.Instance is null. Skipping pool checks.");
            return;
        }

        if (poolTestPrefab == null)
        {
            Debug.LogWarning("Assign a Pool Test Prefab to verify pooling.");
            return;
        }

        ObjectPool pool = PoolManager.Instance.CreatePool(poolTestPrefab, 2);
        int beforeTracked = pool.TrackedCount;

        GameObject a = PoolManager.Instance.Get(poolTestPrefab);
        GameObject b = PoolManager.Instance.Get(poolTestPrefab);
        GameObject c = PoolManager.Instance.Get(poolTestPrefab); // forces expand

        bool activeOk = a.activeInHierarchy && b.activeInHierarchy && c.activeInHierarchy;
        PoolManager.Instance.Release(a);
        PoolManager.Instance.Release(a); // duplicate release must be ignored
        PoolManager.Instance.Release(b);
        PoolManager.Instance.Release(c);

        GameObject reused = PoolManager.Instance.Get(poolTestPrefab);
        bool reuseOk = reused == a || reused == b || reused == c;
        bool expandOk = pool.TrackedCount >= beforeTracked && pool.TrackedCount >= 3;

        Debug.Log(activeOk && reuseOk && expandOk
            ? $"PASS: Object pooling (tracked={pool.TrackedCount}, available={pool.AvailableCount})"
            : $"FAIL: pooling activeOk={activeOk} reuseOk={reuseOk} expandOk={expandOk} tracked={pool.TrackedCount}");

        PoolManager.Instance.Release(reused);
        Debug.Log("=== Phase 1 checks finished ===");
    }
}
