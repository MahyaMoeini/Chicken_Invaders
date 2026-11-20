using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent scene flow. Lives on its own DontDestroyOnLoad object (not the menu).
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private LevelDataSO[] levels = new LevelDataSO[3];
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float levelCompleteDisplaySeconds = 2f;

    private static readonly string[] LevelSceneNames = { "Level1", "Level2", "Level3" };

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep ONLY this manager object alive across scenes.
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameEvents.OnLevelCompleted += HandleLevelCompleted;
    }

    private void OnDisable()
    {
        GameEvents.OnLevelCompleted -= HandleLevelCompleted;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void StartNewGame()
    {
        LoadLevel(1, resetStats: true);
    }

    /// <summary>
    /// After Game Over: reload the same level from round 1 with fresh lives/stats.
    /// </summary>
    public void RestartCurrentLevel()
    {
        int level = 1;
        if (GameManager.Instance != null)
        {
            level = Mathf.Clamp(GameManager.Instance.State.CurrentLevel, 1, 3);
        }

        isTransitioning = false;
        LoadLevel(level, resetStats: true);
    }

    public void LoadLevel(int levelIndex, bool resetStats = false)
    {
        if (isTransitioning)
        {
            return;
        }

        if (levelIndex < 1 || levelIndex > 3)
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }

        if (resetStats && GameManager.Instance != null)
        {
            GameManager.Instance.InitializeNewGame();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLevel(levelIndex);
            GameManager.Instance.SetRound(1);
        }

        if (GameOverMenuController.Instance != null && GameOverMenuController.Instance.IsGameOver)
        {
            GameOverMenuController.Instance.Hide();
        }

        if (VictoryMenuController.Instance != null && VictoryMenuController.Instance.IsVictory)
        {
            VictoryMenuController.Instance.Hide();
        }

        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused)
        {
            PauseMenuController.Instance.Resume();
        }

        string sceneName = GetSceneNameForLevel(levelIndex);
        LoadSceneByName(sceneName);
    }

    private string GetSceneNameForLevel(int levelIndex)
    {
        LevelDataSO data = GetLevelByIndex(levelIndex);
        if (data != null && !string.IsNullOrEmpty(data.SceneName))
        {
            return data.SceneName;
        }

        return LevelSceneNames[levelIndex - 1];
    }

    private void HandleLevelCompleted(int levelIndex)
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(TransitionAfterLevel(levelIndex));
    }

    private IEnumerator TransitionAfterLevel(int completedLevelIndex)
    {
        isTransitioning = true;
        PoolManager.Instance?.ResetAllPools();

        // Designed stage-complete banner (GameplayHudUI) shows during this wait.
        yield return new WaitForSecondsRealtime(levelCompleteDisplaySeconds);

        if (GameManager.Instance != null && GameManager.Instance.State.Lives <= 0)
        {
            isTransitioning = false;
            yield break;
        }

        if (completedLevelIndex >= 3)
        {
            GameEvents.RaiseGameCompleted();
            isTransitioning = false;
            yield break;
        }

        int nextIndex = completedLevelIndex + 1;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLevel(nextIndex);
            GameManager.Instance.SetRound(1);
        }

        LoadSceneByName(GetSceneNameForLevel(nextIndex));
        isTransitioning = false;
    }

    private void LoadSceneByName(string sceneName)
    {
        PoolManager.Instance?.ResetAllPools();

        if (GameOverMenuController.Instance != null && GameOverMenuController.Instance.IsGameOver)
        {
            GameOverMenuController.Instance.Hide();
        }

        if (VictoryMenuController.Instance != null && VictoryMenuController.Instance.IsVictory)
        {
            VictoryMenuController.Instance.Hide();
        }

        if (PauseMenuController.Instance != null)
        {
            PauseMenuController.Instance.Resume();
        }

        Time.timeScale = 1f;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"Scene '{sceneName}' is not in Build Settings. " +
                "Use menu: Chicken Invaders > Fix Build Settings Scenes");
            return;
        }

        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public LevelDataSO GetLevelByIndex(int levelIndex)
    {
        if (levels == null)
        {
            return null;
        }

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null && levels[i].LevelIndex == levelIndex)
            {
                return levels[i];
            }
        }

        int arrayIndex = levelIndex - 1;
        if (arrayIndex >= 0 && arrayIndex < levels.Length)
        {
            return levels[arrayIndex];
        }

        return null;
    }

    public void ReturnToMainMenu()
    {
        if (isTransitioning)
        {
            return;
        }

        LoadSceneByName(mainMenuSceneName);
    }
}
