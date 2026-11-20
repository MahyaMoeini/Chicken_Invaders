using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Applies level backgrounds, parallax, HUD, and screen shake when a level loads.
/// </summary>
public class LevelVisualBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Hook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string name = scene.name;
        if (name != "Level1" && name != "Level2" && name != "Level3")
        {
            return;
        }

        EnsureShake();
        ApplyBackground(name);
        GameplayHudUI.EnsureExists();
    }

    private static void EnsureShake()
    {
        if (Camera.main != null && Camera.main.GetComponent<ScreenShake>() == null)
        {
            Camera.main.gameObject.AddComponent<ScreenShake>();
        }
    }

    private static void ApplyBackground(string sceneName)
    {
        Sprite bg = sceneName switch
        {
            "Level1" => VisualAssets.BgLevel1,
            "Level2" => VisualAssets.BgLevel2,
            "Level3" => VisualAssets.BgLevel3,
            _ => null
        };

        if (bg == null)
        {
            // Fallback: keep whatever sprite is already on the scene Background object.
            bg = null;
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        GameObject background = GameObject.Find("Background");
        if (background == null)
        {
            background = new GameObject("Background");
            background.AddComponent<SpriteRenderer>();
        }

        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = background.AddComponent<SpriteRenderer>();
        }

        if (bg != null)
        {
            sr.sprite = bg;
        }

        sr.sortingOrder = -50;
        sr.color = Color.white;

        if (cam != null)
        {
            background.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 10f);
        }
        else
        {
            background.transform.position = new Vector3(0f, 0f, 10f);
        }

        // Fit to camera.
        if (cam != null && cam.orthographic && sr.sprite != null)
        {
            Bounds b = sr.sprite.bounds;
            float worldHeight = cam.orthographicSize * 2f;
            float worldWidth = worldHeight * cam.aspect;
            float scaleY = worldHeight / Mathf.Max(0.01f, b.size.y);
            float scaleX = worldWidth / Mathf.Max(0.01f, b.size.x);
            float s = Mathf.Max(scaleX, scaleY) * 1.1f;
            background.transform.localScale = new Vector3(s, s, 1f);
        }

        ParallaxBackground parallax = background.GetComponent<ParallaxBackground>();
        if (parallax == null)
        {
            background.AddComponent<ParallaxBackground>();
        }
        else if (bg != null)
        {
            parallax.SetSprite(bg);
        }
    }
}
