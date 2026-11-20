using UnityEngine;

/// <summary>
/// Placed in each level scene to guarantee gameplay can start.
/// </summary>
public class LevelSceneController : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (PauseMenuController.Instance != null)
        {
            PauseMenuController.Instance.Resume();
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<WaveManager>();
        }

        // If auto-start already ran, this is harmless; if levelData failed earlier, try again.
        if (waveManager != null && waveManager.ActiveEnemies == 0 && waveManager.PendingSpawns == 0)
        {
            waveManager.StartLevel();
        }
    }
}
