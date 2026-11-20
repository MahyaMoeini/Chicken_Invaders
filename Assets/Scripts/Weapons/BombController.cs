using UnityEngine;

/// <summary>
/// Screen-clear bomb bound to the Bomb input action (X).
/// Instant-kills normal chickens/eggs; bosses take a percent of max health.
/// </summary>
[RequireComponent(typeof(PlayerInputReader))]
public class BombController : MonoBehaviour
{
    [SerializeField] [Range(0.15f, 0.2f)] private float bossDamagePercent = 0.175f;
    [SerializeField] private float normalKillDamage = 9999f;

    private PlayerInputReader inputReader;

    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
    }

    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.OnBombPressed += HandleBombPressed;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.OnBombPressed -= HandleBombPressed;
        }
    }

    private void HandleBombPressed()
    {
        if (GameManager.Instance == null || GameManager.Instance.State.Lives <= 0)
        {
            return;
        }

        if (!GameManager.Instance.TryUseBomb())
        {
            return;
        }

        Detonate();
    }

    private void Detonate()
    {
        VfxSpawner.BombShockwave(transform.position);

        // Include inactive=false so only live on-screen targets are hit.
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is not IDamageable damageable || !damageable.IsAlive)
            {
                continue;
            }

            if (damageable.IsBoss)
            {
                float percentDamage = damageable.MaxHealth * bossDamagePercent;
                damageable.ApplyDamage(percentDamage);
            }
            else
            {
                // Normal chickens / eggs / asteroids die through the shared damage path.
                damageable.ApplyDamage(normalKillDamage);
            }
        }
    }
}
