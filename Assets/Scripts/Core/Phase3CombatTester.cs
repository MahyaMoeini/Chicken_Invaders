using UnityEngine;

/// <summary>
/// Optional SampleScene helper. Disable or remove this component for production builds.
/// Spawns a test gift and can force weapon power for verifying all fire patterns.
/// </summary>
public class Phase3CombatTester : MonoBehaviour
{
    [SerializeField] private bool enableTester = true;
    [SerializeField] private bool spawnGiftOnStart = true;
    [SerializeField] private Vector3 giftSpawnPosition = new Vector3(-2f, 3.5f, 0f);
    [SerializeField] [Range(1, 3)] private int startingWeaponPower = 1;

    private void Start()
    {
        if (!enableTester)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetWeaponPower(startingWeaponPower);
        }

        if (spawnGiftOnStart && WeaponGiftSpawner.Instance != null)
        {
            WeaponGiftSpawner.Instance.SpawnAt(giftSpawnPosition);
        }
    }
}
