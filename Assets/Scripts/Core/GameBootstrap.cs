using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ensures persistent managers exist and gameplay time is never left frozen.
/// </summary>
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetTimeScale()
    {
        // Fixes frozen gameplay when Enter Play Mode Options disables Domain Reload
        // after a previous pause left Time.timeScale at 0.
        Time.timeScale = 1f;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsurePersistentSystems()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance == null)
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }

        if (LevelManager.Instance == null)
        {
            new GameObject("LevelManager").AddComponent<LevelManager>();
        }

        if (PauseMenuController.Instance == null)
        {
            new GameObject("PauseMenu").AddComponent<PauseMenuController>();
        }
        else
        {
            PauseMenuController.Instance.Resume();
        }

        if (GameOverMenuController.Instance == null)
        {
            new GameObject("GameOverMenu").AddComponent<GameOverMenuController>();
        }
        else if (GameOverMenuController.Instance.IsGameOver)
        {
            GameOverMenuController.Instance.Hide();
        }

        if (VictoryMenuController.Instance == null)
        {
            new GameObject("VictoryMenu").AddComponent<VictoryMenuController>();
        }
        else if (VictoryMenuController.Instance.IsVictory)
        {
            VictoryMenuController.Instance.Hide();
        }

        Scene active = SceneManager.GetActiveScene();
        if (active.IsValid() && active.name == "SampleScene")
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
