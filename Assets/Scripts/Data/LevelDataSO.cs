using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data-driven configuration for a single game level / scene.
/// Create assets via: Create > Chicken Invaders > Level Data
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "Chicken Invaders/Level Data", order = 0)]
public class LevelDataSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("1-based level index used by GameManager and save data.")]
    [SerializeField] private int levelIndex = 1;

    [Tooltip("Exact scene name passed to SceneManager.LoadScene in a later phase.")]
    [SerializeField] private string sceneName = "Level1";

    [Header("Presentation")]
    [Tooltip("Visual theme label (e.g. Earth Orbit, Asteroid Belt, Deep Space).")]
    [SerializeField] private string environmentTheme = "Earth Orbit";

    [Tooltip("Optional background sprite or prefab for this level.")]
    [SerializeField] private Sprite background;

    [Tooltip("Background music clip reserved for the audio phase.")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Waves")]
    [Tooltip("Ordered wave configurations played in this level.")]
    [SerializeField] private List<WaveDataSO> waves = new List<WaveDataSO>();

    public int LevelIndex => levelIndex;
    public string SceneName => sceneName;
    public string EnvironmentTheme => environmentTheme;
    public Sprite Background => background;
    public AudioClip BackgroundMusic => backgroundMusic;
    public IReadOnlyList<WaveDataSO> Waves => waves;
}
