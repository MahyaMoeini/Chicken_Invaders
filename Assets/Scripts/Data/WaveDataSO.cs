using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reusable wave configuration. Multiple rounds can share the same asset
/// and vary difficulty through DifficultyModifiers.
/// Create assets via: Create > Chicken Invaders > Wave Data
/// </summary>
[CreateAssetMenu(fileName = "WaveData", menuName = "Chicken Invaders/Wave Data", order = 1)]
public class WaveDataSO : ScriptableObject
{
    [Header("Wave Type")]
    [SerializeField] private WaveType waveType = WaveType.Chicken;

    [Header("Enemy Setup")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 10;
    [SerializeField] private float enemyHealth = 1f;
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float spawnInterval = 0.4f;

    [Header("Egg Dropping")]
    [Tooltip("Random egg drop interval range in seconds (min / max).")]
    [SerializeField] private Vector2 eggDropIntervalRange = new Vector2(2f, 5f);

    [Header("Formation")]
    [SerializeField] private FormationType formationType = FormationType.Grid;
    [SerializeField] private float formationSpacing = 1.5f;

    [Header("Entry Path")]
    [Tooltip("World-space waypoints enemies follow when entering the screen.")]
    [SerializeField] private List<Vector3> entryWaypoints = new List<Vector3>();

    [Header("Dive Attack")]
    [SerializeField] private DiveAttackSettings diveAttack = DiveAttackSettings.Default;

    [Header("Difficulty")]
    [Tooltip("Multipliers so the same wave asset can be reused across rounds.")]
    [SerializeField] private DifficultyModifiers difficultyModifiers = DifficultyModifiers.Default;

    [Header("Boss Extension (future)")]
    [Tooltip("Optional hook for a future boss ScriptableObject. Leave empty for normal waves.")]
    [SerializeField] private ScriptableObject bossEncounterData;

    public WaveType WaveType => waveType;
    public GameObject EnemyPrefab => enemyPrefab;
    public int EnemyCount => Mathf.Max(0, Mathf.RoundToInt(enemyCount * difficultyModifiers.CountMultiplier));
    public float EnemyHealth => enemyHealth * difficultyModifiers.HealthMultiplier;
    public float MovementSpeed => movementSpeed * difficultyModifiers.SpeedMultiplier;
    public float SpawnInterval => Mathf.Max(0.05f, spawnInterval * difficultyModifiers.SpawnIntervalMultiplier);
    public Vector2 EggDropIntervalRange
    {
        get
        {
            float min = eggDropIntervalRange.x * difficultyModifiers.EggIntervalMultiplier;
            float max = eggDropIntervalRange.y * difficultyModifiers.EggIntervalMultiplier;
            return new Vector2(Mathf.Min(min, max), Mathf.Max(min, max));
        }
    }
    public FormationType FormationType => formationType;
    public float FormationSpacing => formationSpacing;
    public IReadOnlyList<Vector3> EntryWaypoints => entryWaypoints;
    public DiveAttackSettings DiveAttack => diveAttack;
    public DifficultyModifiers Difficulty => difficultyModifiers;
    public ScriptableObject BossEncounterData => bossEncounterData;
    public bool HasBossExtension => bossEncounterData != null;
}

public enum WaveType
{
    Chicken = 0,
    Asteroid = 1
}

public enum FormationType
{
    Grid = 0,
    VShape = 1,
    Compact = 2
}

[Serializable]
public struct DiveAttackSettings
{
    [Tooltip("Chance (0-1) that an idle enemy starts a dive.")]
    [Range(0f, 1f)] public float diveChance;

    [Tooltip("Seconds between dive attempt rolls.")]
    public float diveCooldown;

    [Tooltip("Speed used while diving toward the player.")]
    public float diveSpeed;

    [Tooltip("If true, diving enemies stop dropping eggs.")]
    public bool disableEggsWhileDiving;

    public static DiveAttackSettings Default => new DiveAttackSettings
    {
        diveChance = 0.15f,
        diveCooldown = 3f,
        diveSpeed = 6f,
        disableEggsWhileDiving = true
    };
}

[Serializable]
public struct DifficultyModifiers
{
    public float HealthMultiplier;
    public float SpeedMultiplier;
    public float SpawnIntervalMultiplier;
    public float EggIntervalMultiplier;
    public float CountMultiplier;

    public static DifficultyModifiers Default => new DifficultyModifiers
    {
        HealthMultiplier = 1f,
        SpeedMultiplier = 1f,
        SpawnIntervalMultiplier = 1f,
        EggIntervalMultiplier = 1f,
        CountMultiplier = 1f
    };
}
