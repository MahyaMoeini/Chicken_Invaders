using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main-menu only UI. Must NOT live on a DontDestroyOnLoad object.
/// Uses visual-pack button/title art when available.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    private Texture2D titleTex;
    private Texture2D newGameTex;
    private Texture2D level1Tex;
    private Texture2D level2Tex;
    private Texture2D level3Tex;
    private Texture2D exitTex;
    private Texture2D panelTex;

    private void Awake()
    {
        titleTex = ToTexture(VisualAssets.TitleBanner);
        newGameTex = ToTexture(VisualAssets.BtnNewGame);
        level1Tex = ToTexture(VisualAssets.BtnLevel1);
        level2Tex = ToTexture(VisualAssets.BtnLevel2);
        level3Tex = ToTexture(VisualAssets.BtnLevel3);
        exitTex = ToTexture(VisualAssets.BtnExit);
        panelTex = ToTexture(VisualAssets.PanelFrame);
    }

    private void OnGUI()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            return;
        }

        float width = 420f;
        float height = 590f;
        Rect box = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

        if (panelTex != null)
        {
            GUI.DrawTexture(box, panelTex, ScaleMode.ScaleToFit);
        }
        else
        {
            GUI.Box(box, string.Empty);
        }

        float x = box.x + 50f;
        float w = width - 100f;
        float y = box.y + 20f;

        if (titleTex != null)
        {
            GUI.DrawTexture(new Rect(x, y, w, 110f), titleTex, ScaleMode.ScaleToFit);
            y += 120f;
        }
        else
        {
            GUI.Box(new Rect(x, y, w, 40f), "Chicken Invaders");
            y += 50f;
        }

        if (DrawSpriteButton(new Rect(x, y, w, 56f), newGameTex, "New Game (Level 1)"))
        {
            StartLevel(1, resetStats: true);
        }

        y += 70f;
        GUI.Label(new Rect(x, y, w, 24f), "Select Level:");
        y += 28f;

        if (DrawSpriteButton(new Rect(x, y, w, 48f), level1Tex, "Play Level 1"))
        {
            StartLevel(1, resetStats: false);
        }

        y += 56f;
        if (DrawSpriteButton(new Rect(x, y, w, 48f), level2Tex, "Play Level 2"))
        {
            StartLevel(2, resetStats: false);
        }

        y += 56f;
        if (DrawSpriteButton(new Rect(x, y, w, 48f), level3Tex, "Play Level 3"))
        {
            StartLevel(3, resetStats: false);
        }

        y += 64f;
        if (DrawSpriteButton(new Rect(x, y, w, 48f), exitTex, "Exit"))
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

    private static bool DrawSpriteButton(Rect rect, Texture2D tex, string fallback)
    {
        if (tex != null)
        {
            if (GUI.Button(rect, tex, GUIStyle.none))
            {
                return true;
            }

            return false;
        }

        return GUI.Button(rect, fallback);
    }

    private static Texture2D ToTexture(Sprite sprite)
    {
        if (sprite == null)
        {
            return null;
        }

        return sprite.texture;
    }

    private void StartLevel(int levelIndex, bool resetStats)
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(levelIndex, resetStats);
            return;
        }

        if (resetStats && GameManager.Instance != null)
        {
            GameManager.Instance.InitializeNewGame();
        }

        SceneManager.LoadScene("Level" + levelIndex);
    }
}
