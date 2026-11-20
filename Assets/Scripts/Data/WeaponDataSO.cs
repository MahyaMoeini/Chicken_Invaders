using UnityEngine;

/// <summary>
/// Data-driven weapon configuration used by the combat system in a later phase.
/// Create assets via: Create > Chicken Invaders > Weapon Data
/// </summary>
[CreateAssetMenu(fileName = "WeaponData", menuName = "Chicken Invaders/Weapon Data", order = 2)]
public class WeaponDataSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string weaponName = "Laser";
    [SerializeField] private Sprite weaponSprite;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float projectileSpeed = 12f;

    [Header("Firing")]
    [Tooltip("Shots per second.")]
    [SerializeField] private float fireRate = 4f;

    [Tooltip("Power level this configuration represents (1-3).")]
    [Range(1, 3)]
    [SerializeField] private int powerLevel = 1;

    [Tooltip("How many projectiles spawn per shot.")]
    [SerializeField] private int projectileCount = 1;

    [Tooltip("Local X offsets for each projectile (length should match projectile count).")]
    [SerializeField] private float[] projectileOffsets = { 0f };

    [Tooltip("Spread angles in degrees for each projectile.")]
    [SerializeField] private float[] spreadAngles = { 0f };

    public string WeaponName => weaponName;
    public Sprite WeaponSprite => weaponSprite;
    public GameObject ProjectilePrefab => projectilePrefab;
    public float Damage => damage;
    public float ProjectileSpeed => projectileSpeed;
    public float FireRate => fireRate;
    public int PowerLevel => powerLevel;
    public int ProjectileCount => projectileCount;
    public float[] ProjectileOffsets => projectileOffsets;
    public float[] SpreadAngles => spreadAngles;
}
