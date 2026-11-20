using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Escape toggles pause in gameplay levels. Uses visual-pack button art when available.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;
    private bool pauseAllowed;
    private InputAction pauseAction;
    private Texture2D resumeTex;
    private Texture2D mainMenuTex;
    private Texture2D exitTex;
    private Texture2D panelTex;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        isPaused = false;
        Time.timeScale = 1f;

        pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        pauseAction.performed += _ => TogglePause();
        pauseAction.Enable();

        SceneManager.sceneLoaded += HandleSceneLoaded;
        EvaluateScene(SceneManager.GetActiveScene());

        resumeTex = SpriteToTex(VisualAssets.BtnResume);
        mainMenuTex = SpriteToTex(VisualAssets.BtnMainMenu);
        exitTex = SpriteToTex(VisualAssets.BtnExit);
        panelTex = SpriteToTex(VisualAssets.PanelFrame);
    }

    private void OnEnable()
    {
        if (!isPaused)
        {
            Time.timeScale = 1f;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (pauseAction != null)
        {
            pauseAction.Disable();
            pauseAction.Dispose();
            pauseAction = null;
        }

        if (Instance == this)
        {
            Instance = null;
        }

        Time.timeScale = 1f;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Resume();
        EvaluateScene(scene);
    }

    private void EvaluateScene(Scene scene)
    {
        string name = scene.name;
        pauseAllowed = name == "Level1" || name == "Level2" || name == "Level3";
    }

    private void TogglePause()
    {
        if (!pauseAllowed)
        {
            return;
        }

        if (GameOverMenuController.Instance != null && GameOverMenuController.Instance.IsGameOver)
        {
            return;
        }

        if (VictoryMenuController.Instance != null && VictoryMenuController.Instance.IsVictory)
        {
            return;
        }

        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        if (!pauseAllowed || isPaused)
        {
            return;
        }

        if (GameOverMenuController.Instance != null && GameOverMenuController.Instance.IsGameOver)
        {
            return;
        }

        if (VictoryMenuController.Instance != null && VictoryMenuController.Instance.IsVictory)
        {
            return;
        }

        isPaused = true;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    public void GoToMainMenu()
    {
        Resume();
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
        if (!isPaused)
        {
            return;
        }

        float width = 360f;
        float height = 310f;
        Rect box = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        if (panelTex != null)
        {
            GUI.DrawTexture(box, panelTex, ScaleMode.ScaleToFit);
        }
        else
        {
            GUI.Box(box, "Paused  (Esc)");
        }

        float x = box.x + 40f;
        float w = width - 80f;

        if (DrawButton(new Rect(x, box.y + 50f, w, 50f), resumeTex, "Resume"))
        {
            Resume();
        }

        if (DrawButton(new Rect(x, box.y + 115f, w, 50f), mainMenuTex, "Main Menu"))
        {
            GoToMainMenu();
        }

        if (DrawButton(new Rect(x, box.y + 180f, w, 50f), exitTex, "Exit"))
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
