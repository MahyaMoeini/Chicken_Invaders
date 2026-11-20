using System;

/// <summary>
/// Central static event bus for gameplay state changes.
/// UI, audio, and VFX systems can subscribe later without coupling to GameManager internals.
/// </summary>
public static class GameEvents
{
    public static event Action<int> OnLivesChanged;
    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnBombCountChanged;
    public static event Action<int> OnWeaponPowerChanged;
    public static event Action<int> OnRoundChanged;
    public static event Action<int> OnLevelChanged;
    public static event Action OnPlayerDied;
    public static event Action<int> OnRoundStarted;
    public static event Action<int> OnRoundCompleted;
    public static event Action<int> OnLevelCompleted;
    public static event Action OnGameCompleted;

    public static void RaiseLivesChanged(int lives) => OnLivesChanged?.Invoke(lives);
    public static void RaiseScoreChanged(int score) => OnScoreChanged?.Invoke(score);
    public static void RaiseBombCountChanged(int bombs) => OnBombCountChanged?.Invoke(bombs);
    public static void RaiseWeaponPowerChanged(int power) => OnWeaponPowerChanged?.Invoke(power);
    public static void RaiseRoundChanged(int round) => OnRoundChanged?.Invoke(round);
    public static void RaiseLevelChanged(int level) => OnLevelChanged?.Invoke(level);
    public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
    public static void RaiseRoundStarted(int round) => OnRoundStarted?.Invoke(round);
    public static void RaiseRoundCompleted(int round) => OnRoundCompleted?.Invoke(round);
    public static void RaiseLevelCompleted(int level) => OnLevelCompleted?.Invoke(level);
    public static void RaiseGameCompleted() => OnGameCompleted?.Invoke();

    /// <summary>
    /// Clears all subscribers. Useful when resetting play mode or tearing down systems.
    /// </summary>
    public static void ClearAll()
    {
        OnLivesChanged = null;
        OnScoreChanged = null;
        OnBombCountChanged = null;
        OnWeaponPowerChanged = null;
        OnRoundChanged = null;
        OnLevelChanged = null;
        OnPlayerDied = null;
        OnRoundStarted = null;
        OnRoundCompleted = null;
        OnLevelCompleted = null;
        OnGameCompleted = null;
    }
}
