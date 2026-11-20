using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ensures pooled/spawned gameplay objects get visual-pack sprites and sizes.
/// </summary>
public class PoolVisualHook : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void HookExisting()
    {
        ApplyAllInScene();

        // Re-apply one frame later (covers prefab instance init order).
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            return;
        }

        GameObject runner = new GameObject("VisualSizeReapply");
        runner.AddComponent<VisualSizeReapplyOnce>();
    }

    public static void ApplyAllInScene()
    {
        SpriteRenderer[] renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < renderers.Length; i++)
        {
            PrefabVisualApplier.Apply(renderers[i].gameObject);
        }
    }
}

public class VisualSizeReapplyOnce : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return null;
        PoolVisualHook.ApplyAllInScene();
        Destroy(gameObject);
    }
}
