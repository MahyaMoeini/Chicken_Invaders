using UnityEngine;

/// <summary>
/// Fires pooled projectiles using the current GameState weapon power level.
/// </summary>
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerWeapon : MonoBehaviour
{
    [Header("Weapon configs (index 0 = power 1)")]
    [SerializeField] private WeaponDataSO[] weaponLevels = new WeaponDataSO[3];

    [SerializeField] private Transform firePoint;

    private PlayerInputReader inputReader;
    private float fireCooldown;

    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
        if (firePoint == null)
        {
            firePoint = transform;
        }

        // Prevent player lasers from colliding with the ship or pickups.
        int playerLayer = LayerMask.NameToLayer("Player");
        int bulletLayer = LayerMask.NameToLayer("PlayerBullet");
        int pickupLayer = LayerMask.NameToLayer("Pickup");
        if (playerLayer >= 0 && bulletLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, bulletLayer, true);
        }

        if (bulletLayer >= 0 && pickupLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(bulletLayer, pickupLayer, true);
        }
    }

    private void Update()
    {
        if (fireCooldown > 0f)
        {
            fireCooldown -= Time.deltaTime;
        }

        if (!CanFire())
        {
            return;
        }

        if (inputReader != null && inputReader.IsFireHeld)
        {
            TryFire();
        }
    }

    private bool CanFire()
    {
        return GameManager.Instance != null
               && GameManager.Instance.State.Lives > 0
               && inputReader != null
               && inputReader.CanReadInput;
    }

    private void TryFire()
    {
        WeaponDataSO data = GetCurrentWeaponData();
        if (data == null || data.ProjectilePrefab == null || PoolManager.Instance == null)
        {
            return;
        }

        if (fireCooldown > 0f)
        {
            return;
        }

        float interval = data.FireRate > 0f ? 1f / data.FireRate : 0.25f;
        fireCooldown = interval;

        int count = Mathf.Max(1, data.ProjectileCount);
        for (int i = 0; i < count; i++)
        {
            float offsetX = GetIndexed(data.ProjectileOffsets, i, 0f);
            float angle = GetIndexed(data.SpreadAngles, i, 0f);
            SpawnProjectile(data, offsetX, angle);
        }
    }

    private void SpawnProjectile(WeaponDataSO data, float offsetX, float angleDegrees)
    {
        GameObject instance = PoolManager.Instance.Get(data.ProjectilePrefab);
        if (instance == null)
        {
            return;
        }

        Vector3 spawnPos = firePoint.position + firePoint.right * offsetX;
        Quaternion rotation = firePoint.rotation * Quaternion.Euler(0f, 0f, angleDegrees);
        instance.transform.SetPositionAndRotation(spawnPos, rotation);

        Projectile projectile = instance.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(data.Damage, data.ProjectileSpeed);
        }

        SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            int power = GameManager.Instance != null ? GameManager.Instance.State.WeaponPowerLevel : 1;
            Sprite laser = power >= 3 ? VisualAssets.Laser3 : power >= 2 ? VisualAssets.Laser2 : VisualAssets.Laser1;
            if (laser != null)
            {
                sr.sprite = laser;
            }
        }
    }

    private WeaponDataSO GetCurrentWeaponData()
    {
        if (weaponLevels == null || weaponLevels.Length == 0 || GameManager.Instance == null)
        {
            return null;
        }

        int power = Mathf.Clamp(GameManager.Instance.State.WeaponPowerLevel, 1, 3);
        int index = Mathf.Clamp(power - 1, 0, weaponLevels.Length - 1);
        return weaponLevels[index];
    }

    private static float GetIndexed(float[] values, int index, float fallback)
    {
        if (values == null || values.Length == 0)
        {
            return fallback;
        }

        return values[Mathf.Clamp(index, 0, values.Length - 1)];
    }
}
