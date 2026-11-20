using UnityEngine;

/// <summary>
/// Persistent runtime owner of GameState.
/// Survives scene loads via DontDestroyOnLoad and prevents duplicate instances.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState gameState = new GameState();

    public GameState State => gameState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure defaults if the component was added in the editor with empty values.
        if (gameState == null)
        {
            gameState = new GameState();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Resets progress to the assignment defaults and notifies listeners.
    /// </summary>
    public void InitializeNewGame()
    {
        gameState.ResetToDefaults();
        // Force bombs from current DefaultBombs (scene-serialized values can be stale).
        gameState.SetBombCount(GameState.DefaultBombs);
        PublishFullState();
    }

    public void SetLevel(int level)
    {
        if (gameState.CurrentLevel == level && level >= 1)
        {
            return;
        }

        gameState.SetLevel(level);
        GameEvents.RaiseLevelChanged(gameState.CurrentLevel);
    }

    public void SetRound(int round)
    {
        if (gameState.CurrentRound == round && round >= 1)
        {
            return;
        }

        gameState.SetRound(round);
        GameEvents.RaiseRoundChanged(gameState.CurrentRound);
    }

    public void AddScore(int amount)
    {
        if (amount == 0)
        {
            return;
        }

        gameState.AddScore(amount);
        GameEvents.RaiseScoreChanged(gameState.Score);
    }

    public void SetScore(int score)
    {
        if (gameState.Score == score)
        {
            return;
        }

        gameState.SetScore(score);
        GameEvents.RaiseScoreChanged(gameState.Score);
    }

    public void SetLives(int lives)
    {
        int previous = gameState.Lives;
        gameState.SetLives(lives);

        if (gameState.Lives != previous)
        {
            GameEvents.RaiseLivesChanged(gameState.Lives);
        }

        if (previous > 0 && gameState.Lives <= 0)
        {
            GameEvents.RaisePlayerDied();
        }
    }

    public void LoseLife()
    {
        SetLives(gameState.Lives - 1);
    }

    public void SetBombCount(int bombs)
    {
        if (gameState.BombCount == bombs)
        {
            return;
        }

        gameState.SetBombCount(bombs);
        GameEvents.RaiseBombCountChanged(gameState.BombCount);
    }

    /// <summary>
    /// Consumes one bomb if available. Returns true when a bomb was used.
    /// </summary>
    public bool TryUseBomb()
    {
        if (gameState.BombCount <= 0)
        {
            return false;
        }

        SetBombCount(gameState.BombCount - 1);
        return true;
    }

    public void UseBomb()
    {
        TryUseBomb();
    }

    public void SetWeaponPower(int power)
    {
        int previous = gameState.WeaponPowerLevel;
        gameState.SetWeaponPowerLevel(power);

        if (gameState.WeaponPowerLevel != previous)
        {
            GameEvents.RaiseWeaponPowerChanged(gameState.WeaponPowerLevel);
        }
    }

    public void IncreaseWeaponPower()
    {
        SetWeaponPower(gameState.WeaponPowerLevel + 1);
    }

    public void DecreaseWeaponPower()
    {
        SetWeaponPower(gameState.WeaponPowerLevel - 1);
    }

    /// <summary>
    /// Applies an already-validated state snapshot (future Continue / load path).
    /// </summary>
    public void ApplyState(GameState source, bool publishEvents = true)
    {
        if (source == null)
        {
            return;
        }

        gameState = source.Clone();

        if (publishEvents)
        {
            PublishFullState();
        }
    }

    private void PublishFullState()
    {
        GameEvents.RaiseLevelChanged(gameState.CurrentLevel);
        GameEvents.RaiseRoundChanged(gameState.CurrentRound);
        GameEvents.RaiseLivesChanged(gameState.Lives);
        GameEvents.RaiseScoreChanged(gameState.Score);
        GameEvents.RaiseBombCountChanged(gameState.BombCount);
        GameEvents.RaiseWeaponPowerChanged(gameState.WeaponPowerLevel);
    }
}
