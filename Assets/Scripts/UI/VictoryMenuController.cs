using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows Victory when all levels are cleared. Play Again starts a new game from Level 1.
/// </summary>
public class VictoryMenuController : MonoBehaviour
{
    public static VictoryMenuController Instance { get; private set; }

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isVictory;
    private bool uiAllowed;
    private Texture2D bannerTex;
    private Texture2D playAgainTex;
    private Texture2D mainMenuTex;
    private Texture2D exitTex;

    public bool IsVictory => isVictory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        isVictory = false;
        Time.timeScale = 1f;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        EvaluateScene(SceneManager.GetActiveScene());
        RefreshButtonTextures();
    }

    private void RefreshButtonTextures()
    {
        bannerTex = SpriteToTex(VisualAssets.BannerVictory);
        playAgainTex = SpriteToTex(VisualAssets.BtnNewGame) ?? SpriteToTex(VisualAssets.BtnRetry);
        mainMenuTex = SpriteToTex(VisualAssets.BtnMainMenu);
        exitTex = SpriteToTex(VisualAssets.BtnExit);
    }

    private void OnEnable()
    {
        GameEvents.OnGameCompleted += HandleGameCompleted;
    }

    private void OnDisable()
    {
        GameEvents.OnGameCompleted -= HandleGameCompleted;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (Instance == this)
        {
            Instance = null;
        }

        if (isVictory)
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

    private void HandleGameCompleted()
    {
        if (!uiAllowed || isVictory)
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
        isVictory = true;
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        isVictory = false;
        Time.timeScale = 1f;
    }

    public void PlayAgain()
    {
        Hide();
        PoolManager.Instance?.ResetAllPools();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartNewGame();
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitializeNewGame();
            GameManager.Instance.SetLevel(1);
            GameManager.Instance.SetRound(1);
        }

        SceneManager.LoadScene("Level1");
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
        if (!isVictory || !uiAllowed)
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
            GUI.Box(new Rect(box.x, box.y, width, 50f), "Victory!");
        }

        float x = box.x + 50f;
        float w = width - 100f;
        int score = GameManager.Instance != null ? GameManager.Instance.State.Score : 0;

        GUIStyle infoStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
        infoStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x, box.y + 100f, w, 36f), $"Score {score}", infoStyle);

        if (DrawButton(new Rect(x, box.y + 145f, w, 50f), playAgainTex, "Play Again"))
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
