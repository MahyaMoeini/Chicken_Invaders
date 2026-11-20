using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows Game Over when lives reach zero. Retry restarts round 1 of the same level.
/// </summary>
public class GameOverMenuController : MonoBehaviour
{
    public static GameOverMenuController Instance { get; private set; }

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isGameOver;
    private bool uiAllowed;
    private Texture2D bannerTex;
    private Texture2D retryTex;
    private Texture2D mainMenuTex;
    private Texture2D exitTex;

    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        isGameOver = false;
        Time.timeScale = 1f;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        EvaluateScene(SceneManager.GetActiveScene());

        RefreshButtonTextures();
    }

    private void RefreshButtonTextures()
    {
        bannerTex = SpriteToTex(VisualAssets.BannerGameOver);
        retryTex = SpriteToTex(VisualAssets.BtnRetry) ?? SpriteToTex(VisualAssets.BtnMainMenu);
        mainMenuTex = SpriteToTex(VisualAssets.BtnMainMenu);
        exitTex = SpriteToTex(VisualAssets.BtnExit);
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDied -= HandlePlayerDied;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (Instance == this)
        {
            Instance = null;
        }

        if (isGameOver)
        {
            Time.timeScale = 1f;
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Hide();
        EvaluateScene(scene);
    }

    private void EvaluateScene(Scene scene)
    {
        string name = scene.name;
        uiAllowed = name == "Level1" || name == "Level2" || name == "Level3";
    }

    private void HandlePlayerDied()
    {
        if (!uiAllowed || isGameOver)
        {
            return;
        }

        Show();
    }

    public void Show()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused)
        {
            PauseMenuController.Instance.Resume();
        }

        RefreshButtonTextures();
        isGameOver = true;
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        isGameOver = false;
        Time.timeScale = 1f;
    }

    public void PlayAgain()
    {
        int level = 1;
        if (GameManager.Instance != null)
        {
            level = Mathf.Max(1, GameManager.Instance.State.CurrentLevel);
        }

        Hide();
        PoolManager.Instance?.ResetAllPools();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartCurrentLevel();
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitializeNewGame();
            GameManager.Instance.SetLevel(level);
            GameManager.Instance.SetRound(1);
        }

        SceneManager.LoadScene("Level" + level);
    }

    public void GoToMainMenu()
    {
        Hide();
        PoolManager.Instance?.ResetAllPools();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ReturnToMainMenu();
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private void OnGUI()
    {
        if (!isGameOver || !uiAllowed)
        {
            return;
        }

        float width = 420f;
        float height = 370f;
        Rect box = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

        if (bannerTex != null)
        {
            GUI.DrawTexture(new Rect(box.x, box.y, width, 90f), bannerTex, ScaleMode.ScaleToFit);
        }
        else
        {
            GUI.Box(new Rect(box.x, box.y, width, 50f), "Game Over");
        }

        float x = box.x + 50f;
        float w = width - 100f;
        int level = GameManager.Instance != null ? GameManager.Instance.State.CurrentLevel : 1;
        int score = GameManager.Instance != null ? GameManager.Instance.State.Score : 0;

        GUIStyle infoStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
        infoStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x, box.y + 100f, w, 36f), $"Level {level}    Score {score}", infoStyle);

        if (DrawButton(new Rect(x, box.y + 145f, w, 50f), retryTex, "Retry"))
        {
            PlayAgain();
        }

        if (DrawButton(new Rect(x, box.y + 210f, w, 50f), mainMenuTex, "Main Menu"))
        {
            GoToMainMenu();
        }

        if (DrawButton(new Rect(x, box.y + 275f, w, 50f), exitTex, "Exit"))
        {
            QuitGame();
        }
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static bool DrawButton(Rect rect, Texture2D tex, string fallback)
    {
        if (tex != null)
        {
            return GUI.Button(rect, tex, GUIStyle.none);
        }

        return GUI.Button(rect, fallback);
    }

    private static Texture2D SpriteToTex(Sprite sprite)
    {
        return sprite != null ? sprite.texture : null;
    }
}
