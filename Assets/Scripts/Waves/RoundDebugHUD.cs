using UnityEngine;

/// <summary>
/// Minimal on-screen debug for level/round progress.
/// </summary>
public class RoundDebugHUD : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private bool showGui = true;

    private void Awake()
    {
        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<WaveManager>();
        }
    }

    private void OnGUI()
    {
        // Prefer the TMP gameplay HUD when present.
        if (GameplayHudUI.Instance != null)
        {
            return;
        }

        if (!showGui || GameManager.Instance == null)
        {
            return;
        }

        GameState state = GameManager.Instance.State;
        int pending = waveManager != null ? waveManager.PendingSpawns : 0;
        int active = waveManager != null ? waveManager.ActiveEnemies : 0;

        string text =
            $"Level: {state.CurrentLevel}   Round: {state.CurrentRound}\n" +
            $"Pending: {pending}   Active: {active}\n" +
            $"Score: {state.Score}\n" +
            $"Lives: {state.Lives}   Bombs: {state.BombCount}   Power: {state.WeaponPowerLevel}";

        GUI.Box(new Rect(12, 12, 300, 100), text);
    }
}
