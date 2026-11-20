using UnityEditor;
using UnityEngine;

/// <summary>
/// Menu helpers for the visual pack integration.
/// </summary>
public static class ApplyVisualPack
{
    [MenuItem("Chicken Invaders/Reimport Visual Pack Sprites")]
    public static void Reimport()
    {
        AssetDatabase.ImportAsset("Assets/Art", ImportAssetOptions.ImportRecursive);
        AssetDatabase.ImportAsset("Assets/Resources/ChickenArt", ImportAssetOptions.ImportRecursive);
        AssetDatabase.Refresh();
        Debug.Log("Visual pack sprites reimported. Play MainMenu to verify art/HUD/VFX.");
    }

    [MenuItem("Chicken Invaders/Ping Boss Sprites (future)")]
    public static void PingBosses()
    {
        Object normal = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Enemies/normal/boss.png");
        Object armored = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Enemies/armored/boss.png");
        Object alien = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Enemies/alien/boss.png");
        Debug.Log($"Boss sprites ready: normal={normal != null}, armored={armored != null}, alien={alien != null}");
        if (normal != null)
        {
            EditorGUIUtility.PingObject(normal);
        }
    }
}
