#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Registers all game scenes in Build Settings and sets MainMenu as the play/start scene.
/// </summary>
[InitializeOnLoad]
public static class EnsureBuildScenes
{
    private static readonly string[] RequiredScenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/Level1.unity",
        "Assets/Scenes/Level2.unity",
        "Assets/Scenes/Level3.unity"
    };

    static EnsureBuildScenes()
    {
        EditorApplication.delayCall += Apply;
    }

    [MenuItem("Chicken Invaders/Fix Build Settings Scenes")]
    public static void Apply()
    {
        foreach (string path in RequiredScenes)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("Missing scene file: " + path);
                return;
            }
        }

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        for (int i = 0; i < RequiredScenes.Length; i++)
        {
            scenes.Add(new EditorBuildSettingsScene(RequiredScenes[i], true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();

        // Make Play button open MainMenu.
        SceneAsset mainMenu = AssetDatabase.LoadAssetAtPath<SceneAsset>(RequiredScenes[0]);
        if (mainMenu != null)
        {
            EditorSceneManager.playModeStartScene = mainMenu;
        }

        Debug.Log("Build Settings fixed. Play Mode start scene = MainMenu. Scenes: MainMenu, Level1, Level2, Level3.");
    }
}
#endif
